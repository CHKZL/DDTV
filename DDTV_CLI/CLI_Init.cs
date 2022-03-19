using DDTV_Core;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
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
            
            #region Flv修复demo
            //DDTV_Core.Tool.FlvModule.FileFix.CuttingFFMPEG(@"F:\Users\寒曦朦\OneDrive - hanximeng\OneList\录播/星宮汐Official/02_5/22-37-52_【B限】101_5931.flv");
            //List<string> args = new List<string>();
            //Console.WriteLine("输入需要修复的flv路径，或者直接拖拽文件到本窗口..");
            //while (true)
            //{

            //    string FileName = Console.ReadLine();
            //    if(FileName!="ok")
            //    {
            //        args.Add(FileName);
            //        Console.WriteLine("要继续添加修复的文件请继续拖拽或输入路径，确定开始修复请输入两个小写字母【ok】并且回车");
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}
            //foreach (var item in args)
            //{
            //    DDTV_Core.Tool.FlvModule.FileFix.Fix(item);
            //}
            //Console.WriteLine("修复完成，按任意键关闭窗口");
            //Console.ReadKey();
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //ServicePointManager.DefaultConnectionLimit = 512;
            //ServicePointManager.Expect100Continue = false;
            #endregion
            InitDDTV_Core.Core_Init(InitDDTV_Core.SatrtType.DDTV_CLI);
            while(true)
            {
                Thread.Sleep(60000);
            }
        }
    }
}