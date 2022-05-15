using System;

namespace BiliAccount.Exceptions
{
    /// <summary>
    /// 发送短信需要极验验证时引发的错误
    /// </summary>
    public class SMS_NeedGeetest_Exception : Exception
    {
        #region Public Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="recaptcha_url">校验弹窗地址</param>
        public SMS_NeedGeetest_Exception(string recaptcha_url) : base("需要极验校验！")
        {
            url = recaptcha_url;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// 校验弹窗地址
        /// </summary>
        public string url { get; private set; }

        #endregion Public Properties
    }
}