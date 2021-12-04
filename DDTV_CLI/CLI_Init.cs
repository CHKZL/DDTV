using DDTV_Core;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DDTV_CLI
{
    public class CLI_Init
    {
        public static void Main(string[] args)
        {

            #region Flv修复demo
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
            while (true)
            {
                if (Console.ReadKey().Key.Equals(ConsoleKey.I))
                {
                    Console.WriteLine($"请按对应的按键查看或修改配置：\n" +
                         $"a：查看下载中的任务情况\n" +
                         $"b：查看调用阿B的API次数\n" +
                         $"c：查看API查询次数\n" +
                         $"d: 一键导入关注列表中的V(可能不全需要自己补一下)");
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.A:
                            {
                                int i = 0;
                                Console.WriteLine($"下载中的任务:");
                                foreach (var A1 in Rooms.RoomInfo)
                                {
                                    if (A1.Value.DownloadingList.Count > 0)
                                    {
                                        ulong FileSize = 0;
                                        foreach (var item in A1.Value.DownloadingList)
                                        {
                                            FileSize += (ulong)item.DownloadCount;
                                        }
                                        i++;
                                        Console.WriteLine($"{i}：{A1.Value.uid}  {A1.Value.room_id}  {A1.Value.uname}  {A1.Value.title}  {NetClass.ConversionSize(FileSize)}");
                                    }
                                }
                                break;
                            }
                        case ConsoleKey.B:
                            {
                                Console.WriteLine("API使用统计:");
                                foreach (var item in NetClass.API_Usage_Count)
                                {
                                    Console.WriteLine($"{item.Value}次，来源：{item.Key}");
                                }
                                break;
                            }
                        case ConsoleKey.C:
                            {
                                Console.WriteLine("查询API统计:");
                                foreach (var item in NetClass.SelectAPI_Count)
                                {
                                    Console.WriteLine($"{item.Value}次，来源：{item.Key}");
                                }
                                break;
                            }
                        case ConsoleKey.D:
                            DDTV_Core.SystemAssembly.BilibiliModule.API.UserInfo.follow(long.Parse(DDTV_Core.SystemAssembly.ConfigModule.BilibiliUserConfig.account.uid));
                            break;
                    }
                }
            }
        }
    }
}