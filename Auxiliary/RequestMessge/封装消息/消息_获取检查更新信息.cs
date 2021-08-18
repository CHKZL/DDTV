using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessage.MessageClass;
using static Auxiliary.RequestMessage.System_Core;

namespace Auxiliary.RequestMessage.封装消息
{
    public class 消息_获取检查更新信息
    {
        public static string 检查更新信息()
        {
            SystemUpdateInfo updateInfo = new SystemUpdateInfo()
            {
                IsNewVer = MMPU.是否有新版本,
                NewVer = MMPU.是否有新版本 ? MMPU.检测到的新版本号 : null,
                Update_Log = MMPU.是否有新版本 ? MMPU.更新公告 : null,
            };
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功, new List<SystemUpdateInfo>() { updateInfo });
        }
    }
}
