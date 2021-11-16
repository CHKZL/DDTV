using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.DownloadModule
{
    internal class DownloadClass
    {
        public static List<Downloads> DownloadList = new();
        internal class Downloads
        {
            public string RoomId { get; set; }
            /// <summary>
            /// 是否下载中
            /// </summary>
            public bool IsDownloading { get; set; }
            public List<DownloadInfo> downloadInfos = new();
            public class DownloadInfo
            {
                /// <summary>
                /// 下载地址
                /// </summary>
                public string Url { get; set; }
                /// <summary>
                /// 下载的文件
                /// </summary>
                public string File { set; get; }
                /// <summary>
                /// WebRequest类的HTTP的实现
                /// </summary>
                public HttpWebRequest HttpWebRequest { get; set; }
                /// <summary>
                /// 已下载字节数
                /// </summary>
                public long DownloadCount { get; set; }
                /// <summary>
                /// 下载状态
                /// </summary>
                public DownloadStatus Status { get; set; } = DownloadStatus.NewTask;
                public enum DownloadStatus
                {
                    /// <summary>
                    /// 新任务
                    /// </summary>
                    NewTask = 0,
                    /// <summary>
                    /// 已准备
                    /// </summary>
                    Standby = 1,
                    /// <summary>
                    /// 下载中
                    /// </summary>
                    Downloading = 2,
                    /// <summary>
                    /// 下载结束
                    /// </summary>
                    DownloadComplete = 3,
                }
            }
        }
    }
}
