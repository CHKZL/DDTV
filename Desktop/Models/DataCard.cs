using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Desktop.Models
{
    public struct DataCard
    {
        public long Uid { get; set; }
        public long Room_Id {  get; set; }
        /// <summary>
        /// 封面图
        /// </summary>
        public BitmapImage CoverImage { get; set; }
        public string CoverImageUrl {  get; set; }
        /// <summary>
        /// 头像图片
        /// </summary>
        public BitmapImage FaceImage { get; set; }
        public string FaceImageUrl { get; set; }
        /// <summary>
        /// 当前你标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 主播昵称
        /// </summary>
        public string Nickname { get; set; }
        /// <summary>
        /// 直播状态
        /// </summary>
        public Visibility Live_Status_IsVisible { get; set; }
        public bool Live_Status { get; set; }
    }
}

