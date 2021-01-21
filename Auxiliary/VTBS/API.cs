using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Auxiliary.VTBS
{
    public class API
    {
        /// <summary>
        /// 用于针对不同用户进行自动进行CDN选择，加快数据库连接速度
        /// </summary>
        public class VTBS服务器CDN
        {
            public static string VTBS_Url = "https://api.vtbs.moe";
            public static void 根据CDN更新VTBS_Url()
            {
                new Task(()=> {
                    string CDN_Url = "";
                    try
                    {
                        CDN_Url = MMPU.返回网页内容("https://api.vtbs.moe/meta/cdn");
                    }
                    catch (Exception)
                    {
                    }
                    if(string.IsNullOrEmpty(CDN_Url))
                    {
                        CDN_Url = MMPU.返回网页内容("https://api.tokyo.vtbs.moe/meta/cdn");
                    }
                    JArray JO = string.IsNullOrEmpty(CDN_Url) ? (JArray)JsonConvert.DeserializeObject("[]") : (JArray)JsonConvert.DeserializeObject(CDN_Url);
                    List<延迟对象> PING = new List<延迟对象>();
                    foreach (var item in JO)
                    {
                        PING.Add(new 延迟对象() { CDN_URL = item.ToString() });
                    }
                    VTBS_Url = 返回延迟最低的连接(PING, 5);
                    InfoLog.InfoPrintf("获取到VTBS当前可用CDN为:" + VTBS_Url, InfoLog.InfoClass.Debug);
                }).Start();
            }
            /// <summary>
            /// 返回延迟最低的连接
            /// </summary>
            /// <param name="URL">连接对象</param>
            /// <param name="num">采集次数</param>
            /// <returns></returns>
            private static string 返回延迟最低的连接(List<延迟对象> URL,double num)
            {
                string 默认返回值 = "https://api.vtbs.moe";
                double 当前延迟 = 65535.0;
                foreach (var item in URL)
                {
                    for(int i=0;i<num+1.0;i++)
                    {
                        double T = MMPU.测试延迟(item.CDN_URL + "/v1/vtbs");
                        double 延迟 = T > 0 ? T : 10000.0;
                        if(i!=0)
                        {
                            item.延迟总计数 += 延迟;
                        }
                    }
                    item.平均延迟 = item.延迟总计数 / num;
                }
                foreach (var item in URL)
                {
                    if (item.平均延迟 < 当前延迟)
                    {
                        当前延迟 = item.平均延迟;
                        默认返回值 = item.CDN_URL;
                    }
                }
                return 默认返回值;
            }
            private class 延迟对象
            {
                public double 平均延迟 { set; get; }
                public double 最高延迟 { set; get; }
                public double 最低延迟 { set; get; }
                public double 延迟总计数 { set; get; }
                public string CDN_URL { set; get; }
            }
        }  
    }
}