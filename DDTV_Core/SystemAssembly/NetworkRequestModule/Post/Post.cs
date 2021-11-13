using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.NetworkRequestModule.Post
{
    internal class Post
    {
        /// <summary>
        /// 通过Post请求发送网络请求获取Json类返回信息(该方法应该是为批量获取房间信息特殊准备的)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jsonParam"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string GetWebInfo_JsonClass(string url, string jsonParam, string encode)
        {
            string strURL = url;
            HttpWebRequest request;
            request = (HttpWebRequest)WebRequest.Create(strURL);
            request.Method = "POST";
            request.ContentType = "application/json;charset=" + encode.ToUpper();
            string paraUrlCoded = jsonParam;
            byte[] payload;
            payload = Encoding.GetEncoding(encode.ToUpper()).GetBytes(paraUrlCoded);
            request.ContentLength = payload.Length;
            //request.UserAgent = NetClass.UA();
            //if (url.Contains("bilibili"))
            //{
            //    if (!string.IsNullOrEmpty(BilibiliModule.User.BilibiliUser.cookie.cookie))
            //    {
            //        request.CookieContainer = NetClass.CookieContainerTransformation(BilibiliModule.User.BilibiliUser.cookie.cookie);
            //    }
            //}
            Stream writer = request.GetRequestStream();
            writer.Write(payload, 0, payload.Length);
            writer.Close();
            System.Net.HttpWebResponse response;
            response = (System.Net.HttpWebResponse)request.GetResponse();
            System.IO.Stream s;
            s = response.GetResponseStream();
            string StrDate = "";
            string strValue = "";
            StreamReader Reader = new StreamReader(s, Encoding.GetEncoding(encode.ToUpper()));
            while ((StrDate = Reader.ReadLine()) != null)
            {
                strValue += StrDate + "\r\n";
            }
            return strValue;
        }
    }
}
