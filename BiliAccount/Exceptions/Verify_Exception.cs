using System;

namespace BiliAccount.Exceptions
{
    /// <summary>
    /// 设备验证错误
    /// </summary>
    public class Verify_Exception : Exception
    {
        #region Public Fields

        public int code;

        #endregion Public Fields

        #region Public Constructors

        public Verify_Exception(int code, string message) : base(message)
        {
            this.code = code;
        }

        #endregion Public Constructors
    }
}