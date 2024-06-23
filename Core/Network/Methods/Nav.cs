using Core.Account;
using Core.LogModule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Core.Network.Methods.User;

namespace Core.Network.Methods
{
    public class Nav
    {
        #region internal Method
        internal static Nav_Class? GetNav()
        {
            return _NAV();
        }
        #endregion

        #region Private Method
        /// <summary>
        /// 获取目前cookie所属账号状态和基本信息
        /// </summary>
        /// <returns></returns>
        private static Nav_Class? _NAV()
        {
            const int maxAttempts = 3;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                string WebText = "";
                try
                {
                    WebText = Get.GetBody($"{Config.Core_RunConfig._MainDomainName}/x/web-interface/nav", true);
                    Nav_Class? Nav_Class = System.Text.Json.JsonSerializer.Deserialize<Nav_Class>(WebText);
                    RuntimeObject.Account.nav_info = Nav_Class.data;
                    return Nav_Class;
                }
                catch (Exception e)
                {
                    Log.Error(nameof(_NAV), $"获取Nva状态出现错误，重试。获取到的状态内容文本:{WebText}",e);
                    if (attempt == maxAttempts - 1)
                    {
                        return null;
                    }
                }
                Thread.Sleep(500);
            }
            return null;
        }

        #endregion

        #region Public Class

        public class Nav_Class
        {
            public long code { get; set; }
            public string message { get; set; }
            public long ttl { get; set; }
            public Data data { get; set; }

            public class Data
            {
              
                public long mid { get; set; }
              
                [JsonIgnore]
                public Wbi_Img wbi_img { get; set; }
            }
            public class Wbi_Img
            {
                public string img_url { get; set; }
                public string sub_url { get; set; }
            }
        }




        #endregion
    }
}
