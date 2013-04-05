using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Transliterator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        [DllImport("TrEngine.dll", CharSet = CharSet.Unicode)]
        static extern bool RegisterKeyboardLayout(string filepath);

        [DllImport("TrEngine.dll")]
        static extern void EnableTransliteration();
        
        [DllImport("TrEngine.dll")]
        static extern void DisableTransliteration();

        [DllImport("TrEngine.dll")]
        static extern void SetCurrentLayout(int index);

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EnableTransliteration();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DisableTransliteration();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Russian.kbd");
            RegisterKeyboardLayout(path);
            SetCurrentLayout(0);
            EnableTransliteration();
        }
    }
}
