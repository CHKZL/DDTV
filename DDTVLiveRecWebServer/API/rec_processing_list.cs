using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer.API
{
    public class rec_processing_list
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "rec_processing_list");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge>((int)ReturnInfoPackage.MessgeCode.鉴权失败, null);
            }
            else
            {
                List<RecList> list = new List<RecList>();
                foreach (var item in Auxiliary.MMPU.DownList)
                {
                    if(item.DownIofo.下载状态)
                    {
                        list.Add(new RecList
                        {
                            RoomId = item.DownIofo.房间_频道号,
                            Name = item.DownIofo.主播名称,
                            StartTime = item.DownIofo.开始时间,
                            EndTime = item.DownIofo.结束时间,
                            Downloaded_bit = item.DownIofo.已下载大小bit,
                            Downloaded_str = item.DownIofo.已下载大小str,
                            GUID = item.DownIofo.事件GUID,
                        });
                    }
                }
                return ReturnInfoPackage.InfoPkak((int)ReturnInfoPackage.MessgeCode.请求成功, list);
            }
        }
        private class Messge : ReturnInfoPackage.Messge<RecList>
        {
            public static new List<RecList> Package { set; get; }

        }
        private class RecList
        {
            /// <summary>
            /// 房间号
            /// </summary>
            public string RoomId { set; get; }
            /// <summary>
            /// 已下载大小(bit)
            /// </summary>
            public double Downloaded_bit { set; get; }
            /// <summary>
            /// 已下载大小(换算后的大小:字符串格式)
            /// </summary>
            public string Downloaded_str { set; get; }
            /// <summary>
            /// 主播名称
            /// </summary>
            public string Name { set; get; }
            /// <summary>
            /// 开始下载时间
            /// </summary>
            public int StartTime { set; get; }
            /// <summary>
            /// 结束下载时间
            /// </summary>
            public int EndTime { set; get; }
            /// <summary>
            /// 下载任务唯一标识符
            /// </summary>
            public string GUID { set; get; }
        }
    }
}
