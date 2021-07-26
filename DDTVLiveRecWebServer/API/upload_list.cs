using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer.API
{
    public class upload_list
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "upload_list");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge>(鉴权结果, null);
            }
            else
            {
                return ReturnInfoPackage.InfoPkak(鉴权结果, Auxiliary.Upload.Uploader.UploadList);
            }
        }
        private class Messge
        {
            public static List<Auxiliary.Upload.UploadTask.UploadInfo> Package { set; get; }
        }
    }
}
