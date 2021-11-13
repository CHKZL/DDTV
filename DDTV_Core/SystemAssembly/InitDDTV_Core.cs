using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly
{
    public class InitDDTV_Core
    {
        public static void Init()
        {
            Log.Log.LogInit();
            ConfigModule.ConfigFile.ReadConfigFile();
        }
    }
}
