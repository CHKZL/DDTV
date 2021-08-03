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
    public class rec_processing_list
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "rec_processing_list");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge<RecLProcessinist>>((int)ServerSendMessgeCode.鉴权失败, null);
            }
            else
            {
                return Auxiliary.RequestMessge.封装消息.获取当前录制中的任务队列简报信息.当前录制中的任务队列简报信息();
            }
        }
     
    }
}
