using System;

#pragma warning disable CS1591

namespace BiliAccount.Exceptions
{
    /// <summary>
    /// 注销错误
    /// </summary>
    public class Revoke_Exception : Exception
    {
        #region Public Constructors

        public Revoke_Exception(int code, string message) : base(message)
        {
            this.code = code;
        }

        #endregion Public Constructors

        #region Public Properties

        public int code { get; private set; }

        #endregion Public Properties
    }
}