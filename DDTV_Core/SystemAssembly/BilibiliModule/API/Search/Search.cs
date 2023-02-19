using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.Search
{
    public class Search
    {
        private static int CoolingTime = 5;
        private static DateTime TriggerTime = DateTime.MinValue;
        /// <summary>
        /// 搜索_主播
        /// </summary>
        /// <param name="KeyWord">搜索的关键字</param>
        /// <returns></returns>
        public static JObject Search_Live_User(string KeyWord)
        {
            if (TriggerTime.AddSeconds(CoolingTime) < DateTime.Now)
            {
                string WebR = NetworkRequestModule.Get.Get.GetRequest("https://api.bilibili.com/x/web-interface/search/type?" +
               $"search_type=live_user&" +
               $"keyword={KeyWord}");
                TriggerTime = DateTime.Now;
                JObject JO = (JObject)JsonConvert.DeserializeObject(WebR);
                return JO;
            }
            else
            {
                return (JObject)JsonConvert.DeserializeObject("{\"code\": -1,\"message\": \"请求频率过高，请稍候再试\"}");
            }
        }

    }
}
