namespace BiliAccount.Linq
{
    /// <summary>
    /// 短信登录
    /// </summary>
    public partial class BySMS
    {
        #region Private Fields

        private static string captcha_key;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="code">短信验证码</param>
        /// <param name="tel">电话号码</param>
        /// <returns>账号实例</returns>
        /// <exception cref="Core.BySMS.SMS_Login_Exception"/>
        public static Account Login(string code, string tel)
        {
            return Core.BySMS.Login(captcha_key, code, tel);
        }

        /// <summary>
        /// 发送验证短信
        /// </summary>
        /// <param name="tel">电话号码</param>
        /// <exception cref="Exceptions.SMS_Send_Exception"/>
        public static void SendSMS(string tel)
        {
            captcha_key = Core.BySMS.SMS_Send(tel);
        }

        #endregion Public Methods
    }
}