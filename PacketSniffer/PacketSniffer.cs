﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PacketSniffer.Factories;
using PacketSniffer.Packets;
using PacketSniffer.Socket;

namespace PacketSniffer
{
    // TODO Clean this up
    public class PacketSniffer : IPacketSniffer
    {
        public ObservableCollection<IPacketFrame> Packets { get; }
        public IObservable<NetworkInterface> SelectedNetworkInterface { get; set; }
        private readonly IPacketQueue _packetQueue;
        private readonly IAbstractFrameFactory _frameFactory;


        private Task _ip6SnifferTask;
        private Task _ip4SnifferTask;
        private readonly Task _dequeueTask;

        private readonly CancellationTokenSource _tokenSource;
        

        public PacketSniffer()
        {
            Packets = new ObservableCollection<IPacketFrame>();
            _packetQueue = new PacketQueue();
            _frameFactory = new PacketFactory();
        
            _tokenSource = new CancellationTokenSource();
            _dequeueTask = new Task(DequeuePacketBuffer, _tokenSource.Token);
        }

        public IEnumerable<NetworkInterface> GetAvailableNetworkInterfaces()
        {
            return NetworkInterface.GetAllNetworkInterfaces();
        }

        public void SniffPackets(NetworkInterface networkInterface)
        {
            ISnifferSocket socketIp4 = new SnifferSocket(AddressFamily.InterNetwork,
                SocketType.Raw,
                ProtocolType.IP,
                networkInterface,
                _packetQueue);
            
            ISnifferSocket socketIp6 = new SnifferSocket(AddressFamily.InterNetworkV6,
                SocketType.Raw,
                ProtocolType.IP,
                networkInterface,
                _packetQueue);
            
            _ip4SnifferTask = new Task(socketIp4.ReceivePacket, _tokenSource.Token);
            _ip6SnifferTask = new Task(socketIp6.ReceivePacket, _tokenSource.Token);
            _ip6SnifferTask.Start();
            _ip4SnifferTask.Start();
            _dequeueTask.Start();
        }

        public void StopSniffing()
        {
            _tokenSource.Cancel();
            _packetQueue.Clear();
        }

        private void DequeuePacketBuffer()
        {
            var buffer = _packetQueue.DequeuePacket();

            IPacketFrame packet = _frameFactory.CreatePacketFrame(buffer);
            Packets.Add(packet);
        }
    }
}