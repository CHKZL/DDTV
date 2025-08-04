using System.ComponentModel;

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
        public string IpvState { get; set; } = "正常，目前使用的IPv4";
        public string ProxyState { get; set; } = "正常，未检测到代理";
        public string ProxyUrl { get; set; } = "";
        public string announcement { get; set; } = "";
        public string WarningMessage { get; set; } = "";//例如：【警告：登录态已失效！请在设置界面重新扫码登陆】

    }
}
