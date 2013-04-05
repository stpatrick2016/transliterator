using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Diagnostics;
using Microsoft.Win32;

namespace Transliterator.LayoutEditor.Views
{
    /// <summary>
    /// Interaction logic for LayoutView.xaml
    /// </summary>
    public partial class LayoutView : UserControl
    {
        #region Data members
        private LayoutViewModel _vm = new LayoutViewModel();
        #endregion

        public LayoutView()
        {
            InitializeComponent();
            DataContext = _vm;
        }

        private void fillEnglish_Click(object sender, RoutedEventArgs e)
        {
            for (char ch = 'a'; ch <= 'z'; ch++)
            {
                if(!_vm.Layout.MappingPairs.Any(m=>m.Source == ch.ToString()))
                {
                    _vm.Layout.MappingPairs.Add(new Infrastructure.MappingPair { Source = ch.ToString() });
                }
            }
        }

        private void newButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.NewLayout();
        }

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.Multiselect = false;
            if (dlg.ShowDialog(Window.GetWindow(this)) == true)
            {
                _vm.LoadLayout(dlg.FileName);
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.CheckPathExists = true;
            dlg.OverwritePrompt = true;
            if (dlg.ShowDialog(Window.GetWindow(this)) == true)
            {
                _vm.SaveLayout(dlg.FileName);
            }
        }
    }
}
