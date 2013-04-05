using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Transliterator.UI.Windows
{
    public static class SkinLoader
    {
        public static void Load(ResourceDictionary rd)
        {
            rd.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Transliterator.UI.Windows;component/Skins/Default.xaml")
            });

        }
    }
}
