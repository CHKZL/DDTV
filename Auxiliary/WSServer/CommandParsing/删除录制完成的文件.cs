using Auxiliary.RequestMessage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessage.File;
using static Auxiliary.RequestMessage.MessageClass;

namespace Auxiliary.WSServer.CommandParsing
{
    internal class 删除录制完成的文件
    {
        internal static string 删除文件(string mess)
        {
            FileInfo File = new FileInfo();
            try
            {
                JObject JO = (JObject)JsonConvert.DeserializeObject(mess);
                File.Directory = JO["Directory"].ToString();
                File.Name = JO["Name"].ToString();
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak<Message<FileDeleteInfo>>((int)ServerSendMessageCode.请求成功但出现了错误, null, "服务器收到的数据不符合消息解析的必要条件，请检查数据格式");
            }
            return Auxiliary.RequestMessage.封装消息.文件_删除录制的文件.删除(File.Directory, File.Name);
        }
        internal class FileInfo
        { 
            internal string Directory { set; get; }
            internal string Name { set; get; }
        }
    }
}
