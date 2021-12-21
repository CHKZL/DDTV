using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static DDTV_Core.SystemAssembly.DownloadModule.DownloadClass;

namespace DDTV_WEB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecordingInfo_Text : ControllerBase
    {
        [HttpGet(Name = "RecordingInfo_Text")]
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
    [Route("api/[controller]")]
    [ApiController]
    public class RecordingInfo : ControllerBase
    {
        [HttpGet(Name = "RecordingInfo")]
        public string get()
        {
            List<Downloads> downloads = new List<Downloads>();
            foreach (var A1 in Rooms.RoomInfo)
            {
                if (A1.Value.DownloadingList.Count > 0)
                {
                    foreach (var item in A1.Value.DownloadingList)
                    {
                        Downloads _ = item;
                        _.flvTimes = null;
                        _.FlvHeader = null;
                        _.FlvScriptTag = null;
                        _.HttpWebRequest = null;
                        downloads.Add(_);
                    }
                }
            }
            return MessageBase.Success(nameof(Transcod), downloads);
        }
    }
    [Route("api/[controller]")]
    [ApiController]
    public class RecordComplete : ControllerBase
    {
        [HttpGet(Name = "RecordComplete")]
        public string get()
        {
            List<Downloads> downloads = new List<Downloads>();
            foreach (var A1 in Rooms.RoomInfo)
            {
                if (A1.Value.DownloadedLog.Count > 0)
                {
                    foreach (var item in A1.Value.DownloadedLog)
                    {
                        Downloads _ = item;
                        _.flvTimes = null;
                        _.FlvHeader = null;
                        _.FlvScriptTag = null;
                        _.HttpWebRequest = null;
                        downloads.Add(_);
                    }      
                }
            }
            return MessageBase.Success(nameof(Transcod), downloads);
        }
    }
}
