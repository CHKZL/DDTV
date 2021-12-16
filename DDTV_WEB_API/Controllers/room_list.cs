using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace DDTV_WEB_API.Controllers
{
   
    
    public class room_list : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "room_list")]
        [Consumes("application/json")]
        public string Post()
        {
            //Response.ContentType = "application/json";
            Dictionary<long, RoomInfoClass.RoomInfo> keyValuePairs = Rooms.RoomInfo;
            foreach (var pair in keyValuePairs)
            {
                pair.Value.roomWebSocket = null;
                pair.Value.DownloadedLog = null;
                pair.Value.DownloadingList = null;
                pair.Value.DanmuFile = null;
            }
            return MessageBase.Success(nameof(room_list), keyValuePairs);
        }
    }
}