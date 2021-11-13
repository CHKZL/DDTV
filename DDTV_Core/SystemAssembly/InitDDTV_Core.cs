using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly
{
    public class InitDDTV_Core
    {
        public static string Ver = "3.0.1.1-dev";
        public static void Init()
        {
            Log.Log.LogInit();
            ConfigModule.ConfigFile.ReadConfigFile();   
            BilibiliModule.User.BilibiliUser.Init();
            
        }
        public enum SatrtType
        {
            DDTV_Core=0,
            DDTV_GUI=1,
            DDTV_CLI=2,
            DDTV_Other=int.MaxValue
        }
    }
}
