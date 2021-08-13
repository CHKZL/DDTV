using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer.API
{
    public class WebLogin
    {
        public static bool 是否初次登陆 = true;
        public static DateTime 下次可登陆时间 = DateTime.MinValue;
        public static int 错误次数 = 0;
        public static string Web(HttpContext context)
        {
            try
            {
                int B = context.Request.Form.Count();
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new Auxiliary.RequestMessage.ServerClass.Login()
                {
                    message = "请求的表单格式不对！",
                    result = false,
                });
            }
            string UserName = context.Request.Form["WebUserName"];
            string Password = context.Request.Form["WebPassword"];
            if(!string.IsNullOrEmpty(UserName) || !string.IsNullOrEmpty(Password))
            {
                if(是否初次登陆)
                {
                    下次可登陆时间 = DateTime.Now.AddSeconds(5);
                }
                if (DateTime.Now < 下次可登陆时间)
                {
                    下次可登陆时间 = DateTime.Now.AddSeconds(5);
                    if(UserName==Auxiliary.MMPU.WebUserName&&Password==Auxiliary.MMPU.WebPassword)
                    {
                        错误次数 = 0;
                        string WebToken = Guid.NewGuid().ToString();
                        Auxiliary.MMPU.WebToken = WebToken;
                        return JsonConvert.SerializeObject(new Auxiliary.RequestMessage.ServerClass.Login()
                        {
                            message = "登陆成功",
                            result = true,
                            WebToken= WebToken
                        });
                    }
                    else
                    {
                        if(错误次数>=5)
                        {
                            下次可登陆时间 = DateTime.Now.AddMinutes(30);
                        }
                        else
                        {
                            错误次数++;
                        }
                        return JsonConvert.SerializeObject(new Auxiliary.RequestMessage.ServerClass.Login()
                        {
                            message = "验证失败,还有" + (5 - 错误次数) + "次机会，全部错误将停止鉴权10分钟",
                            result = false,
                        });
                    }
                }
                else
                {
                    return JsonConvert.SerializeObject(new Auxiliary.RequestMessage.ServerClass.Login()
                    {
                        message = "操作过于频繁",
                        result = false,
                    });
                }
            }
            else
            {
                return JsonConvert.SerializeObject(new Auxiliary.RequestMessage.ServerClass.Login()
                {
                    message = "Null验证失败",
                    result = false,
                });
            }
        }

    }
}
