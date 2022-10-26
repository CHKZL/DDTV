using DDTV_Core.SystemAssembly.Log;
using DDTV_Core.Tool.TranscodModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.Tool
{
    public class HlsModule
    {
        public class Sun
        {
            public static TranscodClass HLS_SUN(TranscodClass transcodClass)
            {
                try
                {
                    transcodClass.IsTranscod = true;
                    Log.AddLog(nameof(HlsModule), LogClass.LogType.Info, $"开始合并HLS文件：[{transcodClass.HLS_Files[0]}]");
                    using (FileStream fs = new FileStream($"{transcodClass.AfterFilePath}{transcodClass.AfterFilenameExtension}", FileMode.Create))
                    {
                       
                        transcodClass.AfterFilePath = $"{transcodClass.AfterFilePath}{transcodClass.AfterFilenameExtension}";
                        string AddFilePath = $"{transcodClass.AfterFilePath.Replace(transcodClass.AfterFilePath.Split('/')[transcodClass.AfterFilePath.Split('/').Length - 1], "")}{transcodClass.HLS_Files[0]}";
                        fs.Write(File.ReadAllBytes(AddFilePath));
                        FileOperation.Del(AddFilePath);
                        if (transcodClass.HLS_Files.Count > 2)
                        {
                            bool F = true;
                            int i = 0;
                            foreach (var item in transcodClass.HLS_Files)
                            {
                                if (F)
                                {
                                    F = false;
                                }
                                else
                                {
                                    AddFilePath = $"{transcodClass.AfterFilePath.Replace(transcodClass.AfterFilePath.Split('/')[transcodClass.AfterFilePath.Split('/').Length - 1], "")}{item}.m4s";
                                    fs.Write(File.ReadAllBytes(AddFilePath));
                                    Tool.FileOperation.Del(AddFilePath);
                                    i++;
                                }
                            }
                            Log.AddLog(nameof(HlsModule), LogClass.LogType.Info, $"HLS文件合并完成:{transcodClass.AfterFilePath}{transcodClass.AfterFilenameExtension}");
                            transcodClass.IsTranscod = false;
                            return transcodClass;
                        }
                        else
                        {
                            Log.AddLog(nameof(HlsModule), LogClass.LogType.Info, $"HLS文件合并未达到基础要求数量，放弃合并");
                            transcodClass.IsTranscod = false;
                            return transcodClass;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.AddLog(nameof(Transcod), LogClass.LogType.Warn, "HLS文件合并处理出现致命错误！错误信息:\n" + e.ToString(), true, e, true);
                    transcodClass.IsTranscod = false;
                    return transcodClass;
                }
               
            }
        }
    }
}
