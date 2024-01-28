using System;
using System.ComponentModel;

namespace FileExplorer.MVVM.Bases
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public Action CloseAction { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
