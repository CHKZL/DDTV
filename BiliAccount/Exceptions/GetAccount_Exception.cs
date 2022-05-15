using System;

namespace BiliAccount.Exceptions
{
    /// <summary>
    /// 获取登录账号信息错误
    /// </summary>
    public class GetAccount_Exception : Exception
    {
        #region Public Fields

        public int code;

        #endregion Public Fields

        #region Public Constructors

        public GetAccount_Exception(int code, string message) : base(message)
        {
            this.code = code;
        }

        #endregion Public Constructors
    }
}