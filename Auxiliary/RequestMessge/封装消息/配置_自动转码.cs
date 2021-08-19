using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessage.MessageClass;

namespace Auxiliary.RequestMessage.封装消息
{
    public class 配置_自动转码
    {
        public static string 转码(bool T)
        {
            MMPU.转码功能使能 = T;
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功, new List<Config.AutoTranscoding>() { new Config.AutoTranscoding() { result = true, message = $"修改转码设置成功，当前转码使能为{T}" } });
        }
    }
}
