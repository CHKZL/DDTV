using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.DownloadModule;
using DDTV_Core.SystemAssembly.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass;

namespace DDTV_Core.Tool
{
    public class FileOperation
    {
        public static bool SpaceIsInsufficientWarn = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.SpaceIsInsufficientWarn, "False", CoreConfigClass.Group.Core));
        private static DelEvent delEvent = new DelEvent();
        /// <summary>
        /// 硬盘快满提示
        /// </summary>
        public static event EventHandler<string> PathAlmostFull;
        /// <summary>
        /// 判断网络路径的文件是否存在
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static bool IsExistsNetFile(string Url)
        {
            try
            {
                if(string.IsNullOrEmpty(Url))
                {
                    return false;
                }
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(Url));
                if (!DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.WhetherToEnableProxy)
                {
                    httpWebRequest.Proxy = null;
                }
                httpWebRequest.Accept = "*/*";
                httpWebRequest.UserAgent = SystemAssembly.NetworkRequestModule.NetClass.UA();
                httpWebRequest.Method = "GET";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.Referer = "https://www.bilibili.com/";
                if (!string.IsNullOrEmpty(BilibiliUserConfig.account.cookie))
                {
                    httpWebRequest.Headers.Add("Cookie", BilibiliUserConfig.account.cookie);
                }
                httpWebRequest.Timeout = 5000;
                //返回响应状态是否是成功比较的布尔值
                if (((HttpWebResponse)httpWebRequest.GetResponse()).StatusCode == HttpStatusCode.OK)
                {

                }
                try
                {
                    if (httpWebRequest != null) httpWebRequest.Abort();
                }
                catch (Exception){}
                return true;
            }
            catch(WebException e) 
            {
                if(DDTV_Core.InitDDTV_Core.IsDevDebug)
                {
                    Log.AddLog(nameof(FileOperation), LogClass.LogType.Debug, $"判断远端文件：文件不存在{e.Status}({(int)e.Status})[{Url}]");
                }
                return false;
            }
            catch (Exception e)
            {
                //Log.AddLog(nameof(FileOperation), LogClass.LogType.Warn, Url+"   " + e.Message,false,null,false);
                //Log.AddLog(nameof(FileOperation),LogClass.LogType.Warn, "请求的网络路径地址:\n" + Url, false, null, false);
                return false;
            }
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="File"></param>
        public static void Del(string File)
        {
            delEvent.AddFile(File);
        }
        public static void Del(List<string> File)
        {
            foreach (var item in File)
            {
                delEvent.AddFile(item);
            }
        }
        /// <summary>
        /// 检查字符串是否符合文件路径标准
        /// </summary>
        /// <param name="Text"></param>
        /// <returns>返回清除不符合要求的字符后的字符串</returns>
        public static string CheckFilenames(string Text)
        {
            Text = Text.Replace(" ", string.Empty).Replace("/", string.Empty).Replace("\\", string.Empty).Replace("\"", string.Empty).Replace(":", string.Empty).Replace("*", string.Empty).Replace("?", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty).Replace("|", string.Empty).Replace("#", string.Empty).Replace("&", string.Empty).Replace("=", string.Empty).Replace("%", string.Empty).Replace("\0", string.Empty);
            StringBuilder rBuilder = new StringBuilder(Text);
            foreach (char rInvalidChar in Path.GetInvalidPathChars())
                rBuilder = rBuilder.Replace(rInvalidChar.ToString(), string.Empty);
            Text = rBuilder.ToString();
            return Text;
        }
        /// <summary>
        /// 替换关键字(用于替换预设的关键字如{roomid},{name}之类的)
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string ReplaceKeyword(long uid, string Text)
        {
            Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo);
 
            if (roomInfo.DownloadedFileInfo.AfterRepairFiles != null)
            {
                string FileList = string.Empty;
                foreach (var item in roomInfo.DownloadedFileInfo.AfterRepairFiles)
                {
                    FileList += item.FullName + ";";
                }
                Text = Text.Replace("{AfterRepairFiles}", FileList);
                Text = Text.Replace("{AfterLen}", roomInfo.DownloadedFileInfo.AfterRepairFiles.Count().ToString());
            }

            if (roomInfo.DownloadedFileInfo.BeforeRepairFiles != null)
            {
                string FileList = string.Empty;
                foreach (var item in roomInfo.DownloadedFileInfo.BeforeRepairFiles)
                {
                    FileList += item.FullName + ";";
                }
                Text = Text.Replace("{BeforeRepairFiles}", FileList);
                Text = Text.Replace("{BeforeLen}", roomInfo.DownloadedFileInfo.BeforeRepairFiles.Count().ToString());
            }


            if (roomInfo.DownloadedFileInfo.DanMuFile != null)
                Text = Text.Replace("{DanMuFile}", roomInfo.DownloadedFileInfo.DanMuFile.FullName);

            if (roomInfo.DownloadedFileInfo.SCFile != null)
                Text = Text.Replace("{SCFile}", roomInfo.DownloadedFileInfo.SCFile.FullName);

            if (roomInfo.DownloadedFileInfo.GuardFile != null)
                Text = Text.Replace("{GuardFile}", roomInfo.DownloadedFileInfo.GuardFile.FullName);

            if (roomInfo.DownloadedFileInfo.GiftFile != null)
                Text = Text.Replace("{GiftFile}", roomInfo.DownloadedFileInfo.GiftFile.FullName);

            Text = Text
                .Replace("{ROOMID}", Rooms.GetValue(uid, CacheType.room_id))
                .Replace("{YYYY}", DateTime.Now.ToString("yyyy"))
                .Replace("{YY}", DateTime.Now.ToString("yy"))
                .Replace("{MM}", DateTime.Now.ToString("MM"))
                .Replace("{DD}", DateTime.Now.ToString("dd"))
                .Replace("{HH}", DateTime.Now.ToString("HH"))
                .Replace("{mm}", DateTime.Now.ToString("mm"))
                .Replace("{SS}", DateTime.Now.ToString("ss"))
                .Replace("{FFFF}", DateTime.Now.ToString("fff"))
                .Replace("{yyyy}", DateTime.Now.ToString("yyyy"))
                .Replace("{yy}", DateTime.Now.ToString("yy"))
                .Replace("{MM}", DateTime.Now.ToString("MM"))
                .Replace("{dd}", DateTime.Now.ToString("dd"))
                .Replace("{HH}", DateTime.Now.ToString("HH"))
                .Replace("{mm}", DateTime.Now.ToString("mm"))
                .Replace("{ss}", DateTime.Now.ToString("ss"))
                .Replace("{fff}", DateTime.Now.ToString("fff"))
                .Replace("{NAME}", Rooms.GetValue(uid, CacheType.uname))
                .Replace("{DATE}", DateTime.Now.ToString("yyyy_MM_dd"))
                .Replace("{TIME}", DateTime.Now.ToString("HH_mm_ss"))
                .Replace("{TITLE}", Rooms.GetValue(uid, CacheType.title))
                .Replace("{R}", new Random().Next(1000, 9999).ToString());
            return Text;
        }
        /// <summary>
        /// 在指定路径中创建所有目录
        /// </summary>
        /// <param name="Path">指定的路径</param>
        /// <returns></returns>
        public static string CreateAll(string Path)
        {
            Directory.CreateDirectory(Path);
            return Path;
        }

        /// <summary>
        /// 根据文件路径判断剩余空间是否足够
        /// </summary>
        /// <param name="File">需要判断的文件（如果后面的参数全部留空则使用这个文件的大小作为判断标准）</param>
        /// <param name="GB_DemandForSpace">需要的空间_GB</param>
        /// <param name="MB_DemandForSpace">需要的空间_MB</param>
        /// <param name="KB_DemandForSpace">需要的空间_KB</param>
        /// <param name="B_DemandForSpace">需要的空间_B</param>
        /// <returns></returns>
        public static bool SpaceWillBeEnough(string File, long GB_DemandForSpace=0, long MB_DemandForSpace=0, long KB_DemandForSpace=0, long B_DemandForSpace=0)
        {
            if (System.IO.File.Exists(File))
            {
                FileInfo fileInfo = new FileInfo(File);
                long DemandForSpace = fileInfo.Length;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string PathName = fileInfo.DirectoryName.Split(':').Length > 1 ? fileInfo.DirectoryName.Split(':')[0] : Path.GetFullPath(fileInfo.DirectoryName).Split(':')[0];
                    if(GB_DemandForSpace!=0|| MB_DemandForSpace!=0|| KB_DemandForSpace !=0|| B_DemandForSpace!=0)
                    {
                        DemandForSpace = (GB_DemandForSpace * 1073741824) + (MB_DemandForSpace * 1048576) + (KB_DemandForSpace * 1024) + B_DemandForSpace;
                    }
                    if (GetHardDiskSpace(PathName, 2) < DemandForSpace)
                    {
                        if (PathAlmostFull != null)
                        {
                            PathAlmostFull.Invoke(null, $"在准备操作文件{fileInfo.Name}的时候发现硬盘剩余空间低于警戒线，硬盘空间可能不足可能造成未知的错误");
                        }
                        return false;
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                
                    DriveInfo[] BB = DriveInfo.GetDrives();
                    foreach (var item in BB)
                    {
                        if (fileInfo.DirectoryName.StartsWith(item.Name))
                        {
                            if (GB_DemandForSpace != 0 || MB_DemandForSpace != 0 || KB_DemandForSpace != 0 || B_DemandForSpace != 0)
                            {
                                DemandForSpace = (GB_DemandForSpace * 1073741824) + (MB_DemandForSpace * 1048576) + (KB_DemandForSpace * 1024) + B_DemandForSpace;
                            }
                            if (item.TotalFreeSpace< DemandForSpace)
                            {
                                PathAlmostFull.Invoke(null, $"在准备操作文件{fileInfo.Name}的时候发现硬盘剩余空间低于警戒线，硬盘空间可能不足可能造成未知的错误");
                            }
                        }
                    }
                }
            }
            return true;
        }

        private static bool IsRSWD = false;
        /// <summary>
        /// 剩余空间检测警告
        /// </summary>
        public static void RemainingSpaceWarningDetection()
        {
            if (!IsRSWD)
            {
                IsRSWD = true;
                Task.Run(() => {
                    while (true)
                    {
                        if (SpaceIsInsufficientWarn)
                        {
                            try
                            {
                                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                                {
                                    string _SavePathName = Download.DownloadPath.Split(':').Length > 1 ? Download.DownloadPath.Split(':')[0] : Path.GetFullPath(Download.DownloadPath).Split(':')[0];
                                    string _TmpPathName = Download.DownloadPath.Split(':').Length > 1 ? Download.DownloadPath.Split(':')[0] : Path.GetFullPath(Download.DownloadPath).Split(':')[0];
                                    double SaveUsingProportion = (double)GetHardDiskSpace(_SavePathName, 2) / (double)GetHardDiskSpace(_SavePathName, 1);
                                    if (SaveUsingProportion < 0.05)
                                    {
                                        PathAlmostFull.Invoke(null, "录制文件夹所在盘符剩余空间已不足5%！");
                                    }
                                    double TmpUsingProportion = (double)GetHardDiskSpace(_TmpPathName, 2) / (double)GetHardDiskSpace(_TmpPathName, 1);
                                    if (TmpUsingProportion < 0.05)
                                    {
                                        PathAlmostFull.Invoke(null, "临时文件夹所在盘符剩余空间已不足5%！");
                                    }
                                }
                                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                                {
                                    string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                                    DriveInfo[] BB = DriveInfo.GetDrives();
                                    foreach (var item in BB)
                                    {
                                        if (BaseDirectory.StartsWith(item.Name))
                                        {
                                            if ((double)item.TotalFreeSpace / (double)item.TotalSize < 0.05)
                                            {
                                                PathAlmostFull.Invoke(null, "DDTV所在盘符剩余空间已不足5%！");
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }
                        }
                        Thread.Sleep(300*1000);
                    }
                });
            }
        }

        /// <summary>
        /// 通过盘符获取剩余空间
        /// </summary>
        /// <param name="str_HardDiskName">盘符 如 C D E</param>
        /// <param name="Type">1为总空间，2为剩余空间</param>
        /// <returns></returns>
        public static long GetHardDiskSpace(string str_HardDiskName,int Type)
        {
            long totalSize = 0;
            str_HardDiskName = str_HardDiskName + ":\\";
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                if (drive.Name == str_HardDiskName)
                {
                    switch (Type)
                    {
                        case 1:
                            return drive.TotalSize;
                        case 2:
                            return drive.TotalFreeSpace;
                    }
                }
            }
            return totalSize;
        }

        /// <summary>
        /// 文件删除服务
        /// </summary>
        private class DelEvent
        {
            private List<string> DelFilelist = new();
            private bool IsDelEnable = false;
            internal void AddFile(string File)
            {
                DelFilelist.Add(File);
                if (!IsDelEnable)
                {
                    IsDelEnable = true;
                    Del();
                }
            }
            private void Del()
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            for (int i = DelFilelist.Count - 1; i >=0; i--)
                            {
                                if (File.Exists(DelFilelist[i]))
                                {
                                    try
                                    {
                                        File.Delete(DelFilelist[i]);
                                        DelFilelist.RemoveAt(i);
                                    }
                                    catch (Exception) { }
                                }
                                else
                                {
                                    try
                                    {
                                        DelFilelist.RemoveAt(i);
                                    }
                                    catch (Exception) { }
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                        Thread.Sleep(2000);
                    }
                });
            }
        }
    }
    public class DownloadList
    {
        private static ArrayList dirs = new();
        public static ArrayList GetRecFileList()
        {
            dirs.Add(SystemAssembly.DownloadModule.Download.DownloadPath);
            GetDirs(SystemAssembly.DownloadModule.Download.DownloadPath);
            object[] allDir = dirs.ToArray();
            ArrayList list = new ArrayList();
            foreach (object o in allDir)
            {
                list.AddRange(GetFileName(o.ToString()));
            }
            dirs = new();
            return list;
        } 
        private static void GetDirs(string dirPath)
        {
            if (Directory.GetDirectories(dirPath).Length > 0)
            {
                foreach (string path in Directory.GetDirectories(dirPath))
                {
                    dirs.Add(path);
                    GetDirs(path);
                }
            }
        }
        private static ArrayList GetFileName(string dirPath)
        {
            ArrayList list = new ArrayList();
            if (Directory.Exists(dirPath))
            {
                int Conut = Directory.GetFiles(dirPath).Length;
                string[] _ = new string[Conut];
                for (int i = 0 ; i < Conut ; i++)
                {
                    _[i] = Directory.GetFiles(dirPath)[i].Replace("\\", "/");
                }

                list.AddRange(_);
            }
            return list;
        }
    }

}
