using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Init
    {
        public static string Ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "-" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string InitType = "DDTV";
        public static string ClientAID = string.Empty;
        public static string CompiledVersion = "2023-11-03 21:01:27";
        public static bool IsDevDebug = false;
        public static void Start(string InitType = "DDTV", string ClientAID = "", bool IsDev = false)
        {
            LogModule.Log.LogInit();
        }
    }
}
