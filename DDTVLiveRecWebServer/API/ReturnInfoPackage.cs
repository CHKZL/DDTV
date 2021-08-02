using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer.API
{
    public class ReturnInfoPackage
    {
        public static string InfoPkak<T>(int Code, List<T> Package, string Messge = null)
        {
            if (Messge == null)
            {
                switch (Code)
                {
                    case (int)MessgeCode.请求错误:
                        Messge = "请求错误";
                        break;
                    case (int)MessgeCode.鉴权失败:
                        Messge = "鉴权失败";
                        break;
                    case (int)MessgeCode.请求成功:
                        Messge = "请求成功";
                        break;
                    case (int)MessgeCode.请求成功但出现了错误:
                        Messge = "请求成功但出现了错误";
                        break;
                }
            }
            int PackageCount = 0;
            if (Package != null)
            {
                PackageCount = Package.Count;
            }
            string B = JsonConvert.SerializeObject(new Messge<T>()
            {
                messge = Messge,
                code = Code,
                Package = Package,
                queue = PackageCount
            });
            return B;
        }
        public class Messge<T>
        {
            public int code { set; get; }//状态码
            public string messge { set; get; }
            public int queue { set; get; }//Package的长度
            public List<T> Package { set; get; }

        }
        public enum MessgeCode
        {
            请求错误=-1,
            请求成功=1001,
            鉴权失败=1002,
            请求成功但出现了错误=1003,
        }
    }
}
