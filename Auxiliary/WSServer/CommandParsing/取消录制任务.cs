using Auxiliary.RequestMessge;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.Downloader;
using static Auxiliary.RequestMessge.MessgeClass;
using static Auxiliary.WSServer.WSServer;

namespace Auxiliary.WSServer.CommandParsing
{
    internal class 取消录制任务
    {
        internal static string wss取消录制任务(string mess)
        {
            RecInfo Rec = new RecInfo();
            try
            {
                JObject JO = (JObject)JsonConvert.DeserializeObject(mess);
                Rec.GUID = JO["GUID"].ToString();
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak<Messge<DownIofoData>>((int)ServerSendMessgeCode.请求成功但出现了错误, null, "服务器收到的数据不符合消息解析的必要条件，请检查数据格式");
            }
            return RequestMessge.封装消息.执行取消录制任务.取消录制任务(Rec.GUID);
        }
        internal class RecInfo 
        {
            internal string GUID { set; get; } 
        }
    }
}
