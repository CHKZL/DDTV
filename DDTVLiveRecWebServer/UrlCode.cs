using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer
{
    public class UrlCode
    {
        /// <summary>
        /// Url反编码操作
        /// </summary>
        /// <param name="context">HttpContext类</param>
        /// <param name="IsPost">是否是Post请求，真：Post；假：Get</param>
        /// <returns></returns>
        public static Dictionary<string,string> UrlDecode(Microsoft.AspNetCore.Http.HttpContext context, bool IsPost = true)
        {
            Dictionary<string, string> _ = new Dictionary<string, string>();
            if (IsPost)
            {
                foreach (var item in context.Request.Form)
                {
                    _.Add(item.Key.ToLower(), System.Web.HttpUtility.UrlDecode(item.Value, System.Text.Encoding.UTF8));
                }
            }
            else
            {
                foreach (var item in context.Request.Query)
                {
                    _.Add(item.Key.ToLower(), System.Web.HttpUtility.UrlDecode(item.Value, System.Text.Encoding.UTF8));
                }
            }
            return _;
        }
        ///// <summary>
        ///// Url编码操作
        ///// </summary>
        ///// <param name="context">HttpContext类</param>
        ///// <param name="IsPost">是否是Post请求，真：Post；假：Get</param>
        ///// <returns></returns>
        //public static List<string> UrlEncode(Microsoft.AspNetCore.Http.HttpContext context, bool IsPost = true)
        //{
        //    List<string> _ = new List<string>();
        //    if (IsPost)
        //    {
        //        foreach (var item in context.Request.Form)
        //        {
        //            _.Add(System.Web.HttpUtility.UrlEncode(item.Value, System.Text.Encoding.UTF8));
        //        }
        //    }
        //    else
        //    {
        //        foreach (var item in context.Request.Query)
        //        {
        //            _.Add(System.Web.HttpUtility.UrlEncode(item.Value, System.Text.Encoding.UTF8));
        //        }
        //    }
        //    return _;
        //}
    }
}
