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
        public static DateTime 下次可登陆时间 = DateTime.Now;
        public static int 错误次数 = 0;
        public static string Web(HttpContext context)
        {
            string UserName = context.Request.Form["WebUserName"];
            string Password = context.Request.Form["WebPassword"];
            if(!string.IsNullOrEmpty(UserName) || !string.IsNullOrEmpty(Password))
            {
                if(DateTime.Now> 下次可登陆时间)
                {
                    下次可登陆时间 = DateTime.Now.AddSeconds(3);
                    if(UserName==Auxiliary.MMPU.WebUserName&&Password==Auxiliary.MMPU.WebPassword)
                    {
                        错误次数 = 0;
                        string WebToken = Guid.NewGuid().ToString();
                        Auxiliary.MMPU.WebToken = WebToken;
                        return JsonConvert.SerializeObject(new Auxiliary.RequestMessge.ServerClass.Login()
                        {
                            messge = "登陆成功",
                            result = true,
                            WebToken= WebToken
                        });
                    }
                    else
                    {
                        if(错误次数>=5)
                        {
                            下次可登陆时间 = DateTime.Now.AddMinutes(10);
                        }
                        else
                        {
                            错误次数++;
                        }
                        return JsonConvert.SerializeObject(new Auxiliary.RequestMessge.ServerClass.Login()
                        {
                            messge = "验证失败,还有" + (5 - 错误次数) + "次机会，全部错误将停止鉴权10分钟",
                            result = false,
                        });
                    }
                }
                else
                {
                    return JsonConvert.SerializeObject(new Auxiliary.RequestMessge.ServerClass.Login()
                    {
                        messge = "操作过于频繁",
                        result = false,
                    });
                }
            }
            else
            {
                return JsonConvert.SerializeObject(new Auxiliary.RequestMessge.ServerClass.Login()
                {
                    messge = "Null验证失败",
                    result = false,
                });
            }
        }

    }
}
