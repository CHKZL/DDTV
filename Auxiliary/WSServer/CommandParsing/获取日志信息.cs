using Auxiliary.RequestMessge;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auxiliary.WSServer.CommandParsing
{
    internal class 获取日志信息
    {
        internal static string 获取日志(string mess)
        {
            日志 Log = new 日志();
            int count = 0;
            try
            {
                JObject JO = (JObject)JsonConvert.DeserializeObject(mess);
                int.TryParse(JO["count"].ToString(), out count);
                Log.count = count;
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak((int)MessgeClass.ServerSendMessgeCode.请求成功但出现了错误, new List<InfoLog.LogInfo>() { new ServerClass.Login() { messge = "服务器收到的数据不符合消息解析的必要条件，请检查数据格式", result = false } });
            }
            return RequestMessge.封装消息.获取系统日志.日志(count);
        }
        internal class 日志
        {
            internal int count { set; get; }
        }
    }
}
