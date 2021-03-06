﻿using System;
using System.Windows.Input;
using Transliterator.UI.Windows.Controls;

namespace Transliterator.UI.Windows.Views
{
    public class MainViewModel : ViewModelBase
    {
        #region Data members
        private bool _isEnabled = false;
        #endregion

        public MainViewModel()
        {
            HotkeyViewModel = new HotkeySelectorViewModel();
            HotkeyViewModel.HotkeyTriggered += HotkeyViewModel_HotkeyTriggered;
            EnableTranslitCommand = new DelegateCommand(_ => TurnOn());
            DisableTranslitCommand = new DelegateCommand(_ => TurnOff());

            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Russian.kbd");
            EngineAdapter.RegisterKeyboardLayout(path);
            EngineAdapter.SetCurrentLayout(0);
        }

        public class StateChangedEventArgs : EventArgs
        {
            public StateChangedEventArgs(bool isEnabled)
            {
                IsEnabled = isEnabled;
            }

            public bool IsEnabled { get; private set; }
        }

        public event EventHandler<StateChangedEventArgs> StateChanged;

        public ICommand EnableTranslitCommand { get; private set; }
        public ICommand DisableTranslitCommand { get; private set; }

        private void HotkeyViewModel_HotkeyTriggered(object sender, EventArgs e)
        {
            if (EngineAdapter.IsTransliterationEnabled())
            {
                TurnOff();
            }
            else
            {
                TurnOn();
            }
        }

        public HotkeySelectorViewModel HotkeyViewModel { get; private set; }

        public bool LoadOnStartup
        {
            get { return GlobalConfiguration.Instance.LoadOnStartup; }
            set 
            {
                if (value != GlobalConfiguration.Instance.LoadOnStartup)
                {
                    GlobalConfiguration.Instance.LoadOnStartup = value;
                    GlobalConfiguration.Instance.Save();
                    OnPropertyChanged("LoadOnStartup");
                }
            }
        }

        public bool MinimizeOnClose
        {
            get { return GlobalConfiguration.Instance.MinimizeOnClose; }
            set
            {
                if (value != GlobalConfiguration.Instance.MinimizeOnClose)
                {
                    GlobalConfiguration.Instance.MinimizeOnClose = value;
                    GlobalConfiguration.Instance.Save();
                    OnPropertyChanged("MinimizeOnClose");
                }
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(() => IsEnabled, ref _isEnabled, value); }
        }

        private void TurnOn()
        {
            EngineAdapter.EnableTransliteration();
            IsEnabled = true;
            RaiseStateChanged(true);
        }

        private void TurnOff()
        {
            EngineAdapter.DisableTransliteration();
            IsEnabled = false;
            RaiseStateChanged(false);
        }

        private void RaiseStateChanged(bool isEnabled)
        {
            if (StateChanged != null)
            {
                StateChanged(this, new StateChangedEventArgs(isEnabled));
            }
        }
    }
}
