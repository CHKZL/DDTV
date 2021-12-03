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
            /// 下载地址
            /// </summary>
            public string Url { get; set; }
            /// <summary>
            /// 下载的文件
            /// </summary>
            public string File { set; get; }
            public Tool.FlvModule.FlvClass.FlvTimes flvTimes { set; get; } = new();
            /// <summary>
            /// FLV文件头
            /// </summary>
            public Tool.FlvModule.FlvClass.FlvHeader FlvHeader { set; get; } = new();
            /// <summary>
            /// FLV头脚本数据
            /// </summary>
            public Tool.FlvModule.FlvClass.FlvTag FlvScriptTag { set; get; } = new();
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
                IsDownloading = false;
                Status = DownloadStatus.DownloadComplete;

            }
            private bool IsCancel = false;
            public void Cancel()
            {
                IsCancel = true;
            }
            /// <summary>
            /// 下载任务实际执行事件
            /// </summary>
            /// <param name="req">下载任务WebRequest对象</param>
            /// <param name="Path">保存路径</param>
            /// <param name="FileName">保存文件名</param>
            /// <param name="Split">是否切片</param>
            internal void DownFLV_HttpWebRequest(HttpWebRequest req, string Path, string FileName, string format, bool Split)
            {
                Task.Run(() =>
                {
                    try
                    {
                        int count = 1;
                        //Path="D:"+Path.Substring(1, Path.Length-1);
                        Path = Tool.PathOperation.CreateAll(Path);
                        File = Path + "/" + FileName + "_" + count + "." + format;
                        using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                        {
                            using (Stream stream = resp.GetResponseStream())
                            {
                                FileStream fileStream = new FileStream(File, FileMode.Create);

                                Status = DownloadStatus.Downloading;
                                uint DataLength = 9;
                                uint TagLen = 0;
                                while (true)
                                {
                                    byte[] data = new byte[DataLength];
                                    if (IsCancel)
                                    {
                                        stream.Close();
                                        stream.Dispose();
                                        resp.Close();
                                        resp.Dispose();
                                        Status = DownloadStatus.Cancel;
                                        Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Info, $"用户取消[{RoomId}]的录制任务，该任务取消");
                                        Download.DownloadCompleteTaskd(Uid, false, false, false);
                                       
                                        return;
                                    }
                                    for (int i = 0 ; i < DataLength ; i++)
                                    {
                                        EOF: int EndF = 0;
                                        if (stream.CanRead)
                                        {
                                            try
                                            {
                                                EndF = stream.ReadByte();
                                            }
                                            catch (Exception e)
                                            {
                                                Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Warn, $"录制任务读取到错误的EOF位，写入的文件为:{File}",true,e);
                                                goto EOF;
                                            }
                                        }
                                        else
                                        {
                                            EndF = -1;
                                        }
                                        if (EndF != -1)
                                        {
                                            data[i] = (byte)EndF;
                                            DownloadCount++;
                                        }
                                        else
                                        {
                                            Clear();
                                            fileStream.Close();
                                            fileStream.Dispose();
                                            if (BilibiliModule.Rooms.Rooms.GetValue(Uid, DataCacheModule.DataCacheClass.CacheType.live_status) == "1")
                                            {
                                                Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Info, $"检测到录制完成的[{RoomId}]直播状态还为“开播中”持续监听中");
                                                if (resp != null)
                                                {
                                                    resp.Close();
                                                    resp.Dispose();
                                                }
                                                Download.AddDownloadTaskd(Uid,false);
                                                return;
                                            }
                                            else
                                            {
                                                Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Info, $"[{RoomId}]房间的录制子任务已完成");
                                                if (resp != null)
                                                {
                                                    resp.Close();
                                                    resp.Dispose();
                                                }
                                                Download.DownloadCompleteTaskd(Uid, Split);
                                                return;
                                            }
                                        }
                                    }
                                    byte[] FixData = Tool.FlvModule.SteamFix.FixWrite(data, this, out uint DL);
                                    DataLength = DL;
                                    fileStream.Write(FixData, 0, FixData.Length);
                                    if (Split && DownloadCount > (1024 * 1024 * 5) && DataLength == 15)
                                    {
                                        count++;
                                        fileStream.Close();
                                        fileStream.Dispose();
                                        File = Path + "/" + FileName + "_" + count + "." + format;
                                        fileStream = new FileStream(File, FileMode.Create);
                                        byte[] buffer = new byte[9 + 15] { FlvHeader.Signature[0], FlvHeader.Signature[1], FlvHeader.Signature[2], FlvHeader.Version, FlvHeader.Type, FlvHeader.FlvHeaderOffset[0], FlvHeader.FlvHeaderOffset[1], FlvHeader.FlvHeaderOffset[2], FlvHeader.FlvHeaderOffset[3], 0x00, 0x00, 0x00, 0x01, FlvScriptTag.TagType, FlvScriptTag.TagDataSize[0], FlvScriptTag.TagDataSize[1], FlvScriptTag.TagDataSize[2], FlvScriptTag.Timestamp[3], FlvScriptTag.Timestamp[2], FlvScriptTag.Timestamp[1], FlvScriptTag.Timestamp[0], 0x00, 0x00, 0x00 };
                                        fileStream.Write(buffer, 0, buffer.Length);
                                        fileStream.Write(FlvScriptTag.TagaData, 0, FlvScriptTag.TagaData.Length);
                                        fileStream.Write(FlvScriptTag.FistAbody, 0, FlvScriptTag.FistAbody.Length);
                                        fileStream.Write(FlvScriptTag.FistVbody, 0, FlvScriptTag.FistVbody.Length);
                                        TagLen = (uint)FlvScriptTag.TagaData.Length + 15;
                                        flvTimes.ErrorAudioTimes = 0;
                                        flvTimes.ErrorVideoTimes = 0;
                                        flvTimes.FlvTotalTagCount = 1;
                                        flvTimes.FlvVideoTagCount = 1;
                                        flvTimes.FlvAudioTagCount = 1;
                                        flvTimes.IsTagHeader = true;
                                        DownloadCount = TagLen;
                                    }

                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Error, $"新建下载任务发生意料外的错误！错误信息:\n{e.ToString()}");
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
