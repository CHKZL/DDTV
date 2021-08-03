using Auxiliary.RequestMessge;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessge.MessgeClass;
using static Auxiliary.WSServer.WSServer;

namespace Auxiliary.WSServer.CommandParsing
{
    internal class 获取录制任务详情
    {
        internal static string wss获取录制任务详情处理(string mess)
        {
            RecInfo Rec = new RecInfo();
            try
            {
                Rec = JsonConvert.DeserializeObject<RecInfo>(mess);
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak<Messge<Downloader.DownIofoData>>((int)ServerSendMessgeCode.请求成功但出现了错误,null, "服务器收到的数据不符合消息解析的必要条件，请检查数据格式");
            }
            return RequestMessge.封装消息.获取录制任务详情信息.录制任务详情信息(Rec.GUID);       
        }
        internal class RecInfo
        {
            internal string GUID { set; get; }
        }
    }
}
