﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using richClosure.Packets;
using richClosure.Properties;

namespace richClosure.ViewModels
{
    public class PacketCollectionViewModel : INotifyPropertyChanged, IViewModel
    {
        public ObservableCollection<PacketViewModel> PacketObservableCollection { get; }
        public ObservableCollection<IPacket> ModelCollection { get; }

        public PacketViewModel SelectedPacket { get; private set; }

        private int _totalPacketCount;
        public int TotalPacketCount
        {
            get => _totalPacketCount;
            private set
            {
                _totalPacketCount = value;
                OnPropertyChanged(nameof(TotalPacketCount));
            }
        }

        private int _shownPacketCount;
        public int ShownPacketCount
        {
            get => _shownPacketCount;
            private set
            {
                _shownPacketCount = value;
                OnPropertyChanged(nameof(ShownPacketCount));
            }
        }

        public PacketCollectionViewModel(ObservableCollection<IPacket> modelCollection)
        {
            PacketObservableCollection = new ObservableCollection<PacketViewModel>();
            ModelCollection = modelCollection;

            PacketObservableCollection.CollectionChanged += PacketObservableCollection_CollectionChanged;
        }

        private void PacketObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdatePacketCount(sender as ObservableCollection<PacketViewModel>);
        }

        public void ClearPacketList()
        {
            PacketObservableCollection.Clear();
        }

        public void ChangeSelectedPacket(ulong packetId)
        {
            // SelectedPacket = new PacketViewModel(ModelCollection[(int)packetId - 1]);
        }

        private void UpdatePacketCount(ObservableCollection<PacketViewModel> packetCollection)
        {
            TotalPacketCount = packetCollection.Count;
        }

        public void UpdateShownPacketCount(ObservableCollection<PacketViewModel> packetCollection)
        {
            ShownPacketCount = packetCollection.Count;
        }

        public void UpdateShownPacketCount(List<IPacket> packetList)
        {
            ShownPacketCount = packetList.Count;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}