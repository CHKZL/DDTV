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

            #region Flv修复demo
            //DDTV_Core.Tool.FlvModule.FileFix.CuttingFFMPEG(@"F:\Users\寒曦朦\OneDrive - hanximeng\OneList\录播/星宮汐Official/02_5/22-37-52_【B限】101_5931.flv");
            //Console.WriteLine("输入需要修复的flv路径，或者直接拖拽文件到本窗口..");
            //string FileName = Console.ReadLine();

            //DDTV_Core.Tool.FlvModule.FileFix.Fix(FileName);

            //Console.WriteLine("修复完成，按任意键关闭窗口");
            //Console.ReadKey();

            #endregion

            InitDDTV_Core.Core_Init(InitDDTV_Core.SatrtType.DDTV_CLI);
            while (true)
            {
                Thread.Sleep(60000);
            }
        }
    }
}