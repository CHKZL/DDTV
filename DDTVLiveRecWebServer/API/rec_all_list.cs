using Auxiliary.RequestMessge;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Auxiliary.RequestMessge.MessgeClass;
using static Auxiliary.RequestMessge.Rec;

namespace DDTVLiveRecWebServer.API
{
    public class rec_all_list
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "rec_all_list");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge<RecAllList>>((int)ServerSendMessgeCode.鉴权失败, null);
            }
            else
            {
                return Auxiliary.RequestMessge.封装消息.获取所有下载任务的简报队列信息.所有下载任务的简报队列信息();
            }
        }
    }
}
