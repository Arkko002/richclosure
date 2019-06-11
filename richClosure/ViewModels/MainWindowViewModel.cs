﻿using System.Windows.Input;
using richClosure.Commands;
using richClosure.Views;
using richClosure.Window_Services;

namespace richClosure.ViewModels
{
    public class MainWindowViewModel : IViewModel
    {
        private readonly IWindowManager _windowManager;
        // TODO Refactor, maybe seprate class?
        public MainWindowViewModel(
            PacketCollectionViewModel packetCollectionViewModel,
            PacketFilterViewModel packetFilterViewModel,
            PacketSnifferViewModel packetSnifferViewModel,
            IWindowManager windowManager)
        {
            PacketCollectionViewModel = packetCollectionViewModel;
            PacketFilterViewModel = packetFilterViewModel;
            PacketSnifferViewModel = packetSnifferViewModel;

            _windowManager = windowManager;

            ShowAdapterSelectionCommand = new RelayCommand(x => StartSniffing(), x => true);
        }

        public PacketCollectionViewModel PacketCollectionViewModel { get; set; }

        public PacketFilterViewModel PacketFilterViewModel { get; set; }

        public PacketSnifferViewModel PacketSnifferViewModel { get; set; }

        public ICommand ShowAdapterSelectionCommand { get; }

        public void StartSniffing()
        {
            _windowManager.ShowWindow(typeof(InterfaceSelectionWindow), new InterfaceSelectionViewModel(PacketSnifferViewModel, _windowManager));
        }

        public void StopSniffing()
        {
            if (PacketSnifferViewModel.StopSniffingCommand.CanExecute(null))
            {
                PacketSnifferViewModel.StopSniffingCommand.Execute(null);
            }
        }

    }
}
