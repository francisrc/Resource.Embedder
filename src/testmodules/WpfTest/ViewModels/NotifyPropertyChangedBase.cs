using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace WpfTest.ViewModels
{
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Methods

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged<TProperty>(Expression<Func<TProperty>> prop)
        {
            OnPropertyChanged(prop.GetMemberInfo().Name);
        }

        #endregion Methods
    }
}