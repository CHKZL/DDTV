using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Auxiliary.RequestMessge.MessgeClass;

namespace Auxiliary.RequestMessge
{
    public class ReturnInfoPackage
    {
        public static string InfoPkak<T>(int Code, List<T> Package, string Messge = null)
        {
            if (Messge == null)
            {
                switch (Code)
                {
                    case (int)ServerSendMessgeCode.请求错误:
                        Messge = "请求错误";
                        break;
                    case (int)ServerSendMessgeCode.鉴权失败:
                        Messge = "鉴权失败";
                        break;
                    case (int)ServerSendMessgeCode.请求成功:
                        Messge = "请求成功";
                        break;
                    case (int)ServerSendMessgeCode.请求成功但出现了错误:
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
    }
}
