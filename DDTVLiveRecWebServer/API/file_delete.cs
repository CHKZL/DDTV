using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Auxiliary.RequestMessge.MessgeClass;
using static Auxiliary.RequestMessge.File;
using FileDeleteInfo = Auxiliary.RequestMessge.File.FileDeleteInfo;
using static Auxiliary.RequestMessge.ReturnInfoPackage;

namespace DDTVLiveRecWebServer.API
{
    public class file_delete
    {
        public static string Web(HttpContext context)
        {
            bool 鉴权预处理结果 = false;
            foreach (var item in new List<string>() {
                context.Request.Form["Directory"],
                context.Request.Form["Name"]
            })
            {
                if (string.IsNullOrEmpty(item))
                {
                    鉴权预处理结果 = true;
                    break;
                }
            };
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "file_delete", 鉴权预处理结果 ? true : false);
            if (!鉴权结果.鉴权结果)
            {
                return InfoPkak<Messge<FileDeleteInfo>>((int)ServerSendMessgeCode.鉴权失败, null);
            }
            else
            {
                return Auxiliary.RequestMessge.封装消息.删除录制的文件.删除(context.Request.Form["Directory"], context.Request.Form["Name"]);
            }
        }

    }
}
