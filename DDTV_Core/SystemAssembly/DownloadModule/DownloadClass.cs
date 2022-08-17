using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using DDTV_Core.SystemAssembly.NetworkRequestModule.WebHook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DDTV_Core.SystemAssembly.DownloadModule
{
    public class DownloadClass
    {

        public class Downloads
        {
            public string Token { get; set; } = Guid.NewGuid().ToString("N");
            /// <summary>
            /// 房间号
            /// </summary>
            public string RoomId { get; set; }
            /// <summary>
            /// 用户UID
            /// </summary>
            public long Uid { set; get; }
            /// <summary>
            /// 昵称
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 标题
            /// </summary>
            public string Title { get; set; }
            /// <summary>
            /// FLV大小限制使能
            /// </summary>
            public bool FlvSplit { get; set; } = false;
            /// <summary>
            /// FLV切割大小单位为byte
            /// </summary>
            public long FlvSplitSize { set; get; }
            /// <summary>
            /// 是否下载中
            /// </summary>
            public bool IsDownloading { get; set; }
            /// <summary>
            /// 下载地址
            /// </summary>
            public string Url { get; set; }
            /// <summary>
            /// 下载的完整文件路径
            /// </summary>
            public string FileName { set; get; }
            /// <summary>
            /// 该任务下载的flv文件列表
            /// </summary>
            public List<string> FlvFileList { get; set; } = new List<string>();
            /// <summary>
            /// 文件夹路径
            /// </summary>
            public string FilePath { set; get; }
            /// <summary>
            /// 整体任务开始时间
            /// </summary>
            public DateTime StartTime { set; get; } = DateTime.Now;
            /// <summary>
            /// 整体任务结束时间
            /// </summary>
            public DateTime EndTime { set; get; }
            /// <summary>
            /// 当前录制的文件视频时长
            /// </summary>
            public uint RecordingDuration { set; get; } = 0;
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
            /// 当前已下载字节数
            /// </summary>
            public long DownloadCount { get; set; }
            /// <summary>
            /// 该任务下所有任务的总下载字节数
            /// </summary>
            public long TotalDownloadCount { get; set; }
            /// <summary>
            /// 下载状态
            /// </summary>
            public DownloadStatus Status { get; set; } = DownloadStatus.NewTask;
            /// <summary>
            /// 下载速度
            /// </summary>
            public long DownloadSpe { get; set; }
            /// <summary>
            /// 触发下播次数统计
            /// </summary>
            public int LiveStopTriggerStatistics { set; get; }
            /// <summary>
            /// 异常速度统计
            /// </summary>
            public int ErrorDownloadSpeTrigger { set; get; }
            /// <summary>
            /// 当前子任务开始时间
            /// </summary>
            public DateTime SpeedCalibration_Time = DateTime.Now;
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
            public Downloads Clone()
            {
                Downloads downloads = new Downloads();
                downloads = this;
                return downloads;
            }
            public void Clear()
            {
                //HttpWebRequest.Abort();
                IsDownloading = false;
                Status = DownloadStatus.DownloadComplete;
                EndTime = DateTime.Now;

            }
            private bool IsCancel = false;
            public void Cancel()
            {
                IsCancel = true;
            }
            /// <summary>
            /// 下载任务实际执行事件
            /// </summary>
            /// <param name="downloads">下载对象</param>
            /// <param name="req">下载任务WebRequest对象</param>
            /// <param name="Path">保存路径</param>
            /// <param name="FileName">保存文件名</param>
            /// <param name="format">保存的文件格式</param>
            /// <param name="roomInfo">房间对象</param>
            internal void DownFLV_HttpWebRequest(Downloads downloads, HttpWebRequest req, string Path, string FileName, string format, RoomInfoClass.RoomInfo roomInfo)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        WebHook.SendHook(WebHook.HookType.StartRec, roomInfo.uid);
                        int count = 1;
                        //Path="D:"+Path.Substring(1, Path.Length-1);
                        Path = Tool.FileOperation.CreateAll(Path);
                        string _F = Path + $"/{roomInfo.CreationTime.ToString("yy_MM_dd")}/" + FileName + "_" + count + "." + format;
                        downloads.FileName = _F;
                        downloads.FlvFileList.Add(_F);
                        using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                        {
                            using (Stream stream = resp.GetResponseStream())
                            {
                                FileStream fileStream = new FileStream(_F, FileMode.Create);

                                Status = DownloadStatus.Downloading;
                                uint DataLength = 9;
                                uint TagLen = 0;
                                DateTime SpeedCalibration_Time = DateTime.Now;
                                long SpeedCalibration_Size = 0;
                                while (true)
                                {
                                    if (DataLength > 1000000)
                                    {
                                        Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Debug, $"疑是错误！检测到超长的DataLength包，长度:{DataLength}byte");
                                    }
                                    byte[] data = new byte[DataLength];
                                    if (IsCancel)
                                    {
                                        try
                                        {
                                            stream.Close();
                                            stream.Dispose();
                                            resp.Close();
                                            resp.Dispose();
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Error, $"取消[{roomInfo.uname}({roomInfo.room_id})]的时候出现已知范围内的其他错误，错误堆栈已写入日志", true, e, true);
                                        }
                                        Status = DownloadStatus.Cancel;
                                        Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Info, $"用户取消[{roomInfo.uname}({roomInfo.room_id})]的录制任务，该任务取消");
                                        roomInfo.IsDownload = false;
                                        Download.DownloadCompleteTaskd(Uid, false, true);

                                        return;
                                    }
                                    bool IsEnd = false;
                                    for (int i = 0; i < DataLength; i++)
                                    {
                                        int EndF = 0;
                                        if (stream.CanRead)
                                        {
                                            try
                                            {
                                                EndF = stream.ReadByte();
                                                if(LiveStopTriggerStatistics>5)
                                                {
                                                    EndF = -1;
                                                    IsEnd = true;
                                                    LiveStopTriggerStatistics = 0;
                                                    break;
                                                }
                                            }
                                            catch (NotSupportedException e)
                                            {
                                                IsEnd = true;
                                                //流不支持读取
                                                EndF = -1;
                                                break;
                                            }
                                            catch (ObjectDisposedException e)
                                            {
                                                IsEnd = true;
                                                //被关闭了
                                                EndF = -1;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            IsEnd = true;
                                            EndF = -1;
                                            break;
                                        }
                                        if (!IsEnd)
                                        {
                                            data[i] = (byte)EndF;
                                            DownloadCount++;
                                            TotalDownloadCount++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    if(DataLength==15)
                                    {
                                        bool IsAllFF = true;
                                        foreach (var item in data)
                                        {
                                            if(item!=0xFF)
                                            {
                                                IsAllFF = false;
                                                break;
                                            }
                                        }
                                        if(IsAllFF)
                                        {
                                            Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Warn, $"在录制[{roomInfo.uname}({roomInfo.room_id})]的直播任务DataLength时触发了AllFF错误，结束该子任务开始续命");
                                            IsEnd = true;
                                        }
                                    }
                                    AllFFErrpr: if(IsEnd)
                                    {
                                        Clear();
                                        if (flvTimes.FlvTotalTagCount < 3)
                                        {
                                            Rooms.RoomInfo[Uid].DownloadingList.RemoveAt(Rooms.RoomInfo[Uid].DownloadingList.Count - 1);
                                            if (File.Exists(fileStream.Name))
                                            {
                                                Tool.FileOperation.Del(fileStream.Name);
                                            }
                                        }
                                        try
                                        {
                                            fileStream.Close();
                                            fileStream.Dispose();
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Error, $"取消[{roomInfo.uname}({roomInfo.room_id})]的时候出现已知范围内的其他错误，错误堆栈已写入日志", true, e, true);
                                        }

                                        try
                                        {
                                            stream.Close();
                                            stream.Dispose();
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Error, $"取消[{roomInfo.uname}({roomInfo.room_id})]的时候出现已知范围内的其他错误，错误堆栈已写入日志", true, e, true);
                                        }
                                        try
                                        {
                                            resp.Close();
                                            resp.Dispose();
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Error, $"取消[{roomInfo.uname}({roomInfo.room_id})]的时候出现已知范围内的其他错误，错误堆栈已写入日志", true, e, true);
                                        }

                                        if (Rooms.GetValue(Uid, DataCacheModule.DataCacheClass.CacheType.live_status) == "1" && !IsCancel)
                                        {
                                            Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Info, $"[{roomInfo.uname}({roomInfo.room_id})]录制流已断开，但监测到直播状态还为“开播中”持续监听中，尝试继续监听");
                                            roomInfo.IsDownload = false;
                                            Download.AddDownloadTaskd(Uid, false);
                                            return;
                                        }
                                        else
                                        {
                                            Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Info, $"房间[{roomInfo.uname}({roomInfo.room_id})]的录制子任务已完成");
                                            roomInfo.IsDownload = false;
                                            Download.DownloadCompleteTaskd(Uid, downloads.FlvSplit);
                                            return;
                                        }
                                    }
                                    bool IsFixWriteError = false;
                                    byte[] FixData = Tool.FlvModule.SteamFix.FixWrite(data, this, out uint DL,out IsFixWriteError);
                                    if(IsFixWriteError)
                                    {
                                        IsEnd = true;
                                        Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Warn, $"在录制[{roomInfo.uname}({roomInfo.room_id})]的直播任务FixWrite时触发了AllFF错误，结束该子任务开始续命");
                                        goto AllFFErrpr;
                                    }
                                    DataLength = DL;
                                    if (FixData != null)
                                    {
                                        fileStream.Write(FixData, 0, FixData.Length);
                                    }
                                    if (downloads.FlvSplit && DownloadCount > downloads.FlvSplitSize && DataLength == 15)
                                    {
                                        count++;
                                        fileStream.Close();
                                        fileStream.Dispose();
                                        string _F2 = Path + $"/{roomInfo.CreationTime.ToString("yy_MM_dd")}/" + FileName + "_" + count + "." + format;
                                        downloads.FlvFileList.Add(_F2);
                                        fileStream = new FileStream(_F2, FileMode.Create);
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
                                    TimeSpan ts = DateTime.Now - SpeedCalibration_Time;    //计算时间差
                                    if (ts.TotalMilliseconds > 3000)
                                    {
                                        if(roomInfo.live_status!=1)
                                        {
                                            LiveStopTriggerStatistics++;
                                            Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Debug, $"房间[{roomInfo.uname}({roomInfo.room_id})]触发下播次数统计:{LiveStopTriggerStatistics}",false,null,false);
                                        }
                                        else if (roomInfo.live_status == 1)
                                        {
                                            LiveStopTriggerStatistics = 0;
                                        }
                                        double Proportion = ts.TotalMilliseconds / 3000.0;
                                        long CurrentDownloadSize = TotalDownloadCount - SpeedCalibration_Size;

                                        double T = CurrentDownloadSize / Proportion / 3.0;
                                        DownloadSpe = Convert.ToInt64(T);
                                        if(DownloadSpe > (1048576*100))
                                        {
                                            Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Debug, $"疑是错误！检测到超长的DownloadSpe长度，长度:{DownloadSpe}byte");
                                        }
                                        SpeedCalibration_Size = TotalDownloadCount;
                                        SpeedCalibration_Time = DateTime.Now;
                                    }
                                }
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Error, $"新建下载任务发生意WEB连接错误，尝试重连", true, ex);
                        roomInfo.IsDownload = false;
                        IsDownloading = false;
                        Rooms.RoomInfo[Uid].DownloadingList.RemoveAt(Rooms.RoomInfo[Uid].DownloadingList.Count - 1);

                        if (!IsCancel)
                        {
                            Download.AddDownloadTaskd(Uid, false);
                        }
                        return;
                    }
                    catch (Exception e)
                    {
                        Log.Log.AddLog(nameof(DownloadClass), Log.LogClass.LogType.Error, $"新建下载任务发生意料外的错误", true, e);
                        roomInfo.IsDownload = false;
                        IsDownloading = false;
                        if (!IsCancel)
                        {
                            Download.AddDownloadTaskd(Uid, false);
                        }
                        return;
                    }
                });
            }

            public static byte[] TrimNullByte(byte[] oldByte)
            {
                var list = new List<byte>();
                list.AddRange(oldByte);
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i] == 0x00)
                        list.RemoveAt(i);
                    else
                        break;
                }
                var lastbyte = new byte[list.Count];
                for (var i = 0; i < list.Count; i++)
                {
                    lastbyte[i] = list[i];
                }
                return lastbyte;
            }
        }
    }
}
