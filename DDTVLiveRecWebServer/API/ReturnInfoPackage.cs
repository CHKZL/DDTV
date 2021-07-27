using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer.API
{
    public class ReturnInfoPackage
    {
        public static string InfoPkak<T>(int Code, string Messge, List<T> Package)
        {
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
            鉴权失败=1002
        }
    }
}
