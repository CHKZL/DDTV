using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer.API
{
    public class file_delete
    {
        public static string Web(HttpContext context)
        {
            bool 鉴权预处理结果 = false;
            foreach (var item in new List<string>() {
                context.Request.Form["Directory"],
                context.Request.Form["Name"],
                context.Request.Form["RoomId"]
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
                return ReturnInfoPackage.InfoPkak<Messge>((int)ReturnInfoPackage.MessgeCode.鉴权失败, null);
            }
            else
            {
                try
                {
                    if (Directory.Exists(Auxiliary.MMPU.缓存路径 + context.Request.Form["Directory"]))
                    {
                        if (File.Exists(Auxiliary.MMPU.缓存路径 + context.Request.Form["Directory"] + "\\" + context.Request.Form["Name"]))
                        {
                            Auxiliary.MMPU.文件删除委托(Auxiliary.MMPU.缓存路径 + context.Request.Form["Directory"] + "\\" + context.Request.Form["Name"], "来自API接口'file_delete'的请求，删除文件");
                            return ReturnInfoPackage.InfoPkak((int)ReturnInfoPackage.MessgeCode.请求成功, new List<FileInfo>() { new FileInfo() { result = true, messge = "文件已提加入删除委托列表，等待文件锁解锁后自动删除" } });
                        }
                        else
                        {
                            return ReturnInfoPackage.InfoPkak((int)ReturnInfoPackage.MessgeCode.请求成功但出现了错误, new List<FileInfo>() { new FileInfo() { result = false } }, "该文件不存在");
                        }
                    }
                    else
                    {
                        return ReturnInfoPackage.InfoPkak((int)ReturnInfoPackage.MessgeCode.请求成功但出现了错误, new List<FileInfo>() { new FileInfo() { result = false } }, "路径有误");
                    }
                }
                catch (Exception)
                {
                    return ReturnInfoPackage.InfoPkak((int)ReturnInfoPackage.MessgeCode.请求成功但出现了错误, new List<FileInfo>() { new FileInfo() { result = false } }, "文件路径错误或者格式符合法！");
                }
            }
        }
        private class Messge : ReturnInfoPackage.Messge<FileInfo>
        {
            public static new List<FileInfo> Package { set; get; }
        }
        private class FileInfo
        {
            public bool result { set; get; }
            public string messge { set; get; } = null;
        }
    }
}
