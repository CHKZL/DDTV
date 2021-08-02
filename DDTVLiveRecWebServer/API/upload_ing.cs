using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer.API
{
    public class upload_ing
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "upload_ing");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge>((int)ReturnInfoPackage.MessgeCode.鉴权失败, null);
            }
            else
            {
                List<Auxiliary.Upload.UploadTask.UploadInfo> A = new List<Auxiliary.Upload.UploadTask.UploadInfo>();
                foreach (var item1 in Auxiliary.Upload.Uploader.UploadList)
                {
                    foreach (var item2 in item1.status)
                    {
                        if(item2.Value.statusCode!=0&& item2.Value.statusCode != -1)
                        {
                            A.Add(item1);
                        }
                    }
                }
                return ReturnInfoPackage.InfoPkak((int)ReturnInfoPackage.MessgeCode.请求成功, A);
            }
        }
        private class Messge
        {
            public static List<Auxiliary.Upload.UploadTask.UploadInfo> Package { set; get; }
        }
    }
}
