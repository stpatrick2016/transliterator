using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Transliterator.LayoutEditor.Infrastructure
{
    public class LayoutInfo : INotifyPropertyChanged
    {
        #region Data members
        private ObservableCollection<MappingPair> _pairs = new ObservableCollection<MappingPair>();
        #endregion

        #region Properties
        public IList<MappingPair> MappingPairs
        {
            get { return _pairs; }
        }

        private string _layoutName;
        public string LayoutName 
        {
            get { return _layoutName; }
            set { SetProperty(() => LayoutName, ref _layoutName, value); }
        }

        #endregion

        #region Methods
        public void Load(string targetFilename)
        {
            _pairs.Clear();

            bool isReadingHeader = false;
            var lines = File.ReadAllLines(targetFilename);
            foreach (var line in lines)
            {
                //begin of the header
                if (line == ":begin header:")
                {
                    isReadingHeader = true;
                }

                //end of the header
                else if (line == ":end header:")
                {
                    isReadingHeader = false;
                }

                //we are in reading header mode
                else if (isReadingHeader)
                {
                    ParseHeader(line);
                }

                //header read, we parse the character pairs
                else
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
                            if(targets[1].Length > 0)
                            {
                                pair.UpperCaseTarget = Char.ConvertFromUtf32(Int32.Parse(targets[1], System.Globalization.NumberStyles.HexNumber));
                            }

                            pair.IsUpperCaseAutomatic = false;
                        }
                    }
                }
            }
        }

        public void Save(string filepath)
        {
            List<string> lines = new List<string>();
            WriteHeader(lines);
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

        #region Private methods
        private void ParseHeader(string line)
        {
            var ss = line.Split("=".ToCharArray(), 2);
            if (ss.Length == 2)
            {
                if (ss[0] == "name")
                {
                    LayoutName = ss[1];
                }
            }
        }

        private void WriteHeader(IList<string> headerLines)
        {
            headerLines.Add(":begin header:");
            headerLines.Add(String.Format("name={0}", LayoutName));
            headerLines.Add(":end header:");
        }
        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
#if !SILVERLIGHT
        [field: NonSerialized]
#endif
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Sets the property if its value has changed,
        /// and also invokes the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="name">Property public name</param>
        /// <param name="field">Backing field of property</param>
        /// <param name="value">New property value</param>
        /// <returns><c>true</c> if property's value was changed, <c>false</c> otherwise</returns>
        protected bool SetProperty<T>(string name, ref T field, T value)
        {
            if (!object.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(name);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the property if its value has changed,
        /// and also invokes the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="propertyExpression">A Lambda expression representing the property that has a new value.</param>
        /// <param name="field">Backing field of property</param>
        /// <param name="value">New property value</param>
        /// <returns><c>true</c> if property's value was changed, <c>false</c> otherwise</returns>
        protected bool SetProperty<T>(System.Linq.Expressions.Expression<Func<T>> propertyExpression, ref T field, T value)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }

            var memberExpression = propertyExpression.Body as System.Linq.Expressions.MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException("Set property called with non-member expression", "propertyExpression");
            }

            var property = memberExpression.Member as System.Reflection.PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException("Set property called with non-property expression", "propertyExpression");
            }

            var getMethod = property.GetGetMethod(true);
            if (getMethod.IsStatic)
            {
                throw new ArgumentException("SetProperty doesn't support static properties", "propertyExpression");
            }

            return SetProperty(memberExpression.Member.Name, ref field, value);
        }
        #endregion
    }
}
