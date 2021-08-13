using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Auxiliary.RequestMessage.MessageClass;

namespace Auxiliary.RequestMessage
{
    public class ReturnInfoPackage
    {
        public static string InfoPkak<T>(int Code, List<T> Package, string Message = null)
        {
            if (Message == null)
            {
                switch (Code)
                {
                    case (int)ServerSendMessageCode.请求错误:
                        Message = "请求错误";
                        break;
                    case (int)ServerSendMessageCode.鉴权失败:
                        Message = "鉴权失败";
                        break;
                    case (int)ServerSendMessageCode.请求成功:
                        Message = "请求成功";
                        break;
                    case (int)ServerSendMessageCode.请求成功但出现了错误:
                        Message = "请求成功但出现了错误";
                        break;
                }
            }
            int PackageCount = 0;
            if (Package != null)
            {
                PackageCount = Package.Count;
            }
            string B = JsonConvert.SerializeObject(new Message<T>()
            {
                message = Message,
                code = Code,
                Package = Package,
                queue = PackageCount
            });
            return B;
        }
    }
}
