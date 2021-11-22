using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.DownloadModule
{
    public class DownloadClass
    {
       
        public class Downloads
        {
            /// <summary>
            /// 房间号
            /// </summary>
            public string RoomId { get; set; }
            public long Uid { set; get; }
            /// <summary>
            /// 是否下载中
            /// </summary>
            public bool IsDownloading { get; set; }
            /// <summary>
            /// 是否已修复FlvMetaData
            /// </summary>
            public bool IsFix { set; get; } = false;
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
                NewTask,
                /// <summary>
                /// 已准备
                /// </summary>
                Standby,
                /// <summary>
                /// 下载中
                /// </summary>
                Downloading,
                /// <summary>
                /// 下载结束
                /// </summary>
                DownloadComplete,
                /// <summary>
                /// 取消下载
                /// </summary>
                Cancel,
            }
            public void Clear()
            {
                //HttpWebRequest.Abort();
                IsDownloading=false;
                Status=DownloadStatus.DownloadComplete;
               
            }
            private bool IsCancel = false;
            public void Cancel()
            {
                IsCancel=true;
            }
            internal void DownFLV_HttpWebRequest(HttpWebRequest req, string Path,string FileName)
            {
                Task.Run(async () => {
                    //Path="D:"+Path.Substring(1, Path.Length-1);
                    Path= Tool.PathOperation.CreateAll(Path);                
                    FileName = Path+"/"+FileName;
                    File=FileName;
                    HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                    Stream stream = resp.GetResponseStream();
                    FileStream fileStream = new FileStream(FileName, FileMode.Create);
                    Status= DownloadStatus.Downloading;
                    
                    while (true)
                    {
                        if (stream.CanRead)
                        {
                            if(IsCancel)
                            {
                                stream.Close();
                                stream.Dispose();
                                resp.Close();
                                resp.Dispose();
                                Status= DownloadStatus.Cancel;
                                Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Info, $"用户取消[{RoomId}]的录制任务，该任务取消");
                                Download.DownloadCompleteTaskd(Uid,false);
                                return;
                            }
                            int EndF = stream.ReadByte();
                            if (EndF!=-1)
                            {
                                DownloadCount++;
                                byte T = (byte)EndF;
                                fileStream.Write(new byte[] { T }, 0, 1);
                            }
                            else
                            {
                                Clear();
                                fileStream.Close();
                                fileStream.Dispose();
                                if(await Tool.Flv.Fix.FixFlvMetaData(fileStream.Name)>-1)
                                {
                                    IsFix=true;
                                }
                                if (BilibiliModule.Rooms.Rooms.GetValue(Uid, DataCacheModule.DataCacheClass.CacheType.live_status)=="1")
                                {
                                    Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Info, $"检测到录制完成的[{RoomId}]直播状态还为“开播中”持续监听中");
                                    Download.AddDownloadTaskd(Uid);
                                    return;
                                }
                                else
                                {
                                    Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Info, $"[{RoomId}]房间的录制子任务已完成");
                                    Download.DownloadCompleteTaskd(Uid);
                                    return;
                                }
                            }
                        }                      
                    }
                });
            }
        }
        public class DlwnLog
        {
            /// <summary>
            /// 房间号
            /// </summary>
            public string RoomId { get; set; }
            public long Uid { set; get; }
            /// <summary>
            /// 是否下载中
            /// </summary>
            public bool IsDownloading { get; set; }
            /// <summary>
            /// 是否已修复FlvMetaData
            /// </summary>
            public bool IsFix { set; get; } = false;
            /// <summary>
            /// 下载地址
            /// </summary>
            public string Url { get; set; }
            /// <summary>
            /// 下载的文件
            /// </summary>
            public string File { set; get; }
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
