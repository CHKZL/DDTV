using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;

namespace DDTVLiveRecWebServer.API
{
    public class file_lists
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "file_lists");
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
