using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;

namespace Transliterator.LayoutEditor.Infrastructure
{
    public class LayoutInfo
    {
        #region Data members
        private ObservableCollection<MappingPair> _pairs = new ObservableCollection<MappingPair>();
        #endregion

        #region Properties
        public IList<MappingPair> MappingPairs
        {
            get { return _pairs; }
        }

        #endregion

        #region Methods
        public void Load(string targetFilename)
        {
            _pairs.Clear();

            var lines = File.ReadAllLines(targetFilename);
            foreach (var line in lines)
            {
                var ss = line.Split("=".ToCharArray(), 2);
                MappingPair pair = _pairs.FirstOrDefault(m => m.Source == ss[0]);
                if (pair == null)
                {
                    pair = new MappingPair();
                    pair.Source = ss[0]; 
                    _pairs.Add(pair);
                }

                if (!String.IsNullOrWhiteSpace(ss[1]))
                {
                    var targets = ss[1].Split('^');
                    pair.LowerCaseTarget = String.Empty;
                    for (int i = 0; i < targets[0].Length; i += 4)
                    {
                        pair.LowerCaseTarget += Char.ConvertFromUtf32(Int32.Parse(targets[0].Substring(i, 4), System.Globalization.NumberStyles.HexNumber));
                    }
                    if (targets.Length > 1)
                    {
                        pair.UpperCaseTarget = Char.ConvertFromUtf32(Int32.Parse(targets[1], System.Globalization.NumberStyles.HexNumber));
                        pair.IsUpperCaseAutomatic = false;
                    }
                }
            }
        }

        public void Save(string filepath)
        {
            List<string> lines = new List<string>();
            foreach (var pair in _pairs)
            {
                string line = pair.Source + "=";
                if (!String.IsNullOrEmpty(pair.LowerCaseTarget))
                {
                    for (int i = 0; i < pair.LowerCaseTarget.Length; i++)
                    {
                        line += Char.ConvertToUtf32(pair.LowerCaseTarget, i).ToString("x4");
                    }
                }

                if (!pair.IsUpperCaseAutomatic && !String.IsNullOrEmpty(pair.UpperCaseTarget))
                {
                    line += "^";
                    for (int i = 0; i < pair.UpperCaseTarget.Length; i++)
                    {
                        line += Char.ConvertToUtf32(pair.UpperCaseTarget, i).ToString("x4");
                    }
                }

                lines.Add(line);
            }

            File.WriteAllLines(filepath, lines.ToArray(), Encoding.ASCII);
        }
        #endregion

    }
}
