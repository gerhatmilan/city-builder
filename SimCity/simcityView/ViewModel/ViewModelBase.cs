using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace simcityView.ViewModel
{
   
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
       
        protected ViewModelBase() { }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] String? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
