﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using richClosure.Packets;
using richClosure.Packets.Application_Layer;
using richClosure.Packets.Internet_Layer;
using richClosure.Packets.Session_Layer;
using richClosure.Packets.Transport_Layer;
using richClosure.Properties;

namespace richClosure.ViewModels
{
    public class PacketViewModel : INotifyPropertyChanged, IViewModel
    {
        private string _hexData;
        public string HexData
        {
            get => _hexData;
            private set
            {
                _hexData = value;
                OnPropertyChanged(nameof(HexData));
            }
        }

        private string _asciiData;
        public string AsciiData
        {
            get => _asciiData;
            private set
            {
                _asciiData = value;
                OnPropertyChanged(nameof(AsciiData));
            }
        }

        private IPacket _sourcePacket;
        public IPacket SourcePacket
        {
            get => _sourcePacket;
            set
            {
                if (_sourcePacket == value) return;
                _sourcePacket = value;
                GetAsciiPacketData(_sourcePacket);
                GetHexPacketData(_sourcePacket);
                FillPacketTreeView(_sourcePacket, TreeViewItems);
            }
        }

        private List<TreeViewItem> _treeViewItems;
        public List<TreeViewItem> TreeViewItems
        {
            get => _treeViewItems;
            private set
            {
                _treeViewItems = value;
                OnPropertyChanged(nameof(TreeViewItems));
            }
        }

        public void GetAsciiPacketData(IPacket packet)
        {
            string[] hexString = packet.PacketData.Split('-');

            string tempAscii = string.Empty;

            foreach (string hexval in hexString)
            {
                uint decval = Convert.ToUInt32(hexval, 16);

                if (decval >= 33 && decval <= 126)
                {
                    char ch = Convert.ToChar(decval);
                    tempAscii += ch;
                }
                else
                {
                    tempAscii += ".";
                }
            }

            string resString = string.Empty;

            for (int x = 1; x <= hexString.Length; x++)
            {
                if (x % 16 == 0 && x != 0)
                {
                    resString += tempAscii[x - 1] + "\n";
                }
                else if (x % 8 == 0 && x != 0)
                {
                    resString += tempAscii[x - 1] + "   ";
                }
                else
                {
                    resString += tempAscii[x - 1] + " ";
                }
            }

            AsciiData = resString;
        }

        public void GetHexPacketData(IPacket packet)
        {
            string[] hexTempStr = packet.PacketData.Split('-');
            string resString = string.Empty;

            for (int x = 1; x <= hexTempStr.Length; x++)
            {
                if (x % 16 == 0 && x != 0)
                {
                    resString += hexTempStr[x - 1] + "\n";
                }
                else if (x % 8 == 0 && x != 0)
                {
                    resString += hexTempStr[x - 1] + "   ";
                }
                else
                {
                    resString += hexTempStr[x - 1] + " ";
                }
            }

            HexData = resString;
        }

        private void FillPacketTreeView(IPacket packet, List<TreeViewItem> treeViewItems)
        {
            IpPacket pac = packet as IpPacket;

            FillFrameTreeView(pac, treeViewItems);

            FillIpTreeView(pac, treeViewItems);
            FillIpProtocolTreeView(pac, treeViewItems);

            if (packet.IpAppProtocol != AppProtocolEnum.NoAppProtocol)
            {
                FillAppProtocolTreeView(pac, treeViewItems);
            }
        }

        private void FillFrameTreeView(IPacket packet, List<TreeViewItem> treeViewItems)
        {
            TreeViewItem frameItem = new TreeViewItem { Header = "Frame " + packet.PacketId + ", Time Captured: " + packet.TimeDateCaptured };
            treeViewItems.Add(frameItem);
        }

        private void FillIpTreeView(IPacket packet, List<TreeViewItem> treeViewItems)
        {
            switch (packet.IpVersion)
            {
                case 4:
                    IpPacket ip4Packet = packet as IpPacket;

                    TreeViewItem ipItem = new TreeViewItem { Header = "IP4 Layer, " + "Dest: " + ip4Packet.Ip4Adrs["dst"] + ", Src: " + ip4Packet.Ip4Adrs["src"] };

                    ipItem.Items.Add(new TreeViewItem { Header = "Version: " + ip4Packet.IpVersion });
                    ipItem.Items.Add(new TreeViewItem { Header = "Header Length: " + ip4Packet.Ip4HeaderLength });
                    ipItem.Items.Add(new TreeViewItem { Header = "Protocol: " + ip4Packet.IpProtocol });
                    ipItem.Items.Add(new TreeViewItem { Header = "Dest. Address: " + ip4Packet.Ip4Adrs["dst"] });
                    ipItem.Items.Add(new TreeViewItem { Header = "Src. Address: " + ip4Packet.Ip4Adrs["src"] });
                    ipItem.Items.Add(new TreeViewItem { Header = "DSCP: " + ip4Packet.Ip4Dscp });
                    ipItem.Items.Add(new TreeViewItem { Header = "Total Length: " + ip4Packet.IpTotalLength });
                    ipItem.Items.Add(new TreeViewItem { Header = "Identification: " + ip4Packet.Ip4Identification });
                    ipItem.Items.Add(new TreeViewItem { Header = "Offset: " + ip4Packet.Ip4Offset });

                    TreeViewItem ipFlagsItem = new TreeViewItem();
                    ipFlagsItem.Items.Add(new TreeViewItem { Header = "DF - " + ip4Packet.Ip4Flags.Df });
                    ipFlagsItem.Items.Add(new TreeViewItem { Header = "MF - " + ip4Packet.Ip4Flags.Mf });
                    ipFlagsItem.Items.Add(new TreeViewItem { Header = "Res. - " + ip4Packet.Ip4Flags.Res });
                    ipItem.Items.Add(ipFlagsItem);

                    ipItem.Items.Add(new TreeViewItem { Header = "TTL: " + ip4Packet.Ip4TimeToLive });
                    ipItem.Items.Add(new TreeViewItem { Header = "Header Checksum: " + ip4Packet.Ip4HeaderChecksum });
                    treeViewItems.Add(ipItem);
                    break;

                case 6:
                    IpPacket ip6Packet = packet as IpPacket;
                    TreeViewItem ip6Item = new TreeViewItem { Header = "IPv6 Layer, " + "Dest: " + ip6Packet.Ip6Adrs["dst"] + ", Src: " + ip6Packet.Ip6Adrs["src"] };

                    ip6Item.Items.Add(new TreeViewItem { Header = "Version: " + ip6Packet.IpVersion });
                    ip6Item.Items.Add(new TreeViewItem { Header = "Traffic Class: " + ip6Packet.Ip6TrafficClass });
                    ip6Item.Items.Add(new TreeViewItem { Header = "Flow Label: " + ip6Packet.Ip6FlowLabel });
                    ip6Item.Items.Add(new TreeViewItem { Header = "Payload Length: " + ip6Packet.IpTotalLength });
                    ip6Item.Items.Add(new TreeViewItem { Header = "Next Header: " + ip6Packet.IpProtocol });
                    ip6Item.Items.Add(new TreeViewItem { Header = "Hop Limit: " + ip6Packet.Ip6HopLimit });
                    ip6Item.Items.Add(new TreeViewItem { Header = "Src. Adr.: " + ip6Packet.Ip6Adrs["dst"] });
                    ip6Item.Items.Add(new TreeViewItem { Header = "Dest. Adr.: " + ip6Packet.Ip6Adrs["src"] });
                    treeViewItems.Add(ip6Item);
                    break;
            }

        }

        private void FillIpProtocolTreeView(IPacket packet, List<TreeViewItem> treeViewItems)
        {
            TreeViewItem ipProtocolItem = new TreeViewItem { Header = packet.IpProtocol.ToString() };
            treeViewItems.Add(ipProtocolItem);

            switch (packet.IpProtocol)
            {
                case IpProtocolEnum.Tcp:
                    FillTcpTreeView(ipProtocolItem, packet);
                    break;

                case IpProtocolEnum.Icmp:
                    FillIcmpTreeView(ipProtocolItem, packet);
                    break;

                case IpProtocolEnum.Udp:
                    FillUdpTreeView(ipProtocolItem, packet);
                    break;
            }
        }

        private void FillTcpTreeView(TreeViewItem ipProtocolItem, IPacket packet)
        {

            TcpPacket tcpPacket = packet as TcpPacket;

            ipProtocolItem.Header += ", Dest. Port: " + tcpPacket.TcpPorts["dst"] +
                                     ", Src. Port: " + tcpPacket.TcpPorts["src"];

            ipProtocolItem.Items.Add(new TreeViewItem { Header = "Dest. port: " + tcpPacket.TcpPorts["dst"] });
            ipProtocolItem.Items.Add(new TreeViewItem { Header = "Src. port: " + tcpPacket.TcpPorts["src"] });
            ipProtocolItem.Items.Add(new TreeViewItem { Header = "Seq. Number: " + tcpPacket.TcpSequenceNumber });
            ipProtocolItem.Items.Add(new TreeViewItem { Header = "Ack Number: " + tcpPacket.TcpAckNumber });
            ipProtocolItem.Items.Add(new TreeViewItem { Header = "Urg. Pointer: " + tcpPacket.TcpUrgentPointer });
            ipProtocolItem.Items.Add(new TreeViewItem { Header = "Data Offset: " + tcpPacket.TcpDataOffset });
            ipProtocolItem.Items.Add(new TreeViewItem { Header = "Window Size: " + tcpPacket.TcpWindowSize });
            ipProtocolItem.Items.Add(new TreeViewItem { Header = "Checksum: " + tcpPacket.TcpChecksum });

            TreeViewItem tcpFlagsItem = new TreeViewItem { Header = "TCP Flags" };
            tcpFlagsItem.Items.Add(new TreeViewItem { Header = "FIN - " + tcpPacket.TcpFlags.Fin.IsSet.ToString() });
            tcpFlagsItem.Items.Add(new TreeViewItem { Header = "SYN - " + tcpPacket.TcpFlags.Syn.IsSet.ToString() });
            tcpFlagsItem.Items.Add(new TreeViewItem { Header = "RST - " + tcpPacket.TcpFlags.Rst.IsSet.ToString() });
            tcpFlagsItem.Items.Add(new TreeViewItem { Header = "PSH - " + tcpPacket.TcpFlags.Psh.IsSet.ToString() });
            tcpFlagsItem.Items.Add(new TreeViewItem { Header = "ACK - " + tcpPacket.TcpFlags.Ack.IsSet.ToString() });
            tcpFlagsItem.Items.Add(new TreeViewItem { Header = "URG - " + tcpPacket.TcpFlags.Urg.IsSet.ToString() });
            tcpFlagsItem.Items.Add(new TreeViewItem { Header = "ECE - " + tcpPacket.TcpFlags.Ece.IsSet.ToString() });
            tcpFlagsItem.Items.Add(new TreeViewItem { Header = "CWR - " + tcpPacket.TcpFlags.Cwr.IsSet.ToString() });
            tcpFlagsItem.Items.Add(new TreeViewItem { Header = "NS - " + tcpPacket.TcpFlags.Ns.IsSet.ToString() });

            ipProtocolItem.Items.Add(tcpFlagsItem);
        }

        private void FillIcmpTreeView(TreeViewItem ipProtocolItem, IPacket packet)
        {
            IcmpPacket icmpPacket = packet as IcmpPacket;
            ipProtocolItem.Header += ", Type: " + icmpPacket.IcmpType + ", Code: " + icmpPacket.IcmpCode;

            ipProtocolItem.Items.Add(new TreeViewItem { Header = "ICMP Type: " + icmpPacket.IcmpType + ", " + (IcmpTypeEnum)icmpPacket.IcmpType });

            switch (icmpPacket.IcmpType)
            {
                case 3:
                    ipProtocolItem.Items.Add(new TreeViewItem
                    {
                        Header = "ICMP Code: " + icmpPacket.IcmpCode + ", " +
                        (DestinationUnreachableCodeEnum)icmpPacket.IcmpCode
                    });
                    break;

                case 5:
                    ipProtocolItem.Items.Add(new TreeViewItem
                    {
                        Header = "ICMP Code: " + icmpPacket.IcmpCode + ", " +
                        (RedirectMessageCodeEnum)icmpPacket.IcmpCode
                    });
                    break;

                case 11:
                    ipProtocolItem.Items.Add(new TreeViewItem
                    {
                        Header = "ICMP Code: " + icmpPacket.IcmpCode + ", " +
                        (TimeExceededCodeEnum)icmpPacket.IcmpCode
                    });
                    break;

                case 12:
                    ipProtocolItem.Items.Add(new TreeViewItem
                    {
                        Header = "ICMP Code: " + icmpPacket.IcmpCode + ", " +
                        (ParameterProblemCodeEnum)icmpPacket.IcmpCode
                    });
                    break;

                default:
                    ipProtocolItem.Items.Add(new TreeViewItem
                    {
                        Header = "ICMP Code: " + icmpPacket.IcmpCode + ", " +
                        (IcmpTypeEnum)icmpPacket.IcmpType
                    });
                    break;
            }

            ipProtocolItem.Items.Add(new TreeViewItem { Header = "ICMP Checksum: " + icmpPacket.IcmpChecksum });
            ipProtocolItem.Items.Add(new TreeViewItem { Header = "ICMP Rest: " + icmpPacket.IcmpRest });
        }

        private void FillUdpTreeView(TreeViewItem ipProtocolItem, IPacket packet)
        {
            UdpPacket udpPacket = packet as UdpPacket;

            ipProtocolItem.Header += ", Dest. Port: " + udpPacket.UdpPorts["dst"] +
                                     ", Src. Port: " + udpPacket.UdpPorts["src"];

            ipProtocolItem.Items.Add(new TreeViewItem { Header = "Dest. Port: " + udpPacket.UdpPorts["dst"] });
            ipProtocolItem.Items.Add(new TreeViewItem { Header = "Src. Port: " + udpPacket.UdpPorts["src"] });
            ipProtocolItem.Items.Add(new TreeViewItem { Header = "Length: " + udpPacket.UdpLength });

            ipProtocolItem.Items.Add(new TreeViewItem { Header = "Checksum: " + udpPacket.UdpChecksum });
        }

        private void FillAppProtocolTreeView(IPacket packet, List<TreeViewItem> treeViewItems)
        {
            TreeViewItem appProtocolItem = new TreeViewItem { Header = packet.IpAppProtocol.ToString() };
            treeViewItems.Add(appProtocolItem);

            switch (packet.IpAppProtocol)
            {
                case AppProtocolEnum.Dns:
                    FillDnsProtocolTreeView(appProtocolItem, packet);
                    break;

                case AppProtocolEnum.Dhcp:
                    FillDhcpProtocolTreeView(appProtocolItem, packet);
                    break;

                case AppProtocolEnum.Http:
                    FillHttpProtocolTreeView(appProtocolItem, packet);
                    break;

                case AppProtocolEnum.Tls:
                    FillTlsProtocolTreeView(appProtocolItem, packet);
                    break;
            }
        }

        private void FillDnsProtocolTreeView(TreeViewItem appProtocolItem, IPacket packet)
        {
            if (packet.IpProtocol == IpProtocolEnum.Tcp)
            {
                DnsTcpPacket pac = packet as DnsTcpPacket;
                appProtocolItem.Items.Add(new TreeViewItem { Header = "Identification: " + pac.DnsIdentification });
                appProtocolItem.Items.Add(new TreeViewItem { Header = "QR: " + pac.DnsQr });
                appProtocolItem.Items.Add(new TreeViewItem { Header = "Opcode: " + pac.DnsOpcode });
                appProtocolItem.Items.Add(new TreeViewItem { Header = "Rcode: " + pac.DnsRcode });
                appProtocolItem.Items.Add(new TreeViewItem { Header = "Flags: " + pac.DnsFlags });
                appProtocolItem.Items.Add(new TreeViewItem { Header = "Num. of Questions: " + pac.DnsQuestions });
                appProtocolItem.Items.Add(new TreeViewItem { Header = "Num. of Answers: " + pac.DnsAnswersRr });
                appProtocolItem.Items.Add(new TreeViewItem { Header = "Num. of Auth.: " + pac.DnsAuthRr });
                appProtocolItem.Items.Add(new TreeViewItem { Header = "Num. of Add.: " + pac.DnsAdditionalRr });

                TreeViewItem questionItem = new TreeViewItem { Header = "Questions: " + pac.DnsQuestions };
                appProtocolItem.Items.Add(questionItem);

                TreeViewItem answersItem = new TreeViewItem { Header = "Answers: " + pac.DnsAnswersRr };
                appProtocolItem.Items.Add(answersItem);

                TreeViewItem authItem = new TreeViewItem { Header = "Auth.: " + pac.DnsAuthRr };
                appProtocolItem.Items.Add(authItem);

                TreeViewItem addItem = new TreeViewItem { Header = "Add.: " + pac.DnsAdditionalRr };
                appProtocolItem.Items.Add(addItem);

                for (int i = 0; i < pac.DnsQuestions; i++)
                {
                    FillDnsQuestionTreeView(questionItem, pac.DnsQuerryList[i], i);
                }

                for (int i = 0; i < pac.DnsAnswersRr; i++)
                {
                    FillDnsRecordTreeView(answersItem, pac.DnsAnswerList[i], i);
                }

                for (int i = 0; i < pac.DnsAuthRr; i++)
                {
                    FillDnsRecordTreeView(authItem, pac.DnsAuthList[i], i);
                }

                for (int i = 0; i < pac.DnsAdditionalRr; i++)
                {
                    FillDnsRecordTreeView(addItem, pac.DnsAdditionalList[i], i);
                }

            }
        }

        private void FillDnsRecordTreeView(TreeViewItem recordItem, DnsRecord record, int num)
        {
            TreeViewItem recItem = new TreeViewItem { Header = "Record " + num.ToString() };
            recItem.Items.Add(new TreeViewItem { Header = "Name: " + record.DnsRecordName });
            recItem.Items.Add(new TreeViewItem { Header = "Type: " + record.DnsRecordType });
            recItem.Items.Add(new TreeViewItem { Header = "Class: " + record.DnsRecordClass });
            recItem.Items.Add(new TreeViewItem { Header = "TTL: " + record.DnsTimeToLive });
            recItem.Items.Add(new TreeViewItem { Header = "Data Length: " + record.DnsRdataLength });

            TreeViewItem dataItem = new TreeViewItem { Header = "Data" };
            dataItem.Items.Add(new TreeViewItem { Header = record.DnsRdata });
            recItem.Items.Add(dataItem);

            recordItem.Items.Add(recItem);
        }
        private void FillDnsQuestionTreeView(TreeViewItem questionItem, DnsQuery query, int num)
        {
            TreeViewItem qItem = new TreeViewItem { Header = "Question " + num.ToString() };
            qItem.Items.Add(new TreeViewItem { Header = "Name: " + query.DnsQueryName });
            qItem.Items.Add(new TreeViewItem { Header = "Type: " + query.DnsQueryType });
            qItem.Items.Add(new TreeViewItem { Header = "Class: " + query.DnsQueryClass });

            questionItem.Items.Add(qItem);

        }

        private void FillDhcpProtocolTreeView(TreeViewItem appProtocolItem, IPacket packet)
        {
            DhcpPacket dhcpPacket = packet as DhcpPacket;

            appProtocolItem.Items.Add(new TreeViewItem { Header = "Opcode: " + dhcpPacket.DhcpOpcode });
            appProtocolItem.Items.Add(new TreeViewItem { Header = "Hardware Type: " + dhcpPacket.DhcpHardType });
            appProtocolItem.Items.Add(new TreeViewItem { Header = "Hardware Adr. Length: " + dhcpPacket.DhcpHardAdrLength });
            appProtocolItem.Items.Add(new TreeViewItem { Header = "Hops: " + dhcpPacket.DhcpHopCount });
            appProtocolItem.Items.Add(new TreeViewItem { Header = "Transaction ID: " + dhcpPacket.DhcpTransactionId });
            appProtocolItem.Items.Add(new TreeViewItem { Header = "Seconds: " + dhcpPacket.DhcpNumOfSeconds });
            appProtocolItem.Items.Add(new TreeViewItem { Header = "Flags: " + dhcpPacket.DhcpFlags });
            appProtocolItem.Items.Add(new TreeViewItem { Header = "Client IP: " + dhcpPacket.DhcpClientIpAdr });
            appProtocolItem.Items.Add(new TreeViewItem { Header = "Your IP: " + dhcpPacket.DhcpYourIpAdr });
            appProtocolItem.Items.Add(new TreeViewItem { Header = "Gateway IP: " + dhcpPacket.DhcpGatewayIpAdr });
            appProtocolItem.Items.Add(new TreeViewItem { Header = "Client Hardware Adr.: " + dhcpPacket.DhcpClientHardAdr });
            appProtocolItem.Items.Add(new TreeViewItem { Header = "Server Name: " + dhcpPacket.DhcpServerName });
            appProtocolItem.Items.Add(new TreeViewItem { Header = "Boot Filename: " + dhcpPacket.DhcpBootFilename });
        }

        private void FillHttpProtocolTreeView(TreeViewItem appProtocolItem, IPacket packet)
        {
            HttpPacket pac = packet as HttpPacket;

            foreach (KeyValuePair<string, string> entry in pac.HttpFieldsDict)
            {
                appProtocolItem.Items.Add(new TreeViewItem { Header = entry.Key + ": " + entry.Value });
            }
        }

        private void FillTlsProtocolTreeView(TreeViewItem appProtocolItem, IPacket packet)
        {
            TlsPacket pac = packet as TlsPacket;
            appProtocolItem.Items.Add(new TreeViewItem { Header = "Content Type: " + pac.TlsType });
            appProtocolItem.Items.Add(new TreeViewItem { Header = "Version: " + pac.TlsVersion });
            appProtocolItem.Items.Add(new TreeViewItem { Header = "Data Length: " + pac.TlsDataLength });
            appProtocolItem.Items.Add(new TreeViewItem { Header = "Encrypted Data: " + pac.TlsEncryptedData });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}