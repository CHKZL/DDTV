using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer.API
{
    public class file_range
    {
        public static string Web(HttpContext context)
        {
            bool 鉴权预处理结果 = false;
            foreach (var item in new List<string>() {
                context.Request.Form["RoomId"]
            })
            {
                if (string.IsNullOrEmpty(item))
                {
                    鉴权预处理结果 = true;
                    break;
                }
            };
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "file_range", 鉴权预处理结果 ? true : false);
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge>(鉴权结果, null);
            }
            else
            {
                List<FileInfo> fileInfos = new List<FileInfo>();
                foreach (var Dir in new DirectoryInfo(Auxiliary.MMPU.缓存路径).GetDirectories())
                {
                    foreach (var File in new DirectoryInfo(Auxiliary.MMPU.缓存路径 + Dir.Name).GetFiles())
                    {
                        if (Dir.Name.Split('_')[Dir.Name.Split('_').Length - 1] == context.Request.Form["RoomId"])
                        {
                            fileInfos.Add(new FileInfo()
                            {
                                Directory = Dir.Name,
                                ModifiedTime = File.CreationTime,
                                Name = File.Name,
                                Size = File.Length,
                                Path = Auxiliary.MMPU.缓存路径 + Dir.Name + "/" + File.Name
                            });
                        }
                    }
                }
                return ReturnInfoPackage.InfoPkak(鉴权结果, fileInfos);
            }
        }
        private class Messge
        {
            public static List<FileInfo> Package { set; get; }
        }
        private class FileInfo
        {
            public long Size { set; get; }
            public string Name { set; get; }
            public string Directory { set; get; }
            public string Path { set; get; }
            public DateTime ModifiedTime { set; get; }
        }
    }
}
