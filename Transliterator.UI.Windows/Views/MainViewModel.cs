using System;
using System.Windows.Input;
using Transliterator.UI.Windows.Controls;

namespace Transliterator.UI.Windows.Views
{
    public class MainViewModel : ViewModelBase
    {
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


        private void TurnOn()
        {
            EngineAdapter.EnableTransliteration();
        }

        private void TurnOff()
        {
            EngineAdapter.DisableTransliteration();
        }
    }
}
