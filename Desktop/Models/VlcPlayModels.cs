using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Desktop.Models
{
    public class VlcPlayModels : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string Volume { get; set; }
        public Visibility VolumeVisibility { set; get; } = Visibility.Collapsed;
        public Visibility LoadingVisibility { set; get; } = Visibility.Visible;
        public Visibility MessageVisibility { set; get; } = Visibility.Collapsed;
        public string MessageText { set; get; }

    }
}