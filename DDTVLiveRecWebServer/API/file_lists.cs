using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using static Auxiliary.RequestMessge.File;
using static Auxiliary.RequestMessge.MessgeClass;
using static Auxiliary.RequestMessge.ReturnInfoPackage;

namespace DDTVLiveRecWebServer.API
{
    public class file_lists
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "file_lists");
            if (!鉴权结果.鉴权结果)
            {
                return InfoPkak<Messge<FileListInfo>>((int)ServerSendMessgeCode.鉴权失败, null);
            }
            else
            {
                return Auxiliary.RequestMessge.封装消息.获取当前录制文件夹中的所有文件的列表信息.当前录制文件夹中的所有文件的列表信息();
            }
        }
    }
}
