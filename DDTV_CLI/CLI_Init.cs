using DDTV_Core;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using DDTV_Core.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DDTV_CLI
{
    public class CLI_Init
    {
        public static void Main(string[] args)
        {
            InitDDTV_Core.Core_Init(InitDDTV_Core.SatrtType.DDTV_CLI,args.Contains("--no-update"));
            while (true)
            {
                Thread.Sleep(60000);
            }
        }
        public static string GetPackageVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}