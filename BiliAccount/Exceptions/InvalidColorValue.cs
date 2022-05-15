using System;

namespace BiliAccount.Exceptions
{
    /// <summary>
    /// 传入了错误的颜色值
    /// </summary>
    public class InvalidColorValue : Exception
    {
        #region Public Constructors

        /// <summary>
        /// 以指定的属性名初始化<see cref="InvalidColorValue"/>
        /// </summary>
        /// <param name="name">属性名</param>
        public InvalidColorValue(string name) : base($"传入了错误的颜色值！{name}")
        {
            PropertyName = name;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// 错误的属性名
        /// </summary>
        public string PropertyName { get; private set; }

        #endregion Public Properties
    }
}