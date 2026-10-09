using System.ComponentModel;
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
        /// <summary>
        /// 黑听模式（纯音频）信息面板的可见性
        /// </summary>
        public Visibility AudioOnlyVisibility { set; get; } = Visibility.Collapsed;
        /// <summary>
        /// 黑听模式面板上展示的房间信息文本
        /// </summary>
        public string AudioOnlyRoomText { set; get; }

    }
}