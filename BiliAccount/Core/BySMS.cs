using System;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

#if NETSTANDARD2_0 || NETCORE3_0
using Newtonsoft.Json;
#else

using System.Web.Script.Serialization;

#endif

#pragma warning disable CS0649

namespace BiliAccount.Core
{
    /// <summary>
    /// 通过手机验证码登录
    /// </summary>
    internal class BySMS
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
        /// 登录
        /// </summary>
        /// <param name="captcha_key">captcha_key</param>
        /// <param name="code">验证码</param>
        /// <param name="tel">手机号</param>
        /// <returns>账号实例</returns>
        public static Account Login(string captcha_key, string code, string tel)
        {
            Account account = new Account();
            string param = $"appkey={Config.Instance.Appkey}&bili_local_id={account.DeviceId}&build={Config.Instance.Build}&buvid={account.Buvid}&captcha_key={captcha_key}&channel=bili&cid=86&code={code}&device=phone&device_id={account.DeviceId}&device_name=BiliAccount{account.DeviceGuid}&device_platform=BiliAccount{Config.Dll_Version}&local_id={account.Buvid}&mobi_app=android&platform=android&statistics=%7B%22appId%22%3A1%2C%22platform%22%3A3%2C%22version%22%3A%22{Config.Instance.Version}%22%2C%22abtest%22%3A%22%22%7D&tel={tel}&ts={TimeStamp}";
            param += $"&sign={GetSign(param)}";

            string str = Http.PostBody("https://passport.bilibili.com/x/passport-login/login/sms", param,null, $"BiliAccount/{Config.Dll_Version}");
#if NETSTANDARD2_0 || NETCORE3_0
            Login_DataTemplete obj = JsonConvert.DeserializeObject<Login_DataTemplete>(str);
#else
            Login_DataTemplete obj = (new JavaScriptSerializer()).Deserialize<Login_DataTemplete>(str);
#endif
            if (obj.code == 0)
            {
                account.Uid = obj.data.token_info.mid;
                account.AccessToken = obj.data.token_info.access_token;
                account.RefreshToken = obj.data.token_info.refresh_token;
                account.Expires_AccessToken = DateTime.Now.AddSeconds(obj.data.token_info.expires_in);

                account.Cookies = new CookieCollection();
                foreach (Login_DataTemplete.Data_Templete.Cookie_Info_Templete.Cookie_Templete i in obj.data.cookie_info.cookies)
                {
                    account.strCookies += i.name + "=" + i.value + "; ";
                    account.Cookies.Add(new Cookie(i.name, i.value) { Domain = ".bilibili.com" });
                    account.Expires_Cookies = DateTime.Parse("1970-01-01 08:00:00").AddSeconds(i.expires);

                    if (i.name == "bili_jct")
                        account.CsrfToken = i.value;
                }
                account.strCookies = account.strCookies.Substring(0, account.strCookies.Length - 2);
                account.LoginStatus = Account.LoginStatusEnum.BySMS;
                return account;
            }
            else
                throw new SMS_Login_Exception(obj.code, obj.message);
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <returns>captcha_key</returns>
        /// <exception cref="Exceptions.SMS_Send_Exception"/>
        /// <exception cref="Exceptions.SMS_NeedGeetest_Exception"/>
        public static string SMS_Send(string tel)
        {
            string param = $"appkey={Config.Instance.Appkey}&build={Config.Instance.Build}&channel=bili&cid=86&mobi_app=android&platform=android&statistics=%7B%22appId%22%3A1%2C%22platform%22%3A3%2C%22version%22%3A%22{Config.Instance.Version}%22%2C%22abtest%22%3A%22%22%7D&tel={tel}&ts={TimeStamp}";
            param += $"&sign={GetSign(param)}";
            string str = Http.PostBody("https://passport.bilibili.com/x/passport-login/sms/send", param,null, $"BiliAccount/{Config.Dll_Version}");
#if NETSTANDARD2_0 || NETCORE3_0
            SMS_Send_DataTemplete obj = JsonConvert.DeserializeObject<SMS_Send_DataTemplete>(str);
#else
            SMS_Send_DataTemplete obj = (new JavaScriptSerializer()).Deserialize<SMS_Send_DataTemplete>(str);
#endif
            if (obj.code == 0)
            {
                if (string.IsNullOrEmpty(obj.data.recaptcha_url))
                    return obj.data.captcha_key;
                else
                    throw new Exceptions.SMS_NeedGeetest_Exception(obj.data.recaptcha_url);
            }
            else
                throw new Exceptions.SMS_Send_Exception(obj.code, obj.message);
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

        #region Public Classes

        /// <summary>
        /// 登录错误
        /// </summary>
        public class SMS_Login_Exception : Exception
        {
            #region Public Fields

            public int code;

            #endregion Public Fields

            #region Public Constructors

            public SMS_Login_Exception(int code, string message) : base(message)
            {
                this.code = code;
            }

            #endregion Public Constructors
        }

        #endregion Public Classes

        #region Private Classes

        /// <summary>
        /// 登录数据模板
        /// </summary>
        private class Login_DataTemplete
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
        /// 发送短信返回数据模板
        /// </summary>
        private class SMS_Send_DataTemplete
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

                public string captcha_key;
                public string recaptcha_url;

                #endregion Public Fields
            }

            #endregion Public Classes
        }

        #endregion Private Classes
    }
}