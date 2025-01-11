using Masuit.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.RuntimeObject
{
    public static class Login
    {
        public static async Task<string> get_login_urlAsync()
        {
            string URL = string.Empty;
            int waitTime = 0;
            while (waitTime <= 3000)
            {
                if (System.IO.File.Exists(Core.Config.Core_RunConfig._QrUrl))
                {
                    FileInfo fi = new FileInfo(Core.Config.Core_RunConfig._QrUrl);
                    using (FileStream fs = fi.OpenRead())
                    {
                        URL = fs.ReadAllText(Encoding.UTF8);  
                    }
                }
                else
                {
                    await Task.Delay(1000);
                    waitTime += 1000;
                }
            }
            return URL;
        }
    }
}
