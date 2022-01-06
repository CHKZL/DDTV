using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.DownloadModule;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static DDTV_Core.SystemAssembly.DownloadModule.DownloadClass;

namespace DDTV_WEB_Server.Controllers
{
    public class Rec_RecordingInfo_Text : ProcessingControllerBase.ApiControllerBase
    {
        
        [HttpGet(Name = "Rec_RecordingInfo_Text")]
        public string get()
        {
            string _= "UID　　　房间号　　　昵称　　　标题　　　已下载大小";
            foreach (var A1 in Rooms.RoomInfo)
            {
                if (A1.Value.DownloadingList.Count > 0)
                {
                    ulong FileSize = 0;
                    foreach (var item in A1.Value.DownloadingList)
                    {
                        FileSize += (ulong)item.TotalDownloadCount;
                    }

                    _+=($"{A1.Value.uid}  {A1.Value.room_id}  {A1.Value.uname}  {A1.Value.title}  {NetClass.ConversionSize(FileSize)}\n\r");
                }
            }
            if(Rooms.RoomInfo.Count==0)
            {
                _ = "当前无下载任务";
            }
            return _;
        }
    }

    public class Rec_RecordingInfo : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "Rec_RecordingInfo")]
        public string post([FromForm] string cmd)
        {
            List<Downloads> downloads = new List<Downloads>();
            foreach (var A1 in Rooms.RoomInfo)
            {
                if (A1.Value.DownloadingList.Count > 0)
                {
                    foreach (var item in A1.Value.DownloadingList)
                    {
                        Downloads _ = new()
                        {
                            DownloadCount=item.DownloadCount,
                            EndTime=item.EndTime,
                            FileName=item.FileName,
                            FilePath=item.FilePath,
                            FlvSplit=item.FlvSplit,
                            FlvSplitSize=item.FlvSplitSize,
                            IsDownloading=item.IsDownloading,
                            Name=item.Name,
                            RoomId=item.RoomId,
                            StartTime=item.StartTime,
                            Status=item.Status,
                            Title=item.Title,
                            Token=item.Token,
                            TotalDownloadCount=item.TotalDownloadCount,
                            Uid=item.Uid,
                            Url =item.Url,
                            FlvHeader=null,
                            FlvScriptTag=null,
                            flvTimes=null,
                            HttpWebRequest=null,
                        };

                        downloads.Add(_);
                    }
                }
            }   
            return MessageBase.Success(nameof(Rec_RecordingInfo), downloads);
        }
    }
    public class Rec_RecordingInfo_Lite : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "Rec_RecordingInfo_Lite")]
        public string post([FromForm] string cmd)
        {
            List<LiteDownloads> downloads = new List<LiteDownloads>();
            foreach (var A1 in Rooms.RoomInfo)
            {
                if (A1.Value.DownloadingList.Count > 0)
                {
                    foreach (var item in A1.Value.DownloadingList)
                    {
                        LiteDownloads _ = new()
                        {
                            EndTime = DDTV_Core.Tool.TimeModule.Time.Operate.DateTimeToConvertTimeStamp(item.EndTime),
                            FilePath = item.FilePath,
                            RoomId = item.RoomId,
                            StartTime = DDTV_Core.Tool.TimeModule.Time.Operate.DateTimeToConvertTimeStamp(item.StartTime),
                            Title = item.Title,
                            Token = item.Token,
                            TotalDownloadCount = item.TotalDownloadCount,
                            Uid = item.Uid,
                        };

                        downloads.Add(_);
                    }
                }
            }
            return MessageBase.Success(nameof(Rec_RecordingInfo), downloads);
        }
    }
    public class Rec_RecordCompleteInfon : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "Rec_RecordCompleteInfon")]
        public string post([FromForm] string cmd)
        {
            List<Downloads> downloads = new List<Downloads>();
            foreach (var A1 in Rooms.RoomInfo)
            {
                if (A1.Value.DownloadedLog.Count > 0)
                {
                    foreach (var item in A1.Value.DownloadedLog)
                    {
                        Downloads _ = new()
                        {
                            DownloadCount = item.DownloadCount,
                            EndTime = item.EndTime,
                            FileName = item.FileName,
                            FilePath = item.FilePath,
                            FlvSplit = item.FlvSplit,
                            FlvSplitSize = item.FlvSplitSize,
                            IsDownloading = item.IsDownloading,
                            Name = item.Name,
                            RoomId = item.RoomId,
                            StartTime = item.StartTime,
                            Status = item.Status,
                            Title = item.Title,
                            Token = item.Token,
                            TotalDownloadCount = item.TotalDownloadCount,
                            Uid = item.Uid,
                            Url = item.Url,
                            FlvHeader = null,
                            FlvScriptTag = null,
                            flvTimes = null,
                            HttpWebRequest = null,
                        };
                        downloads.Add(_);
                    }      
                }
            }
            return MessageBase.Success(nameof(Rec_RecordCompleteInfon), downloads);
        }
    }
    public class Rec_RecordCompleteInfon_Lite : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "Rec_RecordCompleteInfon_Lite")]
        public string post([FromForm] string cmd)
        {
            List<LiteDownloads> downloads = new List<LiteDownloads>();
            foreach (var A1 in Rooms.RoomInfo)
            {
                if (A1.Value.DownloadedLog.Count > 0)
                {
                    foreach (var item in A1.Value.DownloadedLog)
                    {
                        LiteDownloads _ = new()
                        {
                            EndTime = DDTV_Core.Tool.TimeModule.Time.Operate.DateTimeToConvertTimeStamp(item.EndTime),
                            FilePath = item.FilePath,
                            RoomId = item.RoomId,
                            StartTime = DDTV_Core.Tool.TimeModule.Time.Operate.DateTimeToConvertTimeStamp(item.StartTime),
                            Title = item.Title,
                            Token = item.Token,
                            TotalDownloadCount = item.TotalDownloadCount,
                            Uid = item.Uid,   
                        };
                        downloads.Add(_);
                    }
                }
            }
            return MessageBase.Success(nameof(Rec_RecordCompleteInfon), downloads);
        }
      
    }
    public class Rec_CancelDownload : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "Rec_CancelDownload")]
        public string post([FromForm] string cmd,[FromForm]long UID)
        {
            if(UID!=0)
            {
                if (Download.CancelDownload(UID))
                {
                    
                    return MessageBase.Success(nameof(Rec_RecordCompleteInfon), $"已取消UID[{UID}]的录制任务", $"已取消UID[{UID}]的录制任务");
                }
                else
                {
                    return MessageBase.Success(nameof(Rec_RecordCompleteInfon), $"取消UID[{UID}]的录制任务出现未知问题，取消失败", $"取消UID[{UID}]的录制任务出现未知问题，取消失败", MessageBase.code.OperationFailed);
                }
                
            }
            else
            {
                return MessageBase.Success(nameof(Rec_RecordCompleteInfon), "输入的UID不正确","输入的UID不正确",MessageBase.code.UIDFailed);
            }
           
        }
    }
    public class LiteDownloads
    {
        public string Token { get; set; }
        /// <summary>
        /// 房间号
        /// </summary>
        public string RoomId { get; set; }
        /// <summary>
        /// 用户UID
        /// </summary>
        public long Uid { set; get; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 文件夹路径
        /// </summary>
        public string FilePath { set; get; }
        /// <summary>
        /// 开始时间(秒，Unix时间戳)
        /// </summary>
        public long StartTime { set; get; }
        /// <summary>
        /// 结束时间(秒，Unix时间戳)
        /// </summary>
        public long EndTime { set; get; }
        /// <summary>
        /// 该任务下所有子任务的总下载字节数
        /// </summary>
        public long TotalDownloadCount { get; set; }
    }
}
