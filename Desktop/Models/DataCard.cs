using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Core.RuntimeObject.RoomCardClass;

namespace Desktop.Models
{
    public struct DataCard
    {
        public long Uid { get; set; }
        public long Room_Id { get; set; }
        /// <summary>
        /// 当前你标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 主播昵称
        /// </summary>
        public string Nickname { get; set; }
        /// <summary>
        /// 是否录制视频
        /// </summary>
        public bool IsRec { get; set; }
        /// <summary>
        /// 视频录制标志状态颜色
        /// </summary>
        public SolidColorBrush RecSign { get; set; }
        /// <summary>
        /// 是否录制弹幕
        /// </summary>
        public bool IsDanmu { get; set; }
        /// <summary>
        /// 弹幕录制标志状态颜色
        /// </summary>
        public SolidColorBrush DanmuSign { get; set; }
        /// <summary>
        /// 开播提醒
        /// </summary>
        public bool IsRemind { get; set; }
        /// <summary>
        /// 开播提醒标志状态颜色
        /// </summary>
        public SolidColorBrush RemindSign { get; set; }


        /// <summary>
        /// 是否为录制中
        /// </summary>
        public bool Rec_Status { get; set; }
        /// <summary>
        /// 录制中标志
        /// </summary>
        public Visibility Rec_Status_IsVisible { get; set; }
        /// <summary>
        /// 是否为直播中
        /// </summary>
        public bool Live_Status { get; set; }
        /// <summary>
        /// 直播中标志
        /// </summary>
        public Visibility Live_Status_IsVisible { get; set; }
        /// <summary>
        /// 是否为未直播
        /// </summary>
        public bool Rest_Status { get; set; }
        /// <summary>
        /// 未直播标志
        /// </summary>
        public Visibility Rest_Status_IsVisible { get; set; }


        /// <summary>
        /// 下载速度bit
        /// </summary>
        public double DownloadSpe { get; set; }
        /// <summary>
        /// 下载速度文本
        /// </summary>
        public string DownloadSpe_str { get; set; }
        /// <summary>
        /// 是否下载中
        /// </summary>
        public bool IsDownload { get; set; }

        ///// <summary>
        ///// 任务状态
        ///// </summary>
        //public DownloadStatus downloadStatus { get; set; }

        /// <summary>
        /// 直播持续时间
        /// </summary>
        public long LiveTime { get; set; }
        /// <summary>
        /// 直播持续时间_字符串
        /// </summary>
        public string LiveTime_str { get; set; }

    }
}

