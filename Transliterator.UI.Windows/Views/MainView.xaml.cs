using System.Windows;
using System.Windows.Controls;

namespace Transliterator.UI.Windows.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();

            DataContextChanged += MainView_DataContextChanged;

        }

        private void MainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            MainViewModel vm = DataContext as MainViewModel;
            if (vm != null)
            {
                vm.StateChanged += MainView_StateChanged;
            }
        }


        private void MainView_StateChanged(object sender, MainViewModel.StateChangedEventArgs e)
        {
            RaiseTranslitStateChangedEvent(e.IsEnabled);
        }


        #region TranslitStateChanged routed event
        public static readonly RoutedEvent TranslitStateChangedEvent = EventManager.RegisterRoutedEvent(
            "TranslitStateChanged", RoutingStrategy.Bubble, typeof(TranslitStateChangedRoutedEventHandler), typeof(MainView));

        public event RoutedEventHandler TranslitStateChanged
        {
            add { AddHandler(TranslitStateChangedEvent, value); }
            remove { RemoveHandler(TranslitStateChangedEvent, value); }
        }

        private void RaiseTranslitStateChangedEvent(bool isEnabled)
        {
            RoutedEventArgs args = new TranslitStateChangedEventArgs(TranslitStateChangedEvent, isEnabled);
            RaiseEvent(args);
        }

        public class TranslitStateChangedEventArgs : RoutedEventArgs
        {
            public TranslitStateChangedEventArgs(RoutedEvent revent, bool isEnabled)
                : base(revent)
            {
                IsEnabled = isEnabled;
            }

            public bool IsEnabled { get; private set; }
        }

        public delegate void TranslitStateChangedRoutedEventHandler(object sender, TranslitStateChangedEventArgs e);
        #endregion
    }
}
