using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.Tool
{
    public class Dokidoki
    {
        public static void SendDokidoki(string Type,string Conut)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>()
            {
                {"Type",Type },
                {"Conut",Conut },
                {"CAID",InitDDTV_Core.ClientAID }
            };
            try
            {
                string Ver = SystemAssembly.NetworkRequestModule.Post.Post.HttpPost("http://api.ddtv.pro/api/Dokidoki", Parameters);
            }
            catch (Exception)
            {
                
            }
        }
    }
}
