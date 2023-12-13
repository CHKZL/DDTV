using ConsoleTableExt;
using Core.RuntimeObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLI
{
    internal class TerminalDisplay
    {
        internal static void DisplayRecordingStatus()
        {
            var OvarviewCardList = RoomList.GetOverview();
            int i = 0;
            //ConsoleTable tables = new ConsoleTable("序号", "UID", "房间号", "昵称", "直播标题", "已下载大小", "下载速率", "状态", "开始时间", "是否录制弹幕信息");
            var tableData = new List<List<object>> { new List<object> { "序号", "UID", "房间号", "昵称", "直播标题", "已下载大小", "下载速率", "状态", "开始时间" } };
            foreach (var row in OvarviewCardList)
            {
                tableData.Add(
                    new List<object>
                    {
                        row.id,
                        row.uid,
                        row.roomid,
                        row.name,
                        row.title,
                        Core.Tools.Linq.ConversionSize(row.downloadedSize, Core.Tools.Linq.ConversionSizeType.String),
                        Core.Tools.Linq.ConversionSize(row.downloadRate, Core.Tools.Linq.ConversionSizeType.BitRate),
                        row.state,
                        row.startTime.ToString("yyyy-MM-dd HH:mm:dd")
                    });
            }
            ConsoleTableBuilder.From(tableData).WithTitle("下载中的任务列表").ExportAndWriteLine();
        }

        internal static void SeKey()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (Console.ReadKey().Key.Equals(ConsoleKey.I))
                    {
                        Console.WriteLine();
                        Console.WriteLine($"请按对应的按键查看或修改配置：");
                        Console.WriteLine($"a：查看下载中的任务情况");
                        Console.WriteLine($"q：退出DDTV");
                        Console.WriteLine();
                        switch (Console.ReadKey().Key)
                        {
                            case ConsoleKey.A:
                                {
                                    DisplayRecordingStatus();
                                    break;
                                }
                            case ConsoleKey.Q:
                                {
                                    Console.WriteLine("确定要退出DDTV么？YES/NO");
                                    Console.WriteLine(":");
                                    Console.WriteLine();
                                    string? Cons = Console.ReadLine();
                                    if (!string.IsNullOrEmpty(Cons) && (Cons.ToLower() == "yes" || Cons.ToLower() == "y"))
                                    {
                                        Environment.Exit(0);
                                    }
                                    else
                                    {
                                        Console.WriteLine("已返回，DDTV正在运行中...");
                                    }
                                    break;
                                }

                            default:
                                break;
                        }
                    }
                }
            });
        }
    }
}
