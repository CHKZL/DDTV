using Auxiliary.RequestMessage;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Auxiliary.RequestMessage.MessageClass;
using static Auxiliary.RequestMessage.Rec;

namespace DDTVLiveRecWebServer.API
{
    public class rec_all_list
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "rec_all_list");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Message<RecAllList>>((int)ServerSendMessageCode.鉴权失败, null, 鉴权结果.鉴权返回消息);
            }
            else
            {
                return Auxiliary.RequestMessage.封装消息.获取所有下载任务的简报队列信息.所有下载任务的简报队列信息();
            }
        }
    }
}
