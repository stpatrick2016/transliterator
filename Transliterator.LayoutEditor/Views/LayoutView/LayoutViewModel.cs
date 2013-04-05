using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Transliterator.LayoutEditor.Infrastructure;
using Transliterator.UI.Windows.Views;

namespace Transliterator.LayoutEditor.Views
{
    public class LayoutViewModel : ViewModelBase
    {
        #region Data members
        private LayoutInfo _layout = new LayoutInfo();
        #endregion

        #region .ctor
        public LayoutViewModel()
            : base()
        {
        }
        #endregion

        #region Properties
        public LayoutInfo Layout
        {
            get { return _layout; }
            set { SetProperty(() => this.Layout, ref _layout, value); }
        }
        #endregion

        #region Methods
        public void LoadLayout(string filepath)
        {
            var layout = new LayoutInfo();
            layout.Load(filepath);
            Layout = layout;
        }

        public void SaveLayout(string filepath)
        {
            Layout.Save(filepath);
        }

        public void NewLayout()
        {
            Layout = new LayoutInfo();
        }
        #endregion

    }
}
