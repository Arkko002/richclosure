﻿using richClosure.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace richClosure.ViewModels
{
    class PacketSnifferViewModel
    {
        private PacketSniffer _packetSniffer;
        public ObservableCollection<IPacket> ModelCollection { get; private set; }

        public NetworkInterface NetworkInterface { get; set; }

        public ICommand StartSniffingCommand { get; private set; }
        public ICommand StopSniffingCommand { get; private set; }


        public PacketSnifferViewModel(ObservableCollection<IPacket> modelCollection)
        {
            ModelCollection = modelCollection;

            StartSniffingCommand = new RelayCommand(x => StartSniffingPackets(), x => !_packetSniffer.IsWorking);
            StopSniffingCommand = new RelayCommand(x => StopSniffingPackets(), x => _packetSniffer.IsWorking);
        }

        private void StartSniffingPackets()
        {
            _packetSniffer = new PacketSniffer(NetworkInterface, ModelCollection);
            _packetSniffer.SniffPackets();
        }

        private void StopSniffingPackets()
        {
            _packetSniffer.StopWorking();
        }
    }
}
