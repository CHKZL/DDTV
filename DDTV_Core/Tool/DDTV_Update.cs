using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace DDTV_Core.Tool
{
    public class DDTV_Update
    {
        public static bool ComparisonVersion(string Type, string LocalVersion)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>()
            {
                {"Type",Type },
                {"LocalVersion",LocalVersion },
                {"CAID",InitDDTV_Core.ClientAID }
            };
            try
            {
                string Ver = SystemAssembly.NetworkRequestModule.Post.Post.HttpPost("http://api.ddtv.pro/api/Ver", Parameters);
                ServerMessageClass.MessageBase.pack<ServerMessageClass.MessageClass.VerClass> pack = JsonConvert.DeserializeObject<ServerMessageClass.MessageBase.pack<ServerMessageClass.MessageClass.VerClass>>(Ver);
                if(pack.data.Ver== LocalVersion)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}
