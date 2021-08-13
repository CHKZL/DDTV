using Auxiliary.RequestMessage;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Auxiliary.RequestMessage.MessageClass;

namespace DDTVLiveRecWebServer.API
{
    public class upload_list
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "upload_list");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Message<Auxiliary.Upload.UploadTask.UploadInfo>>((int)ServerSendMessageCode.鉴权失败, null, 鉴权结果.鉴权返回消息);
            }
            else
            {
                return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功, Auxiliary.Upload.Uploader.UploadList);
            }
        }
    }
}
