using System;
using System.Reflection;

#if NETSTANDARD2_0 || NETCORE3_0
using Newtonsoft.Json;
#else
using System.Web.Script.Serialization;
#endif

#pragma warning disable CS0649

namespace BiliAccount.Core
{
    internal class Config
    {
        #region Private Fields

        private static Config instance;

        /// <summary>
        /// 后端配置文件地址（如非必要请勿修改）
        /// </summary>
        private const string init_url = "http://ctrl.zhangbudademao.com/118/Init.json";

        #endregion Private Fields

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

        #region Private Constructors

        /// <summary>
        /// 初始化登录模块
        /// </summary>
        private Config()
        {
            string str = Http.GetBody($"{init_url}");

            if (!string.IsNullOrEmpty(str))
            {
#if NETSTANDARD2_0 || NETCORE3_0
                Init_DataTemplete obj = JsonConvert.DeserializeObject<Init_DataTemplete>(str);
#else
                Init_DataTemplete obj = (new JavaScriptSerializer()).Deserialize<Init_DataTemplete>(str);
#endif
                Appkey = obj.appkey;
                Appsecret = obj.appsecret;
                Build = obj.build;
                Version = obj.version;
                IsInited = true;
            }
        }

        /// <summary>
        /// 初始化登录模块(DEBUG Mode)
        /// </summary>
        private Config(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
#if NETSTANDARD2_0 || NETCORE3_0
                Init_DataTemplete obj = JsonConvert.DeserializeObject<Init_DataTemplete>(str);
#else
                Init_DataTemplete obj = (new JavaScriptSerializer()).Deserialize<Init_DataTemplete>(str);
#endif
                Appkey = obj.appkey;
                Appsecret = obj.appsecret;
                Build = obj.build;
                Version = obj.version;
                IsInited = true;
            }
        }

        #endregion Private Constructors

        #region Public Properties

        /// <summary>
        /// 单例接口
        /// </summary>
        public static Config Instance
        {
            get
            {
                if (instance == null) instance = new Config();
                return instance;
            }
        }

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
        public string Build { get; private set; } = "5572000";

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
        public string Version { get; private set; } = "5.57.2";

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