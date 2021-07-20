using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDTVLiveRecWebServer.API
{
    public class ReturnInfoPackage
    {
        public static string InfoPkak<T>(鉴权.Authentication.鉴权返回结果 _, List<T> Package)
        {
            int PackageCount = 0;
            if (Package!=null)
            {
                PackageCount = Package.Count;
            }
            string B = JsonConvert.SerializeObject(new Messge<T>()
            {
                messge = _.鉴权返回消息,
                result = _.鉴权结果,
                Package = Package,
                queue = PackageCount
            });
            return B;
        }
        public class Messge<T>
        {
            public bool result { set; get; }
            public string messge { set; get; }
            public int queue { set; get; }
            public List<T> Package { set; get; }

        }
    }
}
