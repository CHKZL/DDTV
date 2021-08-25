using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Auxiliary.RequestMessage.File;
using static Auxiliary.RequestMessage.MessageClass;

namespace Auxiliary.RequestMessage.封装消息
{
    public class 下载_获取当前录制文件夹中的所有文件的列表信息
    {
        public static string 当前录制文件夹中的所有文件的列表信息()
        {
            List<FileListInfo> fileInfos = new List<FileListInfo>();
            foreach (var Dir in new DirectoryInfo(Auxiliary.MMPU.缓存路径).GetDirectories())
            {
                foreach (var File in new DirectoryInfo(Auxiliary.MMPU.缓存路径 + Dir.Name).GetFiles())
                {
                    fileInfos.Add(new FileListInfo()
                    {
                        Directory = Dir.Name,
                        ModifiedTime = File.CreationTime,
                        Name = File.Name,
                        Size = File.Length,
                        Path = MMPU.缓存路径 + Dir.Name + "/" + File.Name
                    });
                }
            }
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功, fileInfos);
        }
    }
}
