using DDTV_Core;
using System;
using System.Collections.Generic;
using System.Net;

namespace DDTV_Core_Test
{
    internal class Program
    {
        private static void Main(string[] arg)
        {
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
            InitDDTV_Core.Core_Init();
            while (true)
            {
                Console.ReadKey();
            }
        }
    }
}
