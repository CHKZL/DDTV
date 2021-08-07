using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessge.MessgeClass;

namespace Auxiliary.RequestMessge.封装消息
{
    public class 修改配置_自动转码
    {
        public static string 转码(bool T)
        {
            MMPU.转码功能使能 = T;
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功, new List<Config.AutoTranscoding>() { new Config.AutoTranscoding() { result = true, messge = $"修改转码设置成功，当前转码使能为{T}" } });
        }
    }
}
