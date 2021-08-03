using Auxiliary.RequestMessge;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Auxiliary.RequestMessge.MessgeClass;

namespace DDTVLiveRecWebServer.API
{
    public class upload_list
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "upload_list");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge<Auxiliary.Upload.UploadTask.UploadInfo>>((int)ServerSendMessgeCode.鉴权失败, null);
            }
            else
            {
                return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功, Auxiliary.Upload.Uploader.UploadList);
            }
        }
    }
}
