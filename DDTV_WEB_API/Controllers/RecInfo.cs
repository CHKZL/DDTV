using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DDTV_WEB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecInfo : ControllerBase
    {
        [HttpGet(Name = "RecInfo")]
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
                        FileSize += (ulong)item.DownloadCount;
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
}
