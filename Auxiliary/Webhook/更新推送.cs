using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auxiliary.Webhook
{
    public class 更新推送
    {
        public static void 更新提示(更新Info Info)
        {
            if (MMPU.WebhookEnable)
            {
               Webhook.post P = new Webhook.post()
                {
                    jsonParam = JsonConvert.SerializeObject(Info),
                    cmd = "UPDATE"
                };
                do
                {
                    if (P.尝试次数 > 3)
                    {
                        InfoLog.InfoPrintf($"WebHook——{P.cmd}信息发送失败，重试三次均超时，放弃该hook请求；(配置的webhook地址为:{MMPU.WebhookUrl})", InfoLog.InfoClass.下载必要提示);
                        break;
                    }
                    P.hookpost(Info);
                } while (!P.是否成功);
            }
        }
        public class 更新Info
        {
            public string Ver { set; get; }
            public string UpdateMsg { set; get; }
        }
    }
}
