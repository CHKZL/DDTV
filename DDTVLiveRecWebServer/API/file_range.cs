using Auxiliary.RequestMessge;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Auxiliary.RequestMessge.File;
using static Auxiliary.RequestMessge.MessgeClass;

namespace DDTVLiveRecWebServer.API
{
    public class file_range
    {
        public static string Web(HttpContext context)
        {
            bool 鉴权预处理结果 = false;
            foreach (var item in new List<string>() {
                context.Request.Form["RoomId"]
            })
            {
                if (string.IsNullOrEmpty(item))
                {
                    鉴权预处理结果 = true;
                    break;
                }
            };
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "file_range", 鉴权预处理结果 ? true : false);
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge<FileRangeInfo>>((int)ServerSendMessgeCode.鉴权失败, null);
            }
            else
            {
                return Auxiliary.RequestMessge.封装消息.根据房间号获取录制的文件列表.获取文件列表(context.Request.Form["RoomId"]);
            }
        }

    }
}
