﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using PacketSniffer.Packets;
using PacketSniffer.Packets.Application_Layer;

namespace PacketSniffer.Factories.ApplicationFactories
{
    internal class DnsPacketByteFactory : IAbstractFactory
    {
        private readonly BinaryReader _binaryReader;
        private readonly Dictionary<string, object> _valueDictionary;

        public DnsPacketByteFactory(BinaryReader binaryReader, Dictionary<string, object> valueDictionary)
        {
            _binaryReader = binaryReader;
            _valueDictionary = valueDictionary;
        }

        public IPacket CreatePacket()
        {        
            ReadPacketDataFromStream();

            IPacket dnsPacket;
            switch ((IpProtocolEnum)_valueDictionary["IpProtocl"])
            {
                case IpProtocolEnum.Tcp:
                    dnsPacket = new DnsTcpPacket(_valueDictionary);
                    break;

                case IpProtocolEnum.Udp:
                    dnsPacket = new DnsUdpPacket(_valueDictionary);
                    break;

                default:
                    throw new ArgumentException();
            }

            return dnsPacket;
        }

        private void ReadPacketDataFromStream()
        {
            _valueDictionary["AppProtocol"] = AppProtocolEnum.Dns;
            _valueDictionary["PacketDisplayedProtocol"] = "DNS";

            _valueDictionary["DnsIdentification"] = (UInt16)IPAddress.NetworkToHostOrder(_binaryReader.ReadInt16());
            UInt16 dnsFlagsAndCodes = (UInt16)IPAddress.NetworkToHostOrder(_binaryReader.ReadInt16());

            _valueDictionary["DnsQuestions"] = (UInt16)IPAddress.NetworkToHostOrder(_binaryReader.ReadInt16());
            _valueDictionary["DnsAnswersRR"] = (UInt16)IPAddress.NetworkToHostOrder(_binaryReader.ReadInt16());
            _valueDictionary["DnsAuthRR"] = (UInt16)IPAddress.NetworkToHostOrder(_binaryReader.ReadInt16());
            _valueDictionary["DnsAdditionalRR"] = (UInt16)IPAddress.NetworkToHostOrder(_binaryReader.ReadInt16());

            string dnsFlagsBinStr = Convert.ToString(dnsFlagsAndCodes, 2);

            while (dnsFlagsBinStr.Length != 16)
            {
                dnsFlagsBinStr = dnsFlagsBinStr.Insert(0, "0");
            }

            string dnsQr;

            dnsQr = dnsFlagsBinStr[0] == 0 ? "Query" : "Response";

            _valueDictionary["DnsQR"] = dnsQr;

            _valueDictionary["DnsOpcode"] = Convert.ToByte(dnsFlagsBinStr.Substring(1, 4), 10);

            _valueDictionary["DnsFLags"] = GetDnsFlags(dnsFlagsBinStr);

            _valueDictionary["DnsRcode"] = Convert.ToByte(dnsFlagsBinStr.Substring(12, 4), 10);

            List<DnsQuery> dnsQueries = new List<DnsQuery>();

            for (int i = 1; i <= (int)_valueDictionary["DnsQuestions"]; i++)
            {
                StringBuilder stringBuilder = new StringBuilder();
                bool reading = true;

                while (reading)
                {
                    byte ch = _binaryReader.ReadByte();

                    if (ch >= 33 && ch <= 126)
                    {
                        stringBuilder.Append(Convert.ToChar(ch));
                    }
                    else
                    {
                        stringBuilder.Append(".");
                    }

                    if (ch == 0x0)
                    {
                        reading = false;
                    }
                }
                string dnsQueryName = stringBuilder.ToString();
                UInt16 dnsQueryType = (UInt16)IPAddress.NetworkToHostOrder(_binaryReader.ReadInt16());
                UInt16 dnsQueryClass = (UInt16)IPAddress.NetworkToHostOrder(_binaryReader.ReadInt16());

                dnsQueries.Add(new DnsQuery
                {
                    DnsQueryName = dnsQueryName,
                    DnsQueryType = (DnsTypeEnum)dnsQueryType,
                    DnsQueryClass = (DnsClassEnum)dnsQueryClass
                });
            }

            List<DnsRecord> dnsAnswers = new List<DnsRecord>();
            ParseDnsRecordHeader(dnsAnswers, (int)_valueDictionary["DnsAnswersRR"], dnsQueries[dnsQueries.Count - 1].DnsQueryName);

            List<DnsRecord> dnsAuths = new List<DnsRecord>();
            ParseDnsRecordHeader(dnsAuths, (int)_valueDictionary["DnsAuthRR"], dnsQueries[dnsQueries.Count - 1].DnsQueryName);

            List<DnsRecord> dnsAdditionals = new List<DnsRecord>();
            ParseDnsRecordHeader(dnsAdditionals, (int)_valueDictionary["DnsAdditionalRR"], dnsQueries[dnsQueries.Count - 1].DnsQueryName);

            _valueDictionary["DnsAnswers"] = dnsAnswers;
            _valueDictionary["DnsAuth"] = dnsAuths;
            _valueDictionary["DnsAdditionals"] = dnsAdditionals;
        }

        private DnsFlags GetDnsFlags(string flagsBinStr)
        {
            string dnsFlags = flagsBinStr.Substring(5, 7);
            int dnsFlagsInt = Convert.ToInt32(dnsFlags);
            DnsFlags dnsFlagsObj = new DnsFlags();

            if ((dnsFlagsInt & 1) != 0)
            {
                dnsFlagsObj.Cd.IsSet = true;
            }
            if ((dnsFlagsInt & 2) != 0)
            {
                dnsFlagsObj.Ad.IsSet = true;
            }
            if ((dnsFlagsInt & 4) != 0)
            {
                dnsFlagsObj.Z.IsSet = true;
            }
            if ((dnsFlagsInt & 8) != 0)
            {
                dnsFlagsObj.Ra.IsSet = true;
            }
            if ((dnsFlagsInt & 16) != 0)
            {
                dnsFlagsObj.Ra.IsSet = true;
            }
            if ((dnsFlagsInt & 32) != 0)
            {
                dnsFlagsObj.Tc.IsSet = true;
            }
            if ((dnsFlagsInt & 64) != 0)
            {
                dnsFlagsObj.Aa.IsSet = true;
            }

            return dnsFlagsObj;
        }

        private void ParseDnsRecordHeader(List<DnsRecord> recordList, int recordsCount, string queryName)
        {
            for (int procRecIndex = 1; procRecIndex <= recordsCount; procRecIndex++)
            {
                string dnsRecordName;

                byte[] byteChar = _binaryReader.ReadBytes(2);

                if (procRecIndex == 1)
                {
                    dnsRecordName = queryName;
                }
                else
                {
                    dnsRecordName = recordList[procRecIndex - 2].DnsRecordType == DnsTypeEnum.Cname ? recordList[procRecIndex - 2].DnsRdata : recordList[procRecIndex - 2].DnsRecordName;
                }

                DnsTypeEnum dnsRecordType = (DnsTypeEnum)IPAddress.NetworkToHostOrder(_binaryReader.ReadInt16());
                DnsClassEnum dnsRecordClass = (DnsClassEnum)IPAddress.NetworkToHostOrder(_binaryReader.ReadInt16());
                UInt32 dnsRecordTtl = (UInt32)IPAddress.NetworkToHostOrder(_binaryReader.ReadInt32());
                UInt16 dnsRDataLength = (UInt16)IPAddress.NetworkToHostOrder(_binaryReader.ReadInt16());

                byte[] byteData = _binaryReader.ReadBytes(dnsRDataLength);
                StringBuilder dataBuilder = new StringBuilder();

                switch (dnsRecordType)
                {
                    case DnsTypeEnum.A:

                        foreach (byte b in byteData)
                        {
                            dataBuilder.Append(b.ToString() + ".");
                        }
                        break;

                    default:
                        foreach (byte b in byteData)
                        {
                            if (b >= 33 && b <= 126)
                            {
                                dataBuilder.Append(Convert.ToChar(b));
                            }
                            else
                            {
                                dataBuilder.Append(".");
                            }
                        }
                        break;
                }

                recordList.Add(new DnsRecord
                {
                    DnsRecordName = dnsRecordName,
                    DnsRecordType = dnsRecordType,
                    DnsRecordClass = dnsRecordClass,
                    DnsTimeToLive = dnsRecordTtl,
                    DnsRdataLength = dnsRDataLength,
                    DnsRdata = dataBuilder.ToString()
                });
            }
        }
    }
}
