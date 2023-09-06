using BiliAccount;
using DDTV_Core.SystemAssembly.BilibiliModule.API;
using DDTV_Core.SystemAssembly.BilibiliModule.User;
using DDTV_Core.SystemAssembly.Log;
using DDTV_Core.SystemAssembly.NetworkRequestModule.WebHook;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.ConfigModule
{
    public class BilibiliUserConfig
    {
        private static string UserConfigFile = "BiliUser.ini";
        public static Account AccClass = new();
        public static CookieInfo account = new CookieInfo();
        public static List<_FansMedal> FansMedal = new List<_FansMedal>();

        public class _FansMedal
        {
            /// <summary>
            /// 主播UID
            /// </summary>
            public long liver_uid { get; set; }
            /// <summary>
            /// 主播昵称
            /// </summary>
            public string liver_name { get; set; }
            /// <summary>
            /// 房间号
            /// </summary>
            public long roomid { get; set; }
            /// <summary>
            /// 牌子等级
            /// </summary>
            public int level { get; set; }
            /// <summary>
            /// 牌子名字
            /// </summary>
            public string medal_name { get; set; }
            /// <summary>
            /// 牌子ID
            /// </summary>
            public long medal_id { get; set; }
        }

        public class CookieInfo
        {
            /// <summary>
            /// 登陆后拼接计算得出的cookie字符串
            /// </summary>
            public string cookie { set; get; }
            /// <summary>
            /// 根据扫码登陆返回信息获得的本cookie有效期
            /// </summary>
            public DateTime ExTime { set; get; }
            /// <summary>
            /// csrf信息
            /// </summary>
            public string csrf { set; get; }
            /// <summary>
            /// 对应的账号UID
            /// </summary>
            public string uid { set; get; }
            /// <summary>
            /// 设备id
            /// </summary>
            public string buvid { set; get; }
            /// <summary>
            /// 当前登陆状态
            /// </summary>
            public LoginStatus loginStatus { set; get; }= LoginStatus.NotLoggedIn;
        }
        public enum LoginStatus
        {
            /// <summary>
            /// 未登录
            /// </summary>
            NotLoggedIn = 0,
            /// <summary>
            /// 已登陆
            /// </summary>
            LoggedIn = 1,
            /// <summary>
            /// 登陆失效
            /// </summary>
            LoginFailure = 2,
            /// <summary>
            /// 登陆中
            /// </summary>
            LoggingIn = 3
        }
        public static bool Init(InitDDTV_Core.SatrtType satrtType = InitDDTV_Core.SatrtType.DDTV_Core)
        {
            Log.Log.AddLog(nameof(BilibiliUserConfig), Log.LogClass.LogType.Info, "加载Userinfo文件");
            ReadUserFile();
            if (!string.IsNullOrEmpty(account.csrf) && !string.IsNullOrEmpty(account.uid) && !string.IsNullOrEmpty(account.cookie) && account.ExTime > DateTime.UtcNow)
            {
                account.loginStatus = LoginStatus.LoggedIn;
                return true;
            }
            else
            {
                account.loginStatus = LoginStatus.LoginFailure;
                return login.VerifyLogin.QRLogin(satrtType);
            }
        }

        public static string GetBuVid()
        {
            string result = NetworkRequestModule.Get.Get.GetRequest("https://api.bilibili.com/x/frontend/finger/spi");
            JObject JO = (JObject)JsonConvert.DeserializeObject(result);
            if (!string.IsNullOrEmpty(JO["code"].ToString()))
            {
                if (JO["code"].ToString() == "0")
                {
                    if (!string.IsNullOrEmpty(JO["data"]["b_3"].ToString()))
                    {
                        return JO["data"]["b_3"].ToString();
                    }
                }
            }
            var T = new HttpClient().GetAsync("https://data.bilibili.com/v/").Result;
            return T.Headers.GetValues("Set-Cookie").First().Split(';').First().Split('=').Last();
        }
        public static bool WritUserFile()
        {
            using FileStream fileStream = File.Create(UserConfigFile);
            {
                fileStream.Write(Encoding.UTF8.GetBytes($"cookie={EncryptionModule.Encryption.AesStr(account.cookie)}" + "\r\n"));
                fileStream.Write(Encoding.UTF8.GetBytes($"ExTime={account.ExTime.ToString("yyyy-MM-dd HH:mm:ss")}" + "\r\n"));
                fileStream.Write(Encoding.UTF8.GetBytes($"csrf={account.csrf}" + "\r\n"));
                fileStream.Write(Encoding.UTF8.GetBytes($"uid={account.uid}" + "\r\n"));
                fileStream.Write(Encoding.UTF8.GetBytes($"buvid={account.buvid}" + "\r\n"));
            }
            return true;
        }
        /// <summary>
        /// 读取User信息文件
        /// </summary>
        /// <param name="BiliUserFile">User配置文件的路径</param>
        /// <returns></returns>
        internal static bool ReadUserFile()
        {
            if (File.Exists(UserConfigFile))
            {
                string[] UserFileLine = File.ReadAllLines(UserConfigFile);
                if (UserFileLine.Length>0||UserFileLine[0].Length>1)
                {
                    account.cookie=String.Empty;
                    account.csrf=String.Empty;
                    account.ExTime=DateTime.MinValue;
                    account.uid=String.Empty;
                    account.buvid=String.Empty;
                    foreach (var item in UserFileLine)
                    {
                        if (item.Split('=').Length>1)
                        {
                            switch (item.Split('=')[0])
                            {
                                case "cookie":
                                    {
                                        try
                                        {
                                            account.cookie = EncryptionModule.Encryption.UnAesStr(item.Split('=')[1]);
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Log.AddLog(nameof(BilibiliUserConfig), Log.LogClass.LogType.Error, "读取BiliUser配置文件[cookie]出现AES错误！",true,e);
                                        }
                                        break;
                                    }
                                case "ExTime":
                                    {
                                        if (DateTime.TryParse(item.Split('=')[1], out DateTime ExTime))
                                        {
                                            account.ExTime=ExTime;
                                        }
                                        break;
                                    }
                                case "csrf":
                                    {
                                        account.csrf = item.Split('=')[1];
                                        break;
                                    }
                                case "uid":
                                    {
                                        account.uid = item.Split('=')[1];
                                        break;
                                    }
                                case "buvid":
                                    {
                                        account.buvid = item.Split('=')[1];
                                        break;
                                    }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(account.csrf)&&!string.IsNullOrEmpty(account.uid)&&!string.IsNullOrEmpty(account.cookie)&&account.ExTime>DateTime.UtcNow)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public class CheckAccount
        {
            public static event EventHandler<EventArgs> CheckAccountChanged;
            private static bool IsOn = false;
            public static bool IsState = true;
            /// <summary>
            /// 账号登陆状态监测间隔时间
            /// </summary>
            private static int IntervalTime = 3600 * 1 * 1000;
            /// <summary>
            /// 检查账号有效性
            /// </summary>
            public static void CheckLoginValidity()
            {
                if (!IsOn)
                { 
                    IsOn = !IsOn;
                    Log.Log.AddLog(nameof(BilibiliUserConfig), LogClass.LogType.Info, $"NAV状态开始持续监测...");
                    Task.Run(() =>
                    {
                        Thread.Sleep(8 * 1000);
                        while (true)
                        {
                            if (IsState)
                            {
                                try
                                {
                                    if (!BilibiliModule.API.UserInfo.LoginValidityVerification())
                                    {
                                        if (CheckAccountChanged != null)
                                        {
                                            CheckAccountChanged.Invoke(null, EventArgs.Empty);
                                            IsState = false;
                                        }
                                    }
                                    else
                                    {
                                        if (account!=null && account.ExTime!=DateTime.MinValue)
                                        {
                                            if(account.ExTime<DateTime.Now.AddDays(7))
                                            {
                                                WebHook.SendHook(WebHook.HookType.LoginWillExpireSoon, 0);
                                            }
                                        }
                                        
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Log.AddLog(nameof(CheckAccount), LogClass.LogType.Error, "验证账号有效性出现意外错误", true, e);
                                }
                            }
                           
                            //BilibiliUserConfig.FansMedal = UserInfo.fansMedal.GetFansMedal(long.Parse(BilibiliUserConfig.account.uid));
                            //string B = JsonConvert.SerializeObject(BilibiliUserConfig.FansMedal);
                            Thread.Sleep(IntervalTime);
                        }
                    });
                }
                else
                {
                    if (!BilibiliModule.API.UserInfo.LoginValidityVerification())
                    {
                        if (CheckAccountChanged != null)
                        {
                            CheckAccountChanged.Invoke(null, EventArgs.Empty);
                            IsState = false;
                        }
                    }
                }
            }
        }
    }
}
