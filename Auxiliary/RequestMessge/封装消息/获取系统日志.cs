using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Auxiliary.InfoLog;
using static Auxiliary.RequestMessage.MessageClass;

namespace Auxiliary.RequestMessage.封装消息
{
    public class 获取系统日志
    {
        public static string 日志(int count)
        {
            List<LogInfo> logs = new List<LogInfo>();
            if (count > 0)
            {
                logs = (List<LogInfo>)logInfos.Skip(Math.Max(0, logInfos.Count - count));
            }
            else
            {
                logs = logInfos;
            }

            return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功, logs);
        }
    }
}
