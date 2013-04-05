using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Transliterator.LayoutEditor.Views
{
    public class ViewModelBase : INotifyPropertyChanged
    {
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
