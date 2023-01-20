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
        public static JObject TypeSearch(string KeyWord)
        {
            string WebR = NetworkRequestModule.Get.Get.GetRequest("https://api.bilibili.com/x/web-interface/search/type?"+
                $"search_type=live_user&" +
                $"keyword={KeyWord}");
           
            JObject JO = (JObject)JsonConvert.DeserializeObject(WebR);
            return JO;
        }
    }
}
