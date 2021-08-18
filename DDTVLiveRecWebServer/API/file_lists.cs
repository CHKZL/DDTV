using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using static Auxiliary.RequestMessage.File;
using static Auxiliary.RequestMessage.MessageClass;
using static Auxiliary.RequestMessage.ReturnInfoPackage;

namespace DDTVLiveRecWebServer.API
{
    public class file_lists
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "file_lists");
            if (!鉴权结果.鉴权结果)
            {
                return InfoPkak<Message<FileListInfo>>((int)ServerSendMessageCode.鉴权失败, null, 鉴权结果.鉴权返回消息);
            }
            else
            {
                return Auxiliary.RequestMessage.封装消息.下载_获取当前录制文件夹中的所有文件的列表信息.当前录制文件夹中的所有文件的列表信息();
            }
        }
    }
}
