using System.Collections.Generic;
using System.ComponentModel;
using Recorder.Models;

namespace Recorder.ViewModels
{
    public class BaseViewmodel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<T>(ref T field, T value, params string[] notifyPropertyNames)
        {
            if (!ReferenceEquals(field, value))
            {
                field = value;
                foreach (string name in notifyPropertyNames)
                {
                    RaisePropertyChanged(name);
                }
            }
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
