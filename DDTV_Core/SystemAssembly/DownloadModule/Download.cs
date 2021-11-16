using DDTV_Core.SystemAssembly.NetworkRequestModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

// Disable the warning.
#pragma warning disable SYSLIB0014

namespace DDTV_Core.SystemAssembly.DownloadModule
{
    internal class Download
    {
        /// <summary>
        /// 增加下载任务(施工中)
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="FilePath"></param>
        public static void AddDownLoad(string Url,string FilePath)
        {
            Log.Log.AddLog(nameof(Download), Log.LogClass.LogType.Info, $"下载调度器收到下载任务[{Url}],当前为开发调试模式，暂不下载");
            return;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
            req.Method = "GET";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Accept="*/*";
            req.UserAgent = NetClass.UA();
            req.Referer="https://www.bilibili.com/";
            if (Url.Contains("bili"))
            {
                if (!string.IsNullOrEmpty(BilibiliModule.User.BilibiliUser.account.cookie))
                {
                    req.CookieContainer = NetClass.CookieContainerTransformation(BilibiliModule.User.BilibiliUser.account.cookie);
                }
            }
            DownFLV_HttpWebRequest(req, FilePath);
        }
        /// <summary>
        /// 以Post方式发送带参数和CookieContainer对象的http请求(该方法应该是为发送弹幕而特殊准备的)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static void DownFLV_HttpWebRequest(HttpWebRequest req, string FilePath)
        {
            Task.Run(() => {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream stream = resp.GetResponseStream();
                FileStream fileStream = new FileStream(FilePath, FileMode.Create);
                while (true)
                {
                    int EndF = stream.ReadByte();
                    if (EndF!=-1)
                    {
                        byte T = (byte)EndF;
                        fileStream.Write(new byte[] { T }, 0, 1);
                    }
                    else
                    {
                        //下载完成后的事件
                        break;
                    }
                }
            });
        }
    }
}
