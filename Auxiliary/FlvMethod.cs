using Auxiliary.FLVEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using static Auxiliary.Downloader;

namespace Auxiliary
{
    public class FlvMethod
    {

        public class Flv
        {
            public string File1Url { set; get; }
            public string File2Url { set; get; }
        }
        public static string FlvSum(Flv A, bool 是否直播结束)
        {
            List<string> DelFileList = new List<string>();
            String path1 = A.File1Url;
            String path2 = A.File2Url;
            if (!File.Exists(path1))
            {
                InfoLog.InfoPrintf("续录文件[" + path1 + "]文件不存在，不符合合并条件，文件合并取消", InfoLog.InfoClass.Debug);
                return null;
            }
            else if (!File.Exists(path2))
            {
                InfoLog.InfoPrintf("续录文件[" + path2 + "]文件不存在，不符合合并条件，文件合并取消", InfoLog.InfoClass.Debug);
                return null;
            }
            string output = "";
            if (是否直播结束)
            {
                string file = A.File1Url.Replace("_202", "⒂").Split('⒂')[0];
                if (file.Substring(file.Length - 4, 4) == ".flv")
                {
                    file = file.Substring(0, file.Length - 4);
                }
                if (!string.IsNullOrEmpty(file))
                {
                    output = file + $"_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}合并.flv";
                }
                else
                {
                    string T1 = A.File1Url.Replace("_202", "⒂").Split('⒂')[0];
                    if (T1.Substring(T1.Length - 4, 4) == ".flv")
                    {
                        T1 = T1.Substring(0, T1.Length - 4);
                    }
                    if (!string.IsNullOrEmpty(T1))
                    {
                        output = T1 + $"_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}合并.flv";
                    }
                    else
                    {
                        output = $"_{new Random().Next(10000, 99999)}合并.flv";
                    }
                }
            }
            else
            {
                output = A.File1Url.Replace("_202", "⒂").Split('⒂')[0] + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".flv";
            }
            if (File.Exists(output))
            {
                output.Replace(".flv", new Random().Next(1000, 9999) + ".flv");
            }
            using (FileStream fs1 = new FileStream(path1, FileMode.Open))
            using (FileStream fs2 = new FileStream(path2, FileMode.Open))
            //using (FileStream fs3 = new FileStream(path3, FileMode.Open))
            using (FileStream fsMerge = new FileStream(output, FileMode.Create))
            {
                if (GetFLVFileInfo(fs1) != null && GetFLVFileInfo(fs2) != null)
                {
                    if (IsSuitableToMerge(GetFLVFileInfo(fs1), GetFLVFileInfo(fs2)) == false
                        )//|| IsSuitableToMerge(GetFLVFileInfo(fs1), GetFLVFileInfo(fs3)) == false)
                    {
                        InfoLog.InfoPrintf("1该视频不适合合并，放弃合并", InfoLog.InfoClass.下载系统信息);
                        return null;
                    }
                    int time = Merge(fs1, fsMerge, true, 0);
                    time = Merge(fs2, fsMerge, false, time);
                    //time = Merge(fs3, fsMerge, false, time);
                    InfoLog.InfoPrintf("续录文件[" + output + "]合并完成", InfoLog.InfoClass.下载系统信息);
                    fs1.Close();
                    fs1.Dispose();
                    fs2.Close();
                    fs2.Dispose();
                    fsMerge.Close();
                    fsMerge.Dispose();
                    //if (是否直播结束)
                    //{
                    //    try
                    //    {
                    //        转码(output);
                    //    }
                    //    catch (Exception)
                    //    {
                    //    }
                    //}
                    //else
                    //{
                    //    GC.Collect();
                    //}
                    return output;
                }
                else
                {
                    InfoLog.InfoPrintf("2该视频不适合合并，放弃合并", InfoLog.InfoClass.下载系统信息);
                    return null;
                }

            }
        }
        /// <summary>
        /// 调用ffmpeg修复阿B的傻逼时间轴，顺便封装成MP4
        /// </summary>
        /// <param name="Filename">转码文件</param>
        public static void 转码(string Filename,DownIofoData downIofoData)
        {
            if (MMPU.转码功能使能)
            {
                try
                {
                    downIofoData.是否转码中 = true;
                    ProcessPuls process = new ProcessPuls();
                    TimeSpan all = new TimeSpan(), now = new TimeSpan();
                    int progress = -1;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        process.StartInfo.FileName = "./libffmpeg/ffmpeg.exe";
                    }
                    else
                    {
                        process.StartInfo.FileName = "ffmpeg";
                    }
                    process.StartInfo.Arguments = "-i " + Filename + " -vcodec copy -acodec copy " + Filename.Replace(".flv", "") + ".mp4";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.CreateNoWindow = true; // 不显示窗口。
                    process.EnableRaisingEvents = true;
                    process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
                    {
                        string stringResults = e.Data;
                        if (stringResults == "" || stringResults == null) return;
                        //Console.WriteLine(stringResults);
                        if (stringResults.Contains("Duration"))
                        {
                            all = TimeSpan.Parse(Regex.Match(stringResults, @"(?<=Duration: ).*?(?=, start)").Value);
                        }
                        if (stringResults.Contains("time"))
                        {
                            string tmpNow = Regex.Match(stringResults, @"(?<= time=).*?(?= bitrate)").Value;
                            if (tmpNow != "")
                            {
                                now = TimeSpan.Parse(tmpNow);
                                progress = (int)Math.Ceiling(now.TotalMilliseconds / all.TotalMilliseconds * 100);
                            }
                        }
                        if (progress != -1)
                        {
                            InfoLog.InfoPrintf($"转码进度:{progress}%", InfoLog.InfoClass.下载系统信息);
                            if(downIofoData!=null)
                            {
                                downIofoData.转码进度 = progress;
                            }
                            else
                            {
                                downIofoData.转码进度 = -1;
                            }
                        }
                        //Console.WriteLine(progress);
                    };  // 捕捉的信息
                    DateTime beginTime = DateTime.Now;
                    process.Start();
                    process.BeginErrorReadLine();   // 开始异步读取
                    process.Exited += delegate (object sender, EventArgs e)
                    {
                        ProcessPuls P = (ProcessPuls)sender;
                        InfoLog.InfoPrintf("转码任务完成:" + P.StartInfo.Arguments, InfoLog.InfoClass.下载系统信息);
                        if (MMPU.转码后自动删除文件)
                        {
                            MMPU.文件删除委托(Filename, "转码完成自动，删除原始文件"); //使用传入参数删除原flv文件
                        }
                    };

                    //NagisaCo: 等待转码，使mp4文件完整后再开始上传功能
                    process.WaitForExit();
                    process.Close();

                    GC.Collect();

                }
                catch (Exception)
                {
                }
                downIofoData.是否转码中 = false;
            }
            else
            {
                GC.Collect();
            }

        }
        public class ProcessPuls : Process
        {
            public string OriginalVideoFilename = "";
            public string NewVideoFilename = "";

        }

        const int FLV_HEADER_SIZE = 9;
        const int FLV_TAG_HEADER_SIZE = 11;
        const int MAX_DATA_SIZE = 16777220;
        class FLVContext
        {
            public byte soundFormat;
            public byte soundRate;
            public byte soundSize;
            public byte soundType;
            public byte videoCodecID;
        }

        static bool IsSuitableToMerge(FLVContext flvCtx1, FLVContext flvCtx2)
        {
            return (flvCtx1.soundFormat == flvCtx2.soundFormat) &&
              (flvCtx1.soundRate == flvCtx2.soundRate) &&
              (flvCtx1.soundSize == flvCtx2.soundSize) &&
              (flvCtx1.soundType == flvCtx2.soundType) &&
              (flvCtx1.videoCodecID == flvCtx2.videoCodecID);
        }

        static bool IsFLVFile(FileStream fs)
        {
            byte[] buf = new byte[FLV_HEADER_SIZE];
            fs.Position = 0;
            if (FLV_HEADER_SIZE != fs.Read(buf, 0, buf.Length))
                return false;

            if (buf[0] != 'F' || buf[1] != 'L' || buf[2] != 'V' || buf[3] != 0x01)
                return false;
            else
                return true;
        }

        static FLVContext GetFLVFileInfo(FileStream fs)
        {
            bool hasAudioParams, hasVideoParams;
            int skipSize;
            int dataSize;
            byte tagType;
            byte[] tmp = new byte[FLV_TAG_HEADER_SIZE + 1];
            if (fs == null) return null;

            FLVContext flvCtx = new FLVContext();
            fs.Position = 0;
            skipSize = 9;
            fs.Position += skipSize;
            hasVideoParams = hasAudioParams = false;
            skipSize = 4;
            while (!hasVideoParams || !hasAudioParams)
            {
                fs.Position += skipSize;

                if (FLV_TAG_HEADER_SIZE + 1 != fs.Read(tmp, 0, tmp.Length))
                    return null;

                tagType = (byte)(tmp[0] & 0x1f);
                switch (tagType)
                {
                    case 8:
                        flvCtx.soundFormat = (byte)((tmp[FLV_TAG_HEADER_SIZE] & 0xf0) >> 4);
                        flvCtx.soundRate = (byte)((tmp[FLV_TAG_HEADER_SIZE] & 0x0c) >> 2);
                        flvCtx.soundSize = (byte)((tmp[FLV_TAG_HEADER_SIZE] & 0x02) >> 1);
                        flvCtx.soundType = (byte)((tmp[FLV_TAG_HEADER_SIZE] & 0x01) >> 0);
                        hasAudioParams = true;
                        break;
                    case 9:
                        flvCtx.videoCodecID = (byte)((tmp[FLV_TAG_HEADER_SIZE] & 0x0f));
                        hasVideoParams = true;
                        break;
                    default:
                        break;
                }

                dataSize = FromInt24StringBe(tmp[1], tmp[2], tmp[3]);
                skipSize = dataSize - 1 + 4;
            }

            return flvCtx;
        }

        static int FromInt24StringBe(byte b0, byte b1, byte b2)
        {
            return (int)((b0 << 16) | (b1 << 8) | (b2));
        }

        static int GetTimestamp(byte b0, byte b1, byte b2, byte b3)
        {
            return ((b3 << 24) | (b0 << 16) | (b1 << 8) | (b2));
        }

        static void SetTimestamp(byte[] data, int idx, int newTimestamp)
        {
            data[idx + 3] = (byte)(newTimestamp >> 24);
            data[idx + 0] = (byte)(newTimestamp >> 16);
            data[idx + 1] = (byte)(newTimestamp >> 8);
            data[idx + 2] = (byte)(newTimestamp);
        }

        static int Merge(FileStream fsInput, FileStream fsMerge, bool isFirstFile, int lastTimestamp = 0)
        {
            int readLen;
            int curTimestamp = 0;
            int newTimestamp = 0;
            int dataSize;
            byte[] tmp = new byte[20];
            byte[] buf = new byte[MAX_DATA_SIZE];

            fsInput.Position = 0;
            if (isFirstFile)
            {
                if (FLV_HEADER_SIZE + 4 == (fsInput.Read(tmp, 0, FLV_HEADER_SIZE + 4)))
                {
                    fsMerge.Position = 0;
                    fsMerge.Write(tmp, 0, FLV_HEADER_SIZE + 4);
                }
            }
            else
            {
                fsInput.Position = FLV_HEADER_SIZE + 4;
            }

            while (fsInput.Read(tmp, 0, FLV_TAG_HEADER_SIZE) > 0)
            {
                dataSize = FromInt24StringBe(tmp[1], tmp[2], tmp[3]);
                curTimestamp = GetTimestamp(tmp[4], tmp[5], tmp[6], tmp[7]);
                newTimestamp = curTimestamp + lastTimestamp;
                SetTimestamp(tmp, 4, newTimestamp);
                fsMerge.Write(tmp, 0, FLV_TAG_HEADER_SIZE);

                readLen = dataSize + 4;
                if (fsInput.Read(buf, 0, readLen) > 0)
                {
                    fsMerge.Write(buf, 0, readLen);
                }
                else
                {
                    goto failed;
                }
            }

            return newTimestamp;

        failed:
            throw new Exception("Merge Failed");
        }

        /// <summary>
        /// 修复FLV(未完工)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int 修复flvMetaData(string[] args)
        {
            CmdModel model = new CmdModel();
            CommandLineParser<CmdModel> parser = new CommandLineParser<CmdModel>(model);
            parser.CaseSensitive = false;
            parser.AssignmentSyntax = true;
            parser.WriteUsageOnError = true;
            if (!parser.Parse(args))
                return 1;

            if (model.FixMetadata && model.RemoveMetadata)
            {
                Console.WriteLine("fixMeta and noMeta flags conflicts.");
                return 1;
            }

            if (model.FromSeconds.HasValue && model.ToSeconds.HasValue && model.FromSeconds.Value >= model.ToSeconds.Value)
            {
                Console.WriteLine("Start of output window (from) should be larger than end (to).");
                return 1;
            }

            Console.WriteLine("Input file: {0}", model.InputFile);

            Stream inputStream;
            if (model.OutputFile == null)
            {
                Console.WriteLine("Loading whole file to memory.");
                inputStream = new MemoryStream();
                using (FileStream fs = new FileStream(model.InputFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    fs.CopyTo(inputStream);
                inputStream.Position = 0;
            }
            else
                inputStream = new FileStream(model.InputFile, FileMode.Open, FileAccess.Read, FileShare.Read);

            DateTime fileDate = File.GetLastWriteTime(model.InputFile);

            FLVFile file = new FLVFile(inputStream);

            file.PrintReport();

            //if (model.FromSeconds.HasValue)
            //    file.CutFromStart(TimeSpan.FromSeconds(model.FromSeconds.Value));
            //if (model.ToSeconds.HasValue)
            //    file.CutToEnd(TimeSpan.FromSeconds(model.ToSeconds.Value));
            //if (model.FilterPackets)
            //    file.FilterPackets();
            ////if (model.FixTimestamps)
            //    file.FixTimeStamps();
            ////if (model.FixMetadata)
            //    file.FixMetadata();
            //if (model.RemoveMetadata)
            //    file.RemoveMetadata();



            file.FilterPackets();

            file.FixTimeStamps();

            file.FixMetadata();



            //if (!(model.FilterPackets || model.FixMetadata || model.FixTimestamps || model.RemoveMetadata || model.FromSeconds.HasValue || model.ToSeconds.HasValue))
            //{
            //    Console.WriteLine("No actions set. Exiting.");
            //    return 0;
            //}

            string outputFile = model.OutputFile ?? model.InputFile;
            Console.WriteLine("Writing: {0}", outputFile);
            file.Write(outputFile);

            inputStream.Dispose();

            if (model.PreserveDate)
                File.SetLastWriteTime(outputFile, fileDate);
            return 0;
        }
    }
}
