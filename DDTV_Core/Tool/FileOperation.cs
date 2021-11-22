using DDTV_Core.SystemAssembly.ConfigModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.Tool
{
    internal class FileOperation
    {
        /// <summary>
        /// 判断网络路径的文件是否存在
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        internal static bool IsExistsNetFile(string Url)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(Url));
                httpWebRequest.Accept = "*/*";
                httpWebRequest.UserAgent = SystemAssembly.NetworkRequestModule.NetClass.UA();
                httpWebRequest.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
                if (!string.IsNullOrEmpty(BilibiliUserConfig.account.cookie))
                {
                    httpWebRequest.Headers.Add("Cookie", BilibiliUserConfig.account.cookie);
                }
                httpWebRequest.Timeout = 5000;
                //返回响应状态是否是成功比较的布尔值
                var B = httpWebRequest.GetResponse;
                if (((HttpWebResponse)httpWebRequest.GetResponse()).StatusCode == HttpStatusCode.OK)
                {

                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
