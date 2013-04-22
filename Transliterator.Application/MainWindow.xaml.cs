using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using Transliterator.UI.Windows;
using Transliterator.UI.Windows.Views;

namespace Transliterator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon = null;
        private System.Drawing.Icon _enabledIcon = null, _disabledIcon = null;
        private bool _reallyExiting = false;
        public MainWindow()
        {
            InitializeComponent();

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Transliterator.Images.TrayGrey.ico"))
            {
                _disabledIcon = new System.Drawing.Icon(stream);
            }

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Transliterator.Images.TrayGreen.ico"))
            {
                _enabledIcon = new System.Drawing.Icon(stream);
            }

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Text = "Transliterator (Disabled)";
            _notifyIcon.Icon = _disabledIcon;
            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            _notifyIcon.Visible = true;
            _notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu();
            _notifyIcon.ContextMenu.MenuItems.Add("Open", NotifyIcon_DoubleClick).DefaultItem = true;
            _notifyIcon.ContextMenu.MenuItems.Add("-");
            _notifyIcon.ContextMenu.MenuItems.Add("Exit", Exit_Clicked);
        }

        private void NotifyIcon_DoubleClick(object sender, System.EventArgs e)
        {
            this.Show();
            WindowState = System.Windows.WindowState.Normal;
            this.Focus();
        }

        private void Exit_Clicked(object sender, System.EventArgs e)
        {
            _reallyExiting = true;
            this.Close();
        }

        private void MainView_TranslitStateChanged(object sender, RoutedEventArgs e)
        {
            MainView.TranslitStateChangedEventArgs args = (MainView.TranslitStateChangedEventArgs)e;
            _notifyIcon.Icon = args.IsEnabled ? _enabledIcon : _disabledIcon;
            _notifyIcon.Text = "Transliterator " + (args.IsEnabled ? "(Enabled)" : "(Disabled)");
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (GlobalConfiguration.Instance.MinimizeOnClose && !_reallyExiting)
            {
                WindowState = System.Windows.WindowState.Minimized;
                e.Cancel = true;
            }
            else
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Minimized)
            {
                this.Hide();
            }
        }
    }
}
