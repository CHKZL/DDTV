using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer.API
{
    public class system_update
    {
        public static string Web(HttpContext context)
        {
            var 鉴权结果 = 鉴权.Authentication.API接口鉴权(context, "system_update");
            if (!鉴权结果.鉴权结果)
            {
                return ReturnInfoPackage.InfoPkak<Messge>((int)ReturnInfoPackage.MessgeCode.鉴权失败, null);
            }
            else
            {
                updateInfo updateInfo = new updateInfo()
                {
                    IsNewVer = Auxiliary.MMPU.是否有新版本,
                    NewVer = Auxiliary.MMPU.是否有新版本 ? Auxiliary.MMPU.检测到的新版本号 : null,
                    Update_Log = Auxiliary.MMPU.是否有新版本 ? Auxiliary.MMPU.更新公告 : null,
                };
                return ReturnInfoPackage.InfoPkak((int)ReturnInfoPackage.MessgeCode.请求成功, new List<updateInfo>() { updateInfo });
            }

        }

        private class Messge : ReturnInfoPackage.Messge<updateInfo>
        {
            public static new List<updateInfo> Package { set; get; }

        }
        public class updateInfo
        {
            /// <summary>
            /// 是否有新版本
            /// </summary>
            public bool IsNewVer { set; get; }
            /// <summary>
            /// 新版本号
            /// </summary>
            public string NewVer { get; set; }
            /// <summary>
            /// 更新日志
            /// </summary>
            public string Update_Log { get; set; }

        }
    }
}
