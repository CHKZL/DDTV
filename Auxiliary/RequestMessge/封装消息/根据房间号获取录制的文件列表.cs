using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Auxiliary.RequestMessage.File;
using static Auxiliary.RequestMessage.MessageClass;

namespace Auxiliary.RequestMessage.封装消息
{
    public class 根据房间号获取录制的文件列表
    {
        public static string 获取文件列表(string RoomId)
        {
            List<FileRangeInfo> fileInfos = new List<FileRangeInfo>();
            foreach (var Dir in new DirectoryInfo(Auxiliary.MMPU.缓存路径).GetDirectories())
            {
                foreach (var File in new DirectoryInfo(Auxiliary.MMPU.缓存路径 + Dir.Name).GetFiles())
                {
                    if (Dir.Name.Split('_')[Dir.Name.Split('_').Length - 1] == RoomId|| Dir.Name.Split('_')[0]==RoomId)
                    {
                        fileInfos.Add(new FileRangeInfo()
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
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功, fileInfos);
        }
    }
}
