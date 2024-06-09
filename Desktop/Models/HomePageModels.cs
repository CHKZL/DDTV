using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop.Models
{
    public class HomePageModels : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public int MonitoringCount { get; set; } = 0;
        public int LiveCount { get; set; } = 0;
        public int RecCount { get; set; } = 0;
        public int HardDiskUsageRate { get; set; } = 0;
        public int MemoryUsageRate { get; set; } = 0;
        public string RunTime { get; set; } = "";
        public string runningState { get; set; } = "正常";
        public string announcement { get; set; } = "";
    }
}
