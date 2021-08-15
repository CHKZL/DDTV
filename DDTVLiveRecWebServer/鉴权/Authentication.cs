using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DDTVLiveRecWebServer.鉴权
{
    public class Authentication
    {
        public static List<string> 不进行时间校验的接口 = new List<string>() 
        {
            "file_steam"
        };
        public static 鉴权返回结果 API接口鉴权(HttpContext context, string cmd, bool 是否缺少关键参数 = false)
        {
       
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (context.Request.Method == "POST")
            {
                try
                {
                    foreach (var item in context.Request.Form)
                    {
                        dic.Add(item.Key, item.Value);
                        if (string.IsNullOrEmpty(item.Value))
                        {
                            if (Auxiliary.MMPU.调试模式)
                                Console.WriteLine($"出现错误{item.Key}为空");
                            return new 鉴权返回结果()
                            {
                                鉴权结果 = false,
                                鉴权返回消息 = "参数不能为空"
                            };
                        }
                    }
                }
                catch (Exception)
                {
                    if (Auxiliary.MMPU.调试模式)
                        Console.WriteLine($"请求中出现了系统无法识别的字符");
                    return new 鉴权返回结果()
                    {
                        鉴权结果 = false,
                        鉴权返回消息 = "请求中出现了系统无法识别的字符"
                    };
                }
            }
            else if (context.Request.Method == "GET")
            {
                try
                {
                    foreach (var item in context.Request.Query)
                    {
                        dic.Add(item.Key, item.Value);
                        if (string.IsNullOrEmpty(item.Value))
                        {
                            if (Auxiliary.MMPU.调试模式)
                                Console.WriteLine($"出现错误{item.Key}为空");
                            return new 鉴权返回结果()
                            {
                                鉴权结果 = false,
                                鉴权返回消息 = "参数不能为空"
                            };
                        }
                    }
                }
                catch (Exception)
                {
                    if (Auxiliary.MMPU.调试模式)
                        Console.WriteLine($"请求中出现了系统无法识别的字符");
                    return new 鉴权返回结果()
                    {
                        鉴权结果 = false,
                        鉴权返回消息 = "缺少必要参数"
                    };
                }
            }
            if (!dic.ContainsKey("sig") || !dic.ContainsKey("time") || !dic.ContainsKey("cmd") || !dic.ContainsKey("ver") || 是否缺少关键参数)
            {
                if (Auxiliary.MMPU.调试模式)
                    Console.WriteLine($"缺少必要的信息！");
                return new 鉴权返回结果()
                {
                    鉴权结果 = false,
                    鉴权返回消息 = "缺少必要参数"
                };
            }
            if (dic.ContainsKey("token"))
            {
                if (Auxiliary.MMPU.调试模式)
                    Console.WriteLine($"不应该提交Token！");
                return new 鉴权返回结果()
                {
                    鉴权结果 = false,
                    鉴权返回消息 = "存在不应该存在的的参数"
                };
            }
            if (dic["cmd"] != cmd)
            {
                if (Auxiliary.MMPU.调试模式)
                    Console.WriteLine($"提交的cmd内容和api请求不一致！");
                return new 鉴权返回结果()
                {
                    鉴权结果 = false,
                    鉴权返回消息 = "参数不正确"
                };
            }
            if(!不进行时间校验的接口.Contains(cmd))
            {
                int Time = 0;
                int NewTime = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                if (string.IsNullOrEmpty(dic["time"]) || !int.TryParse(dic["time"], out Time))
                {
                    if (Auxiliary.MMPU.调试模式)
                        Console.WriteLine($"时间格式错误！服务器时间{NewTime}，提交的时间{Time}");
                    return new 鉴权返回结果()
                    {
                        鉴权结果 = false,
                        鉴权返回消息 = "时间不能为空或值不正确"
                    };
                }
                int 时间差 = NewTime - Time;
                if (时间差 > 300 || 时间差 < -300)
                {
                    if (Auxiliary.MMPU.调试模式)
                        Console.WriteLine($"时间差值过大！服务器时间{NewTime}，提交的时间{Time}");
                    return new 鉴权返回结果()
                    {
                        鉴权结果 = false,
                        鉴权返回消息 = "时间差值过大！"
                    };
                }
            }
           
            switch (dic["ver"])
            {
                //WebToken
                case "1":
                    {
                        dic.Add("token", Auxiliary.MMPU.WebToken);
                        break;
                    }
                //ApiToken
                case "2":
                    {
                        dic.Add("token", Auxiliary.MMPU.ApiToken);
                        break;
                    }
            }

            var dicSort = from objDic in dic orderby objDic.Key ascending select objDic;


            List<List<string>> LS = new List<List<string>>();
            foreach (KeyValuePair<string, string> kvp in dicSort)
            {
                if (kvp.Key != "sig")
                    LS.Add(new List<string>() { kvp.Key, kvp.Value });
            }
            string 加密字符串 = LS[0][0] + "=" + LS[0][1];
            for (int i = 1; i < LS.Count; i++)
            {
                加密字符串 += "&" + LS[i][0] + "=" + LS[i][1];
            }
            string 服务器sig = Auxiliary.Encryption.SHA1_Encrypt(加密字符串);
            if (服务器sig == dic["sig"])
            {
                return new 鉴权返回结果()
                {
                    鉴权结果 = true,
                    鉴权返回消息 = "成功"
                };
            }
            else
            {
                if (Auxiliary.MMPU.调试模式)
                    Console.WriteLine($"sig校验失败，服务器计算的结果为:{服务器sig}，收到的数据为{dic["sig"]}");
                return new 鉴权返回结果()
                {
                    鉴权结果 = false,
                    鉴权返回消息 = "sig校验失败"
                };
            }
        }
        public class 鉴权返回结果
        {
            public bool 鉴权结果 { set; get; }
            public string 鉴权返回消息 { set; get; }
        }
    }
}