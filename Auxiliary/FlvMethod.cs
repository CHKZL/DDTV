using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    public class FlvMethod
    {

        public class Flv
        {
            public string File1Url { set; get; }
            public string File2Url { set; get; }
        }
        public static string FlvSum(Flv A,bool 是否直播结束)
        {
            String path1 = A.File1Url;
            String path2 = A.File2Url;
            if(!File.Exists(path1))
            {
                InfoLog.InfoPrintf("续录文件[" + path1 + "]文件不存在，不符合合并条件，文件合并取消", InfoLog.InfoClass.Debug);
                return null;
            }
            else if (!File.Exists(path2))
            {
                InfoLog.InfoPrintf("续录文件[" + path2 + "]文件不存在，不符合合并条件，文件合并取消", InfoLog.InfoClass.Debug);
                return null;
            }
            String output;
            if (是否直播结束)
            {
                output = A.File1Url.Replace("_202", "⒂").Split('⒂')[0] + ".flv";
            }
            else
            {
                output = A.File1Url.Replace("_202", "⒂").Split('⒂')[0] + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".flv";
            }
            if(File.Exists(output))
            {
                output.Replace(".flv","1.flv");
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
                        InfoLog.InfoPrintf("1该视频不适合合并，放弃合并", InfoLog.InfoClass.下载必要提示);
                        return null;
                    }
                    int time = Merge(fs1, fsMerge, true, 0);
                    time = Merge(fs2, fsMerge, false, time);
                    //time = Merge(fs3, fsMerge, false, time);
                    InfoLog.InfoPrintf("续录文件[" + output + "]合并完成", InfoLog.InfoClass.下载必要提示);
                    fs1.Close();
                    fs1.Dispose();
                    fs2.Close();
                    fs2.Dispose();
                    fsMerge.Close();
                    fsMerge.Dispose();
                    try
                    {
                        if(File.Exists(output))
                        {
                            File.Delete(path1);
                            File.Delete(path2);
                        }
                    }
                    catch (Exception)
                    {

                    }
                    if (是否直播结束)
                    {
                        try
                        {
                            转码(output);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else
                    {
                        GC.Collect();
                    }
                    return output;
                }
                else
                {
                    InfoLog.InfoPrintf("2该视频不适合合并，放弃合并", InfoLog.InfoClass.下载必要提示);
                    return null;
                }
                
            }
        }
        /// <summary>
        /// 调用ffmpeg修复阿B的傻逼时间轴，顺便封装成MP4
        /// </summary>
        /// <param name="Filename">转码文件</param>
        public static void 转码(string Filename)
        {
            if (MMPU.转码功能使能)
            {
                try
                {
                    Process process = new Process();                   
                    process.StartInfo.FileName = "./libffmpeg/ffmpeg.exe";  
                    process.StartInfo.Arguments = "-i " + Filename + " -vcodec copy -acodec copy " + Filename.Replace(".flv", "") + ".mp4";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.ErrorDataReceived += new DataReceivedEventHandler(Output);  // 捕捉ffmpeg.exe的信息
                    DateTime beginTime = DateTime.Now;
                    process.Start();
                    process.BeginErrorReadLine();   // 开始异步读取
                    process.Exited += Process_Exited;
                    GC.Collect();
                }
                catch (Exception)
                {
                }
            }
            else
            {
                GC.Collect();
            }
           
        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            Process P = (Process)sender;
            InfoLog.InfoPrintf("转码任务完成:"+P.StartInfo.Arguments, InfoLog.InfoClass.下载必要提示);
        }

        private static void Output(object sender, DataReceivedEventArgs e)
        {
            InfoLog.InfoPrintf(e.Data, InfoLog.InfoClass.Debug);
           // Console.WriteLine(e.Data);
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
    }
}
