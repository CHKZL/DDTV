using Auxiliary.RequestMessge;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessge.File;
using static Auxiliary.RequestMessge.MessgeClass;

namespace Auxiliary.WSServer.CommandParsing
{
    internal class 根据房间号获得录制的文件列表
    {
        internal static string 获得文件列表(string mess)
        {
            FileList fileList = new FileList();
            try
            {
                fileList = JsonConvert.DeserializeObject<FileList>(mess);
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak<Messge<FileRangeInfo>>((int)ServerSendMessgeCode.请求成功但出现了错误, null, "服务器收到的数据不符合消息解析的必要条件，请检查数据格式");
            }
            return Auxiliary.RequestMessge.封装消息.根据房间号获取录制的文件列表.获取文件列表(fileList.RoomId);
        }
        internal class FileList
        { 
            internal string RoomId { set; get; }
        }
    }
}
