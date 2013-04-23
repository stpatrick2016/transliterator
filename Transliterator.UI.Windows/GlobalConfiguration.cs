using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Transliterator.UI.Windows
{
    public class GlobalConfiguration
    {
        #region Data members
        private static volatile GlobalConfiguration _instance = null;
        private readonly string RegistryPath = @"Software\Philip Patrick\Transliterator";
        #endregion

        #region Properties
        public static GlobalConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(GlobalConfiguration))
                    {
                        if (_instance == null)
                        {
                            _instance = new GlobalConfiguration();
                            _instance.Load();
                        }
                    }
                }

                return _instance;
            }
        }

        public bool LoadOnStartup { get; set; }
        public string Hotkey { get; set; }
        public string HotkeyModifier { get; set; }
        public bool MinimizeOnClose { get; set; }
        #endregion

        #region Methods
        public void Load()
        {
            using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                Hotkey = key.GetValue("Hotkey", null) as string;
                HotkeyModifier = key.GetValue("HotkeyModifier", null) as string;
                MinimizeOnClose = 1.Equals(key.GetValue("MinimizeOnClose", 0));

                key.Close();
            }

            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
            {
                if (key != null)
                {
                    string path = key.GetValue("Transliterator (Philip Patrick)", null) as string;
                    LoadOnStartup = !String.IsNullOrEmpty(path);
                    key.Close();
                }
            }
        }

        public void Save()
        {
            using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                key.SetValue("Hotkey", Hotkey);
                key.SetValue("HotkeyModifier", HotkeyModifier);
                key.SetValue("MinimizeOnClose", MinimizeOnClose ? 1 : 0);

                key.Close();
            }

            using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
            {
                if (LoadOnStartup)
                {
                    string path = System.Reflection.Assembly.GetEntryAssembly().Location;
                    key.SetValue("Transliterator (Philip Patrick)", String.Format("\"{0}\"", path));
                }
                else
                {
                    key.DeleteValue("Transliterator (Philip Patrick)", false);
                }

                key.Close();
            }
        }
        #endregion
    }
}
