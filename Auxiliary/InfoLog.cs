using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    /// <summary>
    /// 重构DDTV整个项目所使用的消息系统，用于区分DDTV和DDTVLIve以及WEB项目中不同等级的提示和提示方式
    /// </summary>
    public class InfoLog
    {
        public static FileStream InfoInitFS;
        public static StreamWriter InfoInitSW;
        public static InfoClasslBool ClasslBool = new InfoClasslBool();
        public class InfoClasslBool
        {
            public bool Debug { set; get; } = false;
            public bool 系统错误信息 { set; get; } = false;
            public bool 杂项提示 { set; get; } = false;
            public bool 下载必要提示 { set; get; } = false;
            public bool 输出到文件 { set; get; } = false;
        }
        public enum InfoClass
        {
            Debug = 0,
            系统错误信息 = 1,
            杂项提示 = 2,
            下载必要提示 = 3
        }
        /// <summary>
        /// 返回下载列表HTML字符串
        /// </summary>
        /// <returns></returns>
        public static string DownloaderInfoPrintf()
        {
            string 返回字符串 = "<html><head><title>%title%</title><meta charset=\"utf-8\"></head><body><table border=\"5\">";
            List<string> 下载任务 = new List<string>();
            int 计数 = 1;
            int 完成数量 = 0;
            int 正在下载数量 = 0;
            下载任务.Add(string.Format("<tr><td>序号</td><td>房间号</td><td>下载状态</td><td>已下载大小</td><td>备注</td><td>主播名称</td></td>"));
            foreach (var item in MMPU.DownList)
            {
                
                下载任务.Add(string.Format("<tr><td>"+ 计数 + "</td><td>"+ item.DownIofo.房间_频道号 + "</td><td>"+(item.DownIofo.备注.Contains("断开")? "■下载完成" : (item.DownIofo.下载状态 ? "□正在下载" : "■下载完成")) + "</td><td> "+ item.DownIofo.已下载大小str + "</td><td>"+ item.DownIofo.备注 + "</td><td>"+ item.DownIofo.主播名称 + "</td></td>"));
                计数++;
                if (item.DownIofo.下载状态)
                {
                    正在下载数量++;
                }
                else
                {
                    完成数量++;
                }
            }
            
            for (int i = 0; i < 下载任务.Count; i++)
            {
                返回字符串 = 返回字符串 + 下载任务[i];
            }
            返回字符串 = (返回字符串 + "</table></body></html>").Replace("%title%", "正在下载:"+ 正在下载数量+" 已经完成:"+完成数量);
            return 返回字符串;
        }
        public static void InfoInit(string LogFile, InfoClasslBool InfoLvevlBool)
        {
            if(!Directory.Exists("./LOG"))
            {
                Directory.CreateDirectory("./LOG");
            }
            LogFile = "./LOG/" + LogFile;
            if (File.Exists(LogFile))
            {
                File.Move(LogFile, LogFile + ".backup_" + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
            }
            InfoInitFS = new FileStream(LogFile, FileMode.Append);
            InfoInitSW = new StreamWriter(InfoInitFS);
            ClasslBool = InfoLvevlBool;
        }
        public static void InfoPrintf(string mess, InfoClass Class)
        {  
            string A = "";
            switch (Class)
            {
                case InfoClass.Debug:
                    {
                        if (ClasslBool.Debug)
                        {
                           
                            Console.WriteLine("\r\n ========= DebugBeginning =========");
                            Console.WriteLine("[Debug] "+DateTime.Now.ToString("MM-dd HH:mm:ss")+": " + mess );
                            Console.WriteLine("=========DebugEnd=========");
                            A = "\r\n=========DebugBeginning=========" + "\r\n[Debug] " + DateTime.Now.ToString("MM-dd HH:mm:ss") + ": " + mess + "\r\n=========DebugEnd=========";
                        }
                    }
                    break;
                case InfoClass.系统错误信息:
                    {
                        if (ClasslBool.系统错误信息)
                        {
                            Console.WriteLine("\r\n ========= SysteErrorInfoBeginning =========");
                            Console.WriteLine("[SysteErrorInfo]: " + mess);
                            Console.WriteLine("=========SysteErrorInfoEnd=========");
                            A = "\r\n=========SysteErrorInfoBeginning=========" + "\r\n[SysteErrorInfo]: " + mess + "\r\n=========SysteErrorInfoEnd=========";
                           
                        }
                    }
                    break;
                case InfoClass.杂项提示:
                    {
                        if (ClasslBool.杂项提示)
                        {                    
                            Console.WriteLine("[Info]: " + mess);
                            A = "\r\n[Info]: " + mess;
                        }
                    }
                    break;
                case InfoClass.下载必要提示:
                    {
                        if (ClasslBool.下载必要提示)
                        {
                            Console.WriteLine("[下载系统消息]" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + mess);
                            A = "\r\n[下载系统消息]" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + mess;
                        }
                    }
                    break; 
            }
            if (ClasslBool.输出到文件 && A.Length > 2)
            {
                OutToFile(A);
            }
        }
        public static void OutToFile(string str)
        {
           
            InfoInitSW.WriteLine(str);
            InfoInitSW.Flush();
        }
    }
}
