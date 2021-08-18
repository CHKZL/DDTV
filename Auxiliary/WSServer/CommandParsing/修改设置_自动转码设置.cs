using Auxiliary.RequestMessage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessage.Config;
using static Auxiliary.RequestMessage.MessageClass;

namespace Auxiliary.WSServer.CommandParsing
{
    internal class 修改设置_自动转码设置
    {
        internal static string 转码设置(string mess)
        {
            ATinfo aTinfo = new ATinfo();
            try
            {
                JObject JO = (JObject)JsonConvert.DeserializeObject(mess);
                aTinfo.transcoding_status = bool.Parse(JO["transcoding_status"].ToString());
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak<Message<AutoTranscoding>>((int)ServerSendMessageCode.请求成功但出现了错误, null, "服务器收到的数据不符合消息解析的必要条件，请检查数据格式");
            }
            return RequestMessage.封装消息.配置_自动转码.转码(aTinfo.transcoding_status);
        }
        internal class ATinfo
        {
            internal bool transcoding_status { set; get; }
        }
    }
}
