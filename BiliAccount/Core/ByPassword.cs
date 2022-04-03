using System;
using System.Drawing;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

#if NETSTANDARD2_0 || NETCORE3_0
using Newtonsoft.Json;
#else

using System.Web.Script.Serialization;

#endif

#pragma warning disable CS0649

namespace BiliAccount.Core
{
    /// <summary>
    /// 通过密码登录
    /// </summary>
    internal class ByPassword
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
        /// <param name="account">账号实例</param>
        public static void DoLogin(ref Account account)
        {
            /* 旧版参数
             * string parm = "appkey=" + Config.Instance.Appkey + "&build=" + Config.Instance.Build + "&mobi_app=android&password=" + account.EncryptedPassword + "&platform=android&ts=" + TimeStamp + "&username=" + account.UserName;
             */
            string parm = $"appkey={Config.Instance.Appkey}&bili_local_id={account.DeviceId}&build={Config.Instance.Build}&buvid={account.Buvid}&channel=bili&device=phone&device_id={account.DeviceId}&device_name=BiliAccount{account.DeviceGuid}&device_platform=BiliAccount{Assembly.GetExecutingAssembly().GetName().Version}&from_pv=main.my-information.my-login.0.click&from_url=bilibili%3A%2F%2Fuser_center%2Fmine&local_id={account.Buvid}&mobi_app=android&password={account.EncryptedPassword}&platform=android&statistics=%7B%22appId%22%3A1%2C%22platform%22%3A3%2C%22version%22%3A%22{Config.Instance.Version}%22%2C%22abtest%22%3A%22%22%7D&ts={TimeStamp}&username={account.UserName}";
            parm += "&sign=" + GetSign(parm);
            //string str = Http.PostBodyOutCookies("http://passport.bilibili.com/api/v3/oauth2/login", out account.Cookies, parm, null, "application/x-www-form-urlencoded;charset=utf-8", "", Config.Instance.User_Agent);
            string str = Http.PostBodyOutCookies("https://passport.bilibili.com/x/passport-login/oauth2/login ", out account.Cookies, parm, null, "application/x-www-form-urlencoded;charset=utf-8", "", $"BiliAccount/{Config.Dll_Version}");
            if (!string.IsNullOrEmpty(str))
            {
#if NETSTANDARD2_0 || NETCORE3_0
                DoLogin_DataTemplete obj = JsonConvert.DeserializeObject<DoLogin_DataTemplete>(str);
#else
                DoLogin_DataTemplete obj = (new JavaScriptSerializer()).Deserialize<DoLogin_DataTemplete>(str);
#endif

                switch (obj.code)
                {
                    case 0://初步登录成功
                        LoginSuccess(obj, ref account);
                        break;

                    case -105://需要验证码
                        account.Url = obj.data.url;
                        account.LoginStatus = Account.LoginStatusEnum.NeedCaptcha;
                        break;

                    case -629://密码错误
                        account.LoginStatus = Account.LoginStatusEnum.WrongPassword;
                        break;

                    default:
                        account.LoginStatus = Account.LoginStatusEnum.None;
                        break;
                }
            }
        }

        /// <summary>
        /// 登录（带验证码。在当前版本api中已鲜见图片验证码，该方法已弃用。）
        /// </summary>
        /// <param name="captcha">验证码字符</param>
        /// <param name="account">账号实例</param>
        [Obsolete("在当前版本api中已鲜见图片验证码，该方法已弃用。")]
        public static void DoLoginWithCatpcha(string captcha, ref Account account)
        {
            string parm = "actionKey=" + Config.Instance.Appkey + "&appkey=" + Config.Instance.Appkey + "&build=" + Config.Instance.Build + "&captcha=" + captcha + "&mobi_app=android&password=" + account.EncryptedPassword + "&device=android&platform=android&ts=" + TimeStamp + "&username=" + account.UserName;
            parm += "&sign=" + GetSign(parm);
            //string str = Http.PostBodyOutCookies("http://passport.bilibili.com/api/v3/oauth2/login", out account.Cookies, parm, account.Cookies, "application/x-www-form-urlencoded;charset=utf-8", "", Config.Instance.User_Agent);
            string str = Http.PostBodyOutCookies("https://passport.bilibili.com/x/passport-login/oauth2/login", out account.Cookies, parm, account.Cookies, "application/x-www-form-urlencoded;charset=utf-8", "", $"BiliAccount/{Config.Dll_Version}");
            if (!string.IsNullOrEmpty(str))
            {
#if NETSTANDARD2_0 || NETCORE3_0
                DoLogin_DataTemplete obj = JsonConvert.DeserializeObject<DoLogin_DataTemplete>(str);
#else
                DoLogin_DataTemplete obj = (new JavaScriptSerializer()).Deserialize<DoLogin_DataTemplete>(str);
#endif
                switch (obj.code)
                {
                    case 0://登录成功
                        LoginSuccess(obj, ref account);
                        break;

                    case -105://验证码错误
                        account.Url = obj.data.url;
                        account.LoginStatus = Account.LoginStatusEnum.NeedCaptcha;
                        break;

                    case -629://密码错误
                        account.LoginStatus = Account.LoginStatusEnum.WrongPassword;
                        break;

                    default:
                        account.LoginStatus = Account.LoginStatusEnum.None;
                        break;
                }
            }
        }

        /// <summary>
        /// 密码加密
        /// </summary>
        /// <param name="password">未加密密码</param>
        /// <param name="key">key</param>
        /// <param name="hash">hash</param>
        /// <returns>加密后密码</returns>
        public static string EncryptPwd(string password, string key, string hash)
        {
            string purposetext = null;
            char purposecode = '\0';
            byte[] pem = RSA.PemUnpack(key, ref purposetext, ref purposecode);
            RSACryptoServiceProvider rsa = RSA.PemDecodePublicKey(pem);
            return UrlEncode(Convert.ToBase64String(RSA.Encrypt(rsa, Encoding.UTF8.GetBytes(hash + password))));
            //return UrlEncode(ExecuteScript("getpwd(\"" + key.Replace("\n", "").Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "") + "\",\"" + hash + "\",\"" + password + "\");", Properties.Resources.js_pwd));
        }

        /// <summary>
        /// 获取验证码图片（在当前版本api中已鲜见图片验证码，该方法已弃用。）
        /// </summary>
        /// <param name="account">账号实例</param>
        [Obsolete("在当前版本api中已鲜见图片验证码，该方法已弃用。")]
        public static Bitmap GetCaptcha(ref Account account)
        {
            return Http.GetPicOutCookies("https://passport.bilibili.com/captcha", out account.Cookies, account.Cookies);
        }

        /// <summary>
        /// 获取key
        /// </summary>
        /// <param name="hash">输出hash</param>
        /// <param name="key">输出key</param>
        /// <param name="cookies">输出cookies</param>
        public static void GetKey(out string hash, out string key, out CookieCollection cookies)
        {
            string parm = "appkey=" + Config.Instance.Appkey;
            parm += "&sign=" + GetSign(parm);
            string str = Http.PostBodyOutCookies("http://passport.bilibili.com/api/oauth2/getKey", out cookies, parm);
            if (!string.IsNullOrEmpty(str))
            {
#if NETSTANDARD2_0 || NETCORE3_0
                GetKey_DataTemplete obj = JsonConvert.DeserializeObject<GetKey_DataTemplete>(str);
#else
                GetKey_DataTemplete obj = (new JavaScriptSerializer()).Deserialize<GetKey_DataTemplete>(str);
#endif
                if (obj.code == 0)
                {
                    hash = obj.data.hash;
                    key = obj.data.key;
                    return;
                }
            }

            //获取失败
            hash = "";
            key = "";
        }

        /// <summary>
        /// 检查token可用性
        /// </summary>
        /// <param name="access_token">token</param>
        /// <returns>是否可用</returns>
        public static bool IsTokenAvailable(string access_token)
        {
            string parm = "access_token=" + access_token + "&appkey=" + Config.Instance.Appkey + "&ts=" + TimeStamp;
            parm += "&sign=" + GetSign(parm);
            string str = Http.GetBody("https://passport.bilibili.com/api/oauth2/info?" + parm);

            if (!string.IsNullOrEmpty(str))
            {
#if NETSTANDARD2_0 || NETCORE3_0
                IsTokenAvailable_DataTemplete obj = JsonConvert.DeserializeObject<IsTokenAvailable_DataTemplete>(str);
#else
                IsTokenAvailable_DataTemplete obj = (new JavaScriptSerializer()).Deserialize<IsTokenAvailable_DataTemplete>(str);
#endif

                if (obj.code == 0 && obj.data.expires_in > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// token续期
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="refresh_token"></param>
        /// <returns>到期时间</returns>
        public static DateTime? RefreshToken(string access_token, string refresh_token)
        {
            string parm = "access_token=" + access_token + "&appkey=" + Config.Instance.Appkey + "&refresh_token=" + refresh_token + "&ts=" + TimeStamp;
            parm += "&sign=" + GetSign(parm);
            string str = Http.PostBody("https://passport.bilibili.com/api/oauth2/refreshToken", parm);

            if (!string.IsNullOrEmpty(str))
            {
#if NETSTANDARD2_0 || NETCORE3_0
                RefreshToken_DataTemplete obj = JsonConvert.DeserializeObject<RefreshToken_DataTemplete>(str);
#else
                RefreshToken_DataTemplete obj = (new JavaScriptSerializer()).Deserialize<RefreshToken_DataTemplete>(str);
#endif
                if (obj.code == 0)
                {
                    return DateTime.Parse("1970-01-01 08:00:00").AddSeconds(obj.ts + obj.data.expires_in);
                }
            }
            return null;
        }

        /// <summary>
        /// 注销登录
        /// </summary>
        /// <param name="account">账号实例</param>
        /// <exception cref="Exceptions.Revoke_Exception"/>
        public static void Revoke(ref Account account)
        {
            if (account == null || account.LoginStatus <= 0) throw new Exceptions.Revoke_Exception(0, "账号未登陆");

            string param = $"access_key={account.AccessToken}&appkey={Config.Instance.Appkey}&bili_local_id={account.DeviceId}&build={Config.Instance.Build}&buvid={account.Buvid}&channel=bili&device=phone&device_id={account.DeviceId}&device_name=BiliAccount{account.DeviceGuid}&device_platform=BiliAccount{Assembly.GetExecutingAssembly().GetName().Version}&local_id={account.Buvid}&mid={account.Uid}&mobi_app=android&platform=android&statistics=%7B%22appId%22%3A1%2C%22platform%22%3A3%2C%22version%22%3A%22{Config.Instance.Version}%22%2C%22abtest%22%3A%22%22%7D&ts={TimeStamp}";
            param += $"&sign={GetSign(param)}";

            string str = Http.PostBody("https://passport.bilibili.com/x/passport-login/revoke", param);
            int code = int.Parse(new Regex("(?<=\"code\":)(\\d+)(?=,)").Match(str).Value);
            if (code != 0)
                throw new Exceptions.Revoke_Exception(code, new Regex("(?<=\"message\":\")(.*?)(?=\")").Match(str).Value);
            else
                account = new Account();
        }

        /// <summary>
        /// SSO
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns>[0]=>(string)strCookies,[1]=>(string)csrf_token,[2]=>(DateTime)Expiress,[3]=>(CookieCollection)Cookies</returns>
        public static object[] SSO(string access_token)
        {
            string parm = $"access_key={access_token}&appkey={Config.Instance.Appkey}&build={Config.Instance.Build}&gourl={UrlEncode("https://www.bilibili.com/")}&mobi_app=android&platform=android&ts={TimeStamp}";
            parm += $"&sign={GetSign(parm)}";

            HttpWebRequest req = null;
            HttpWebResponse rep = null;
            string cookies = "", csrf_token = "";
            CookieCollection cookiesC = new CookieCollection();
            DateTime expires = new DateTime();
            try
            {
                req = (HttpWebRequest)WebRequest.Create("https://passport.bilibili.com/api/login/sso?" + parm);
                req.AllowAutoRedirect = false;
                req.UserAgent = Config.Instance.User_Agent;
                rep = (HttpWebResponse)req.GetResponse();

                foreach (string i in rep.Headers.GetValues("Set-Cookie"))
                {
                    string[] tmp = i.Split(';');
                    string[] tmp2 = tmp[0].Split('=');

                    cookies += tmp[0] + "; ";
                    cookiesC.Add(new Cookie(tmp2[0], tmp2[1]) { Domain = ".bilibili.com" });
                    expires = DateTime.Parse(tmp[2].Split('=')[1]);

                    if (tmp2[0] == "bili_jct")
                        csrf_token = tmp2[1];
                }
                cookies = cookies.Substring(0, cookies.Length - 2);
            }catch(WebException wex)
            {
                if(wex.Status == WebExceptionStatus.ProtocolError && ((HttpWebResponse)wex.Response).StatusCode == HttpStatusCode.Found)
                {
                    foreach (string i in ((HttpWebResponse)wex.Response).Headers.GetValues("Set-Cookie"))
                    {
                        string[] tmp = i.Split(';');
                        string[] tmp2 = tmp[0].Split('=');

                        cookies += tmp[0] + "; ";
                        cookiesC.Add(new Cookie(tmp2[0], tmp2[1]) { Domain = ".bilibili.com" });
                        expires = DateTime.Parse(tmp[2].Split('=')[1]);

                        if (tmp2[0] == "bili_jct")
                            csrf_token = tmp2[1];
                    }
                    cookies = cookies.Substring(0, cookies.Length - 2);
                }
            }
            finally
            {
                if (rep != null) rep.Close();
                if (req != null) req.Abort();
            }
            return new object[] { cookies, csrf_token, expires, cookiesC };
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

        /// <summary>
        /// 初步登录成功判断
        /// </summary>
        /// <param name="obj">登录返回数据体</param>
        /// <param name="account">Account实例</param>
        private static void LoginSuccess(DoLogin_DataTemplete obj, ref Account account)
        {
            switch (obj.data.status)
            {
                case 0://登陆成功
                    account.Uid = obj.data.token_info.mid;
                    account.AccessToken = obj.data.token_info.access_token;
                    account.RefreshToken = obj.data.token_info.refresh_token;
                    account.Expires_AccessToken = DateTime.Now.AddSeconds(obj.data.token_info.expires_in);

                    account.Cookies = new CookieCollection();
                    foreach (DoLogin_DataTemplete.Data_Templete.Cookie_Info_Templete.Cookie_Templete i in obj.data.cookie_info.cookies)
                    {
                        account.strCookies += i.name + "=" + i.value + "; ";
                        account.Cookies.Add(new Cookie(i.name, i.value) { Domain = ".bilibili.com" });
                        account.Expires_Cookies = DateTime.Parse("1970-01-01 08:00:00").AddSeconds(i.expires);

                        if (i.name == "bili_jct")
                            account.CsrfToken = i.value;
                    }
                    account.strCookies = account.strCookies.Substring(0, account.strCookies.Length - 2);
                    account.LoginStatus = Account.LoginStatusEnum.ByPassword;
                    break;

                case 1://手机验证
                    Regex reg = new Regex("&tel=.*?&");
                    Match match = reg.Match(obj.data.url);
                    account.Tel = match.Value.Substring(5, match.Value.Length - 6);
                    account.Url = obj.data.url;
                    account.LoginStatus = Account.LoginStatusEnum.NeedTelVerify;
                    break;

                case 2://设备登录验证
                case 3:
                    account.Url = obj.data.url;
                    account.LoginStatus = Account.LoginStatusEnum.NeedSafeVerify;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// urlencode
        /// </summary>
        /// <param name="str">urlencode的字符串</param>
        /// <returns>urlencode后的字符串</returns>
        private static string UrlEncode(string str)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in str)
            {
                string tmp = HttpUtility.UrlEncode(c.ToString());
                if (tmp.Length > 1)
                {
                    builder.Append(tmp.ToUpper());
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        #endregion Private Methods

        #region Private Classes

        /// <summary>
        /// 登录数据模板
        /// </summary>
        private class DoLogin_DataTemplete
        {
            #region Public Fields

            public int code;
            public Data_Templete data;
            public long ts;

            #endregion Public Fields

            #region Public Classes

            public class Data_Templete
            {
                #region Public Fields

                public Cookie_Info_Templete cookie_info;
                public int status;
                public Token_Info_Templete token_info;
                public string url;

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
        /// GetKey返回值的数据模板
        /// </summary>
        private class GetKey_DataTemplete
        {
            #region Public Fields

            public int code;
            public Data_Templete data;

            #endregion Public Fields

            #region Public Classes

            public class Data_Templete
            {
                #region Public Fields

                public string hash;
                public string key;

                #endregion Public Fields
            }

            #endregion Public Classes
        }

        /// <summary>
        /// 检查token可用性数据模板
        /// </summary>
        private class IsTokenAvailable_DataTemplete
        {
            #region Public Fields

            public int code;
            public Data_Templete data;
            public long ts;

            #endregion Public Fields

            #region Public Classes

            public class Data_Templete
            {
                #region Public Fields

                public long expires_in;

                #endregion Public Fields
            }

            #endregion Public Classes
        }

        /// <summary>
        /// token续期数据模板
        /// </summary>
        private class RefreshToken_DataTemplete
        {
            #region Public Fields

            public int code;
            public Data_Templete data;
            public long ts;

            #endregion Public Fields

            #region Public Classes

            public class Data_Templete
            {
                #region Public Fields

                public long expires_in;

                #endregion Public Fields
            }

            #endregion Public Classes
        }

        #endregion Private Classes
    }
}