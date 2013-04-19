using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using Transliterator.UI.Windows.Views;

namespace Transliterator.UI.Windows.Controls
{
    public class HotkeySelectorViewModel : ViewModelBase
    {
        #region Data members
        private List<Key> _possibleKeys = new List<Key>();
        private bool _useWindowsKey = false;
        private bool _useShift = false;
        private bool _useControl = false;
        private bool _useAlt = false;
        private Key _selectedVirtualKey = Key.None;
        private bool _isLoading = false;
        private HotKey _hotkey = null;
        #endregion

        public event EventHandler HotkeyTriggered;

        public HotkeySelectorViewModel()
        {
            _possibleKeys.Add(Key.None);
            var possibleKeys = Enum.GetValues(typeof(Key)).Cast<Key>().Where(k => k >= Key.A && k <= Key.Z || k >= Key.F1 && k <= Key.F24);
            _possibleKeys.AddRange(possibleKeys);

            LoadConfiguration();
        }

        public IList<Key> VirtualKeys
        {
            get { return _possibleKeys; }
        }


        public Key SelectedVirtualKey
        {
            get { return _selectedVirtualKey; }
            set 
            {
                if (SetProperty(() => SelectedVirtualKey, ref _selectedVirtualKey, value))
                {
                    ApplyChanges();
                }
            }
        }


        public bool UseShift
        {
            get { return _useShift; }
            set 
            { 
                if(SetProperty(() => UseShift, ref _useShift, value))
                {
                    ApplyChanges();
                }
            }
        }

        public bool UseControl
        {
            get { return _useControl; }
            set 
            { 
                if(SetProperty(() => UseControl, ref _useControl, value))
                {
                    ApplyChanges();
                }
            }
        }

        public bool UseAlt
        {
            get { return _useAlt; }
            set 
            { 
                if(SetProperty(() => UseAlt, ref _useAlt, value))
                {
                    ApplyChanges();
                }
            }
        }

        public bool UseWindowsKey
        {
            get { return _useWindowsKey; }
            set 
            { 
                if(SetProperty(() => UseWindowsKey, ref _useWindowsKey, value))
                {
                    ApplyChanges();
                }
            }
        }

        #region Private methods
        private void ApplyChanges()
        {
            if (!_isLoading)
            {
                KeyModifier modifier = (UseAlt ? KeyModifier.Alt : KeyModifier.None)
                    | (UseShift ? KeyModifier.Shift : KeyModifier.None)
                    | (UseControl ? KeyModifier.Ctrl : KeyModifier.None)
                    | (UseWindowsKey ? KeyModifier.Win : KeyModifier.None);
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Philip Patrick\Transliterator"))
                {
                    key.SetValue("Hotkey", SelectedVirtualKey.ToString());
                    key.SetValue("HotkeyModifier", modifier.ToString());
                    key.Close();
                }

                SetHotkey();
            }
        }

        private void SetHotkey()
        {
            if(_hotkey != null)
            {
                _hotkey.Dispose();
                _hotkey = null;
            }

            if (SelectedVirtualKey != Key.None)
            {
                KeyModifier modifier = (UseAlt ? KeyModifier.Alt : KeyModifier.None)
                    | (UseShift ? KeyModifier.Shift : KeyModifier.None)
                    | (UseControl ? KeyModifier.Ctrl : KeyModifier.None)
                    | (UseWindowsKey ? KeyModifier.Win : KeyModifier.None);
                _hotkey = new HotKey(
                    SelectedVirtualKey,
                    modifier | KeyModifier.NoRepeat,
                    _ => RaiseHotkeyTriggered());
            }
        }

        private void RaiseHotkeyTriggered()
        {
            if (HotkeyTriggered != null)
            {
                HotkeyTriggered(this, EventArgs.Empty);
            }
        }

        private void LoadConfiguration()
        {
            _isLoading = true;
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Philip Patrick\Transliterator"))
                {
                    var s = key.GetValue("Hotkey", Key.None.ToString()) as string;
                    Key selectedKey = Key.None;
                    Enum.TryParse(s, out selectedKey);
                    SelectedVirtualKey = selectedKey;

                    s = key.GetValue("HotkeyModifier", KeyModifier.None) as string;
                    KeyModifier modifier = KeyModifier.None;
                    Enum.TryParse(s, out modifier);
                    UseShift = (modifier & KeyModifier.Shift) > 0;
                    UseControl = (modifier & KeyModifier.Ctrl) > 0;
                    UseAlt = (modifier & KeyModifier.Alt) > 0;
                    UseWindowsKey = (modifier & KeyModifier.Win) > 0;

                    key.Close();
                }

                SetHotkey();
            }
            finally
            {
                _isLoading = false;
            }
        }
        #endregion
    }
}
