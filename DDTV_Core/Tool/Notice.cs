using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.Tool
{
    public class Notice
    {
        public static string GetNotice(string Type)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>()
            {
                {"Type",Type },
                {"CAID",InitDDTV_Core.ClientAID }
            };
            try
            {
                return SystemAssembly.NetworkRequestModule.Post.Post.HttpPost("http://api.ddtv.pro/api/Notice_Text", Parameters);
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
