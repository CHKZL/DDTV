using BiliAccount;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.User
{
    internal class BilibiliUser
    {
        internal static Account account = new Account();
        public static CookieInfo cookie = new CookieInfo();
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
        }
        public static bool Init(InitDDTV_Core.SatrtType satrtType = InitDDTV_Core.SatrtType.DDTV_CLI)
        {
            ReadUserFile();
            if (!string.IsNullOrEmpty(cookie.csrf)&&!string.IsNullOrEmpty(cookie.uid)&&!string.IsNullOrEmpty(cookie.cookie)&&cookie.ExTime>DateTime.UtcNow)
            {
                return true;
            }
            else
            {
                login.VerifyLogin.Loing(satrtType);
                return true;
            }
        }
        internal static bool WritUserFile(string BiliUserFile = "./BiliUser.ini")
        {
            if (File.Exists(BiliUserFile))
            {
                File.Delete(BiliUserFile);      
            }
            File.WriteAllLines(BiliUserFile, new string[] {
                    $"cookie={EncryptionModule.Encryption.AesStr(cookie.cookie)}",
                    $"ExTime={cookie.ExTime.ToString("yyyy-MM-dd HH:mm:ss")}",
                    $"csrf={cookie.csrf}",
                    $"uid={cookie.uid}",
                }, Encoding.UTF8);
            return true;
        }
        /// <summary>
        /// 读取User信息文件
        /// </summary>
        /// <param name="BiliUserFile">User配置文件的路径</param>
        /// <returns></returns>
        internal static bool ReadUserFile(string BiliUserFile = "./BiliUser.ini")
        {
            if (File.Exists(BiliUserFile))
            {
                string[] UserFileLine = File.ReadAllLines(BiliUserFile);
                if (UserFileLine.Length>1||UserFileLine[0].Length>1)
                {
                    cookie.cookie=String.Empty;
                    cookie.csrf=String.Empty;
                    cookie.ExTime=DateTime.MinValue;
                    cookie.uid=String.Empty;
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
                                            cookie.cookie = EncryptionModule.Encryption.UnAesStr(item.Split('=')[1]);
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Log.AddLog(nameof(BilibiliUser), Log.LogClass.LogType.Error, "读取BiliUser配置文件[cookie]出现AES错误！",true,e);
                                        }
                                        break;
                                    }
                                case "ExTime":
                                    {
                                        if (DateTime.TryParse(item.Split('=')[1], out DateTime ExTime))
                                        {
                                            cookie.ExTime=ExTime;
                                        }
                                        break;
                                    }
                                case "csrf":
                                    {
                                        cookie.csrf = item.Split('=')[1];
                                        break;
                                    }
                                case "uid":
                                    {
                                        cookie.uid = item.Split('=')[1];
                                        break;
                                    }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(cookie.csrf)&&!string.IsNullOrEmpty(cookie.uid)&&!string.IsNullOrEmpty(cookie.cookie)&&cookie.ExTime>DateTime.UtcNow)
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
    }
}
