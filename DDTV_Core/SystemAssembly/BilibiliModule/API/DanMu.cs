using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API
{
    internal class DanMu
    {
        /// <summary>
        /// 发送直播间弹幕
        /// </summary>
        /// <param name="roomId">房间号(长房间号)</param>
        /// <param name="Message">发送信息(不能超过20个字符)</param>
        internal static void send(string roomId,string Message)
        {
            CookieContainer CK = NetworkRequestModule.NetClass.CookieContainerTransformation(User.BilibiliUser.account.cookie);
            Dictionary<string, string> Params = new Dictionary<string, string>
                {
                    { "color", "16777215" },
                    { "fontsize", "25" },
                    { "mode", "1" },
                    { "msg", Message },
                    { "rnd", (DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1))).TotalSeconds.ToString() },
                    { "roomid", roomId },
                    { "csrf_token", User.BilibiliUser.account.csrf },
                    { "csrf", User.BilibiliUser.account.csrf }
                };
            JObject JO = (JObject)JsonConvert.DeserializeObject(NetworkRequestModule.Post.Post.SendRequest_SendDanmu("https://api.live.bilibili.com/msg/send", Params, CK));
            Log.Log.AddLog(nameof(DanMu), Log.LogClass.LogType.Debug, $"弹幕发送成功");
        }
    }
}
