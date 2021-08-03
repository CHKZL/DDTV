using Auxiliary.RequestMessge;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessge.File;
using static Auxiliary.RequestMessge.MessgeClass;

namespace Auxiliary.WSServer.CommandParsing
{
    internal class 删除录制完成的文件
    {
        internal static string 删除文件(string mess)
        {
            FileInfo File = new FileInfo();
            try
            {
                File = JsonConvert.DeserializeObject<FileInfo>(mess);
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak<Messge<FileDeleteInfo>>((int)ServerSendMessgeCode.请求成功但出现了错误, null, "服务器收到的数据不符合消息解析的必要条件，请检查数据格式");
            }
            return Auxiliary.RequestMessge.封装消息.删除录制的文件.删除(File.Directory, File.Name);
        }
        internal class FileInfo
        { 
            internal string Directory { set; get; }
            internal string Name { set; get; }
        }
    }
}
