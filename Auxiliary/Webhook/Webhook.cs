using Newtonsoft.Json;
using System;
using System.Threading;
using System.IO;
using System.Net;
using System.Text;

namespace Auxiliary.Webhook
{
    public class Webhook
    {
        public static void 开播hook(开播Info Info)
        {
            if (MMPU.WebhookEnable)
            {
                post P = new post()
                {
                    jsonParam = JsonConvert.SerializeObject(Info),
                    cmd="开播"
                };            
                do
                {
                    if (P.尝试次数 > 3)
                    {
                        InfoLog.InfoPrintf($"WebHook——{P.cmd}信息发送失败，重试三次均超时，放弃该hook请求；(配置的webhook地址为:{MMPU.WebhookUrl})", InfoLog.InfoClass.下载必要提示);
                        break;
                    }
                    P.hookpost();
                } while (!P.是否成功);
            }
        }
        public class 开播Info
        {
            public string GUID { set; get; }
            public string RoomId { set; get; }
            public string Name { set; get; }
            public string Title { set; get; }
            public DateTime StartTime { set; get; }
            public string TaskType { set; get; }
        }

        public static void 下播hook(下播Info Info)
        {
            if (MMPU.WebhookEnable)
            {
                post P = new post()
                {
                    jsonParam = JsonConvert.SerializeObject(Info),
                    cmd = "下播"
                };
                do
                {
                    if (P.尝试次数 > 3)
                    {
                        InfoLog.InfoPrintf($"WebHook——{P.cmd}信息发送失败，重试三次均超时，放弃该hook请求；(配置的webhook地址为:{MMPU.WebhookUrl})", InfoLog.InfoClass.下载必要提示);
                        break;
                    }
                    P.hookpost();
                } while (!P.是否成功);
            }
        }
        public class 下播Info
        {
            public string GUID { set; get; }
            public string RoomId { set; get; }
            public string Name { set; get; }
            public string Title { set; get; }
            public DateTime StartTime { set; get; }
            public DateTime EndTime { set; get; }
            public string TaskType { set; get; }
            public string SavePath { set; get; }
            public string Reason { set; get; }
        }
        private class post
        {
            public int 尝试次数 = 0;
            public bool 是否成功 { set; get; } = false;
            public string jsonParam { set; get; } = "";
            public string cmd { set; get; }
            public class mess
            {
                public string cmd { set; get; }
                public string messge { set; get; }
                
            }
            public void hookpost()
            {
                try
                {
                    HttpWebRequest request;
                    request = (HttpWebRequest)WebRequest.Create(MMPU.WebhookUrl);
                    request.Method = "POST";
                    request.ContentType = "application/json; charset=UTF-8";
                    request.UserAgent = $"DDTVCore/{MMPU.版本号}";
                    string paraUrlCoded = JsonConvert.SerializeObject(new mess() { 
                        cmd=cmd,
                        messge=jsonParam
                    });
                    byte[] payload;
                    payload = Encoding.UTF8.GetBytes(paraUrlCoded);
                    request.ContentLength = payload.Length;
                    Stream writer = request.GetRequestStream();
                    writer.Write(payload, 0, payload.Length);
                    writer.Close();
                    HttpWebResponse resp = (HttpWebResponse)request.GetResponse();

                    if ((int)resp.StatusCode >= 200 && (int)resp.StatusCode < 300)
                    {
                        是否成功 = true;
                        InfoLog.InfoPrintf($"WebHook——{P.cmd}信息发送完成", InfoLog.InfoClass.下载必要提示);
                    }
                    else
                    {
                        是否成功 = false;
                    }
                }
                catch (Exception)
                {
                    是否成功 = false;
                }
                 尝试次数++;
                Thread.Sleep(5000);
            }
        }
    }
    
}
