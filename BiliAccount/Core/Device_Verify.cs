using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Net;

#if NETSTANDARD2_0 || NETCORE3_0
using Newtonsoft.Json;
#else

using System.Web.Script.Serialization;

#endif

#pragma warning disable CS0649

namespace BiliAccount.Core
{
    internal class Device_Verify
    {
        #region Private Properties

        /// <summary>
        /// Unix时间戳
        /// </summary>
        private static long TimeStamp
        {
            get
            {
                return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            }
        }

        #endregion Private Properties

        #region Public Methods

        /// <summary>
        /// 获取登录账号信息
        /// </summary>
        /// <param name="code">验证设备返回code</param>
        /// <param name="account">账号实例</param>
        /// <exception cref="Exceptions.GetAccount_Exception"/>
        public static void GetAccount(string code, ref Account account)
        {
            string param = $"appkey={Config.Instance.Appkey}&bili_local_id={account.DeviceId}&build={Config.Instance.Build}&channel=bili&code={code}&grant_type=authorization_code&local_id={account.Buvid}&mobi_app=android&platform=android&statistics=%7B%22appId%22%3A1%2C%22platform%22%3A3%2C%22version%22%3A%22{Config.Instance.Version}%22%2C%22abtest%22%3A%22%22%7D&ts={TimeStamp}";
            param += $"&sign={GetSign(param)}";
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("Buvid", account.Buvid);
            //string str = Http.GetBody($"https://passport.bilibili.com/api/v2/oauth2/access_token?{param}", null, "", Config.Instance.User_Agent, headers);
            string str = Http.GetBody($"https://passport.bilibili.com/api/v2/oauth2/access_token?{param}", null, "", $"BiliAccount/{Config.Dll_Version}", headers);
            if (!string.IsNullOrEmpty(str))
            {
#if NETSTANDARD2_0 || NETCORE3_0
                GetAccount_DataTemplete obj = JsonConvert.DeserializeObject<GetAccount_DataTemplete>(str);
#else
                GetAccount_DataTemplete obj = (new JavaScriptSerializer()).Deserialize<GetAccount_DataTemplete>(str);
#endif
                if (obj.code == 0)
                {
                    account.Uid = obj.data.token_info.mid;
                    account.AccessToken = obj.data.token_info.access_token;
                    account.RefreshToken = obj.data.token_info.refresh_token;
                    account.Expires_AccessToken = DateTime.Parse("1970-01-01 08:00:00").AddSeconds(obj.ts + obj.data.token_info.expires_in);

                    account.Cookies = new CookieCollection();
                    foreach (GetAccount_DataTemplete.Data_Templete.Cookie_Info_Templete.Cookie_Templete i in obj.data.cookie_info.cookies)
                    {
                        account.strCookies += i.name + "=" + i.value + "; ";
                        account.Cookies.Add(new Cookie(i.name, i.value) { Domain = ".bilibili.com" });
                        account.Expires_Cookies = DateTime.Parse("1970-01-01 08:00:00").AddSeconds(i.expires);

                        if (i.name == "bili_jct")
                            account.CsrfToken = i.value;
                    }
                    account.strCookies = account.strCookies.Substring(0, account.strCookies.Length - 2);
                    account.LoginStatus = Account.LoginStatusEnum.ByPassword;
                }
                else
                    throw new Exceptions.GetAccount_Exception(obj.code, obj.message);
            }
            else
                throw new Exceptions.GetAccount_Exception(-1000, "网络错误");
        }

        /// <summary>
        /// 发送验证码信息
        /// </summary>
        /// <exception cref="Exceptions.SMS_Send_Exception"/>
        public static void SMS_Send(string challenge, string key, string tmp_token, string validate)
        {
            string str = Http.PostBody("https://api.bilibili.com/x/safecenter/sms/send", $"csrf=&csrf_token=&type=18&captcha_type=7&captcha_key={key}&captcha=&challenge={challenge}&seccode={validate}%7Cjordan&validate={validate}&tmp_code={tmp_token}",null, $"BiliAccount/{Config.Dll_Version}");
            if (!string.IsNullOrEmpty(str))
            {
                int code = int.Parse(new Regex("(?<=\"code\":)(\\d+)(?=,)").Match(str).Value);
                if (code != 0)
                {
                    throw new Exceptions.SMS_Send_Exception(code, new Regex("(?<=\"message\":\")(.*?)(?=\")").Match(str).Value);
                }
            }
            else
                throw new Exceptions.SMS_Send_Exception(-1000, "网络错误");
        }

        /// <summary>
        /// 验证设备
        /// </summary>
        /// <param name="code">短信验证码</param>
        /// <param name="tmp_token"></param>
        /// <returns>code</returns>
        /// <exception cref="Exceptions.Verify_Exception"/>
        public static string Verify(string code, string tmp_token)
        {
            string param = $"appkey={Config.Instance.Appkey}&build={Config.Instance.Build}&channel=bili&code={code}&csrf=&csrf_token=&mobi_app=android&platform=android&statistics=%7B%22appId%22%3A1%2C%22platform%22%3A3%2C%22version%22%3A%22{Config.Instance.Version}%22%2C%22abtest%22%3A%22%22%7D&tmp_token={tmp_token}&ts={TimeStamp * 1000}";
            param += $"&sign={GetSign(param)}";
            string str = Http.PostBody("https://passport.bilibili.com/api/login/verify_device", param,null, $"BiliAccount/{Config.Dll_Version}");
            if (!string.IsNullOrEmpty(str))
            {
#if NETSTANDARD2_0 || NETCORE3_0
                Verify_DataTemplete obj = JsonConvert.DeserializeObject<Verify_DataTemplete>(str);
#else
                Verify_DataTemplete obj = (new JavaScriptSerializer()).Deserialize<Verify_DataTemplete>(str);
#endif
                if (obj.code == 0)
                {
                    return obj.data.code;
                }
                else
                    throw new Exceptions.Verify_Exception(obj.code, obj.message);
            }
            else
                throw new Exceptions.Verify_Exception(-1000, "网络错误");
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// 获取字符串md5
        /// </summary>
        /// <param name="str">文本</param>
        /// <returns>md5</returns>
        private static string GetMD5(string str)
        {
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(Encoding.UTF8.GetBytes(str));

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 获取参数签名
        /// </summary>
        /// <param name="strReq">请求参数</param>
        /// <returns>签名</returns>
        private static string GetSign(string strReq)
        {
            return GetMD5(strReq + Config.Instance.Appsecret);
        }

        #endregion Private Methods

        #region Private Classes

        /// <summary>
        /// 获取gt数据模板
        /// </summary>
        private class Get_gt_DataTemplete
        {
            #region Public Fields

            public int code;
            public Data data;

            #endregion Public Fields

            #region Public Classes

            public class Data
            {
                #region Public Fields

                public Result result;
                public int type;

                #endregion Public Fields

                #region Public Classes

                public class Result
                {
                    #region Public Fields

                    public string challenge;
                    public string gt;
                    public string key;
                    public int success;

                    #endregion Public Fields
                }

                #endregion Public Classes
            }

            #endregion Public Classes
        }

        /// <summary>
        /// 登录数据模板
        /// </summary>
        private class GetAccount_DataTemplete
        {
            #region Public Fields

            public int code;
            public Data_Templete data;
            public string message;
            public long ts;

            #endregion Public Fields

            #region Public Classes

            public class Data_Templete
            {
                #region Public Fields

                public Cookie_Info_Templete cookie_info;
                public Token_Info_Templete token_info;

                #endregion Public Fields

                #region Public Classes

                public class Cookie_Info_Templete
                {
                    #region Public Fields

                    public Cookie_Templete[] cookies;

                    #endregion Public Fields

                    #region Public Classes

                    public class Cookie_Templete
                    {
                        #region Public Fields

                        public long expires;
                        public string name;
                        public string value;

                        #endregion Public Fields
                    }

                    #endregion Public Classes
                }

                public class Token_Info_Templete
                {
                    #region Public Fields

                    public string access_token;
                    public long expires_in;
                    public string mid;
                    public string refresh_token;

                    #endregion Public Fields
                }

                #endregion Public Classes
            }

            #endregion Public Classes
        }

        /// <summary>
        /// 设备验证返回值数据模板
        /// </summary>
        private class Verify_DataTemplete
        {
            #region Public Fields

            public int code;
            public Data data;
            public string message;

            #endregion Public Fields

            #region Public Classes

            public class Data
            {
                #region Public Fields

                public string code;

                #endregion Public Fields
            }

            #endregion Public Classes
        }

        #endregion Private Classes
    }
}