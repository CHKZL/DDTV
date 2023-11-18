using System;
using System.Reflection;

#pragma warning disable CS0649

namespace Core.Account
{
    internal class Parameter
    {
        /// <summary>
        /// 当前程序集版本号
        /// </summary>
        public static string Dll_Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        #region Public Properties

        /// <summary>
        /// Appkey
        /// </summary>
        public string Appkey { get; private set; } = "bca7e84c2d947ac6";

        /// <summary>
        /// AppSecret
        /// </summary>
        public string Appsecret { get; private set; } = "60698ba2f68e01ce44738920a0ffe768";

        /// <summary>
        /// Build
        /// </summary>
        public string Build { get; private set; } = "6200400";

        /// <summary>
        /// 指示是否已经初始化
        /// </summary>
        public bool IsInited { get; private set; } = false;

        /// <summary>
        /// UA
        /// </summary>
        public string User_Agent
        {
            get
            {
                return $"Mozilla/5.0 BiliDroid/{Version} (bbcallen@gmail.com) os/android model/MI 9 mobi_app/android build/{Build} channel/master innerVer/{Build} osVer/10 network/2";
            }
        }

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; private set; } = "6.20.0";

#endregion Public Properties

#region Private Classes

        /// <summary>
        /// 初始化数据模板
        /// </summary>
        private class Init_DataTemplete
        {
#region Public Fields

            public string appkey;
            public string appsecret;
            public string build;
            public string version;

#endregion Public Fields
        }

#endregion Private Classes
    }
}