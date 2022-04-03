using System.Text.RegularExpressions;

namespace BiliAccount.Linq
{
    /// <summary>
    /// 设备验证类
    /// </summary>
    public class Device_Verify
    {
        #region Public Methods

        /// <summary>
        /// 获取账号信息
        /// </summary>
        /// <param name="code">验证后返回的code</param>
        /// <param name="account">账号实例</param>
        /// <exception cref="Exceptions.GetAccount_Exception"/>
        public static void GetAccount(string code, ref Account account)
        {
            Core.Device_Verify.GetAccount(code, ref account);
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="challenge">challenge</param>
        /// <param name="key">key</param>
        /// <param name="tmp_token">tmp_token</param>
        /// <param name="validate">validate</param>
        /// <exception cref="Exceptions.SMS_Send_Exception"/>
        public static void Send_SMS(string challenge, string key, string tmp_token, string validate)
        {
            Core.Device_Verify.SMS_Send(challenge, key, tmp_token, validate);
        }

        /// <summary>
        /// 从验证url获取tmp_token
        /// </summary>
        /// <param name="url">验证url</param>
        /// <returns>tmp_token</returns>
        public static string Url2TmpToken(string url)
        {
            Regex reg = new Regex("tmp_token=(.+?)(&|$)");
            return reg.Match(url).Groups[1].Value;
        }

        /// <summary>
        /// 单纯的设备验证
        /// </summary>
        /// <param name="code">手机验证码</param>
        /// <param name="tmp_token">tmp_token</param>
        /// <returns>验证后返回的code</returns>
        /// <exception cref="Exceptions.Verify_Exception"/>
        public static string Verify(string code, string tmp_token)
        {
            return Core.Device_Verify.Verify(code, tmp_token);
        }

        /// <summary>
        /// 返回账号信息的设备验证
        /// </summary>
        /// <param name="code">手机验证码</param>
        /// <param name="tmp_token">tmp_token</param>
        /// <param name="account">账号实例</param>
        /// <exception cref="Exceptions.Verify_Exception"/>
        public static void Verify(string code, string tmp_token, ref Account account)
        {
            Core.Device_Verify.GetAccount(Core.Device_Verify.Verify(code, tmp_token), ref account);
        }

        #endregion Public Methods
    }
}