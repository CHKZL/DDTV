using DDTV_Core.Tool.TranscodModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DDTV_Core.SystemAssembly.DownloadModule.DownloadClass;

namespace DDTV_Core.Tool.FlvModule
{
    public class FileFix
    {
        private static FlvClass.FlvTimes flvTimes =new();
        /// <summary>
        /// FLV文件头
        /// </summary>
        private static FlvClass.FlvHeader FlvHeader = new();
        /// <summary>
        /// FLV头脚本数据
        /// </summary>
        private static FlvClass.FlvTag FlvScriptTag = new();
        private static uint Count = 0;

        private static bool IsCuttingPake = true;

        public static event EventHandler<EventArgs> ClipCompleted;
        public static string Fix(string FlvFile,string AddString="修复",string R="")
        {
            flvTimes=new FlvClass.FlvTimes();
            FlvHeader=new FlvClass.FlvHeader();
            FlvScriptTag=new FlvClass.FlvTag();
            Count = 0;
            uint DataLength = 9;
            string NewFile = FlvFile.Replace(R+".flv", AddString+".flv");
            FileStream stream = new FileStream(FlvFile,FileMode.Open);
            FileStream fileStream = new FileStream(NewFile, FileMode.Create);
            while (true)
            {
                byte[] data = new byte[DataLength];
                for (int i = 0 ; i < DataLength ; i++)
                {
                    int EndF = 0;
                    if (stream.CanRead)
                    {
                        EndF = stream.ReadByte();
                    }
                    else
                    {
                        EndF = -1;
                    }
                    if (EndF != -1)
                    {
                        data[i] = (byte)EndF;
                        Count++;
                    }
                    else
                    {
                        stream.Close();
                        stream.Dispose();
                        fileStream.Close();
                        fileStream.Dispose();
                        return NewFile;
                    }
                }
                byte[] FixData = FixWrite(data, out uint DL,out uint A,out byte B);
                DataLength = DL;
                if(fileStream.CanWrite)
                {
                    if (FixData != null)
                        fileStream.Write(FixData, 0, FixData.Length);
                }
                else
                {
                    return NewFile;
                }
            }
        }
        public static void Cutting(uint Start,uint End,string FlvFile)
        {
            flvTimes = new FlvClass.FlvTimes();
            FlvHeader = new FlvClass.FlvHeader();
            FlvScriptTag = new FlvClass.FlvTag();
            Count = 0;
            uint DataLength = 9;
            string NewFile = FlvFile.Replace(".flv", "CuttingTmp.flv");
            string TmpFile= FlvFile +"Cutting";
            File.Copy(FlvFile, TmpFile);
            FlvFile = TmpFile;
            FileStream stream = new FileStream(FlvFile, FileMode.Open);
            FileStream fileStream = new FileStream(NewFile, FileMode.Create);
            while (true)
            {
                byte[] data = new byte[DataLength];
                for (int i = 0 ; i < DataLength ; i++)
                {
                    int EndF = 0;
                    if (stream.CanRead)
                    {
                        EndF = stream.ReadByte();
                    }
                    else
                    {
                        EndF = -1;
                    }
                    if (EndF != -1)
                    {
                        data[i] = (byte)EndF;
                        Count++;
                    }
                    else
                    {
                        stream.Close();
                        stream.Dispose();
                        fileStream.Close();
                        fileStream.Dispose();
                        IsCuttingPake = true;
                        FileOperation.Del(FlvFile);
                        CuttingFFMPEG(NewFile);
                        if (ClipCompleted != null)
                        {
                            ClipCompleted.Invoke(null, EventArgs.Empty);
                        }
                        return;
                    }
                }
                byte[] FixData = FixWrite(data, out uint DL, out uint PakeTime, out byte TagType);
                if (TagType == 0x08 || TagType == 0x09)
                {
                    if(PakeTime==0||(PakeTime>Start&& PakeTime<End))
                    {
                        IsCuttingPake = true;
                        if (fileStream.CanWrite)
                        {
                            fileStream.Write(FixData, 0, FixData.Length);
                        }
                        else
                        {
                            IsCuttingPake = true;
                            FileOperation.Del(FlvFile);
                            CuttingFFMPEG(NewFile);
                            if (ClipCompleted != null)
                            {
                                ClipCompleted.Invoke(null, EventArgs.Empty);
                            }
                            return;
                        }
                    }
                    else
                    {
                        IsCuttingPake = false;
                    }
                }
                else
                {             
                    if (IsCuttingPake)
                    {
                        if (fileStream.CanWrite)
                        {
                            fileStream.Write(FixData, 0, FixData.Length);
                        }
                        else
                        {
                            IsCuttingPake = true;
                            FileOperation.Del(FlvFile);
                            CuttingFFMPEG(NewFile);
                            if (ClipCompleted != null)
                            {
                                ClipCompleted.Invoke(null, EventArgs.Empty);
                            }
                            return;
                        }
                    }
                    
                }
                DataLength = DL;
            }

        }
        public static void CuttingFFMPEG(string FilePath)
        {
            string F = Fix(FilePath,"切片", "CuttingTmp");
            FileOperation.Del(FilePath);
            TranscodClass transcodClass = new TranscodClass()
            {
                AfterFilenameExtension = ".mp4",
                AfterFilePath = F.Replace(".flv", ".mp4"),
                BeforeFilePath = F
            };
            Transcod.CallFFMPEG(transcodClass);
            FileOperation.Del(F);
          
        }

        private static long FlvAudioFPS = 0;
        private static long FlvVideoFPS = 0;
        private static long ErrorAudioFPS = 0;
        private static long ErrorVideoFPS = 0;
        private static byte[] FixWrite(byte[] data, out uint Len,out uint PakeTime,out byte TagType)
        {
            if (Count > 9)
            {
                if (flvTimes.IsTagHeader)
                {
                    flvTimes.IsTagHeader = !flvTimes.IsTagHeader;
                    flvTimes.FlvTotalTagCount++;
                    TagType = data[4];
                    switch (data[4])
                    {
                        case 0x12:
                            {
                                PakeTime = 0;
                                //downloads.FlvScriptTag.tag=new byte[data.Length];
                                //downloads.FlvScriptTag.tag=data;
                                Len = BitConverter.ToUInt32(new byte[] { data[7], data[6], data[5], 0x00 }, 0);
                                FlvScriptTag.PreTagsize[0] = data[0];
                                FlvScriptTag.PreTagsize[1] = data[1];
                                FlvScriptTag.PreTagsize[2] = data[2];
                                FlvScriptTag.PreTagsize[3] = data[3];

                                FlvScriptTag.TagType = data[4];
                                FlvScriptTag.TagDataSize[0] = data[5];
                                FlvScriptTag.TagDataSize[1] = data[6];
                                FlvScriptTag.TagDataSize[2] = data[7];

                                FlvScriptTag.Timestamp[0] = 0x00;
                                FlvScriptTag.Timestamp[1] = 0x00;
                                FlvScriptTag.Timestamp[2] = 0x00;
                                FlvScriptTag.Timestamp[3] = 0x00;
                                data[8] = 0x00;
                                data[9] = 0x00;
                                data[10] = 0x00;
                                data[11] = 0x00;

                                data[12] = 0x00;
                                data[13] = 0x00;
                                data[14] = 0x00;
                                flvTimes.TagType = 0x12;
                                return data;
                            }
                        case 0x08:
                            {

                                Len = BitConverter.ToUInt32(new byte[] { data[7], data[6], data[5], 0x00 }, 0);
                                byte[] TD = new byte[] { data[11], data[8], data[9], data[10] };
                                Array.Reverse(TD);
                                uint Time = BitConverter.ToUInt32(TD, 0);
                                if (Time > 0 && flvTimes.ErrorAudioTimes == 0)
                                {
                                    flvTimes.ErrorAudioTimes = Time;
                                }
                                data[4] = 0x08;
                                byte[] c = BitConverter.GetBytes(Time - flvTimes.ErrorAudioTimes- ErrorAudioFPS);
                                data[8] = c[2];
                                data[9] = c[1];
                                data[10] = c[0];
                                data[11] = c[3];
                                PakeTime = BitConverter.ToUInt32(new byte[] { data[10], data[9], data[8], data[11] }, 0);
//#if DEBUG
                                if (PakeTime - FlvAudioFPS > 500)
                                {
                                    data = null;
                                    ErrorAudioFPS = PakeTime - FlvAudioFPS;
                                    FlvAudioFPS = PakeTime;
                                    return data;
                                }
                                else
                                {
                                    Console.WriteLine($"从文件中加载FlvTag包属性:[音频包]，TagData数据长度[{Len}],检测到时间戳错误，修复时间戳为[{BitConverter.ToUInt32(new byte[] { data[10], data[9], data[8], data[11] }, 0)}]");
                                }
//#endif
                                FlvAudioFPS = PakeTime;
                                if (data != null)
                                {
                                    flvTimes.TagType = 0x08;
                                    if (flvTimes.FlvAudioTagCount == 0)
                                    {
                                        FlvScriptTag.FistAbody = new byte[data.Length];
                                        FlvScriptTag.FistAbody = data;
                                    }
                                    flvTimes.FlvAudioTagCount++;
                                }
                                return data;
                            }
                        case 0x09:
                            {
                                Len = BitConverter.ToUInt32(new byte[] { data[7], data[6], data[5], 0x00 }, 0);
                                byte[] TD = new byte[] { data[11], data[8], data[9], data[10] };
                                Array.Reverse(TD);
                                uint Time = BitConverter.ToUInt32(TD, 0);
                                if (Time > 0 && flvTimes.ErrorVideoTimes == 0)
                                {
                                    flvTimes.ErrorVideoTimes = Time;
                                }
                                data[4] = 0x09;
                                byte[] c = BitConverter.GetBytes(Time - flvTimes.ErrorVideoTimes- ErrorVideoFPS);
                                data[8] = c[2];
                                data[9] = c[1];
                                data[10] = c[0];
                                data[11] = c[3];
                                PakeTime = BitConverter.ToUInt32(new byte[] { data[10], data[9], data[8], data[11] }, 0);

//#if DEBUG
                                if (PakeTime - FlvVideoFPS > 500)
                                {
                                    data = null;
                                    ErrorVideoFPS = PakeTime - FlvVideoFPS;
                                    FlvVideoFPS = PakeTime;
                                    return data;
                                }
                                else
                                {
                                    Console.WriteLine($"从文件中加载FlvTag包属性:[视频包]，TagData数据长度[{Len}],检测到时间戳错误，修复时间戳为[{BitConverter.ToUInt32(new byte[] { data[10], data[9], data[8], data[11] }, 0)}]");
                                }
//#endif
                                FlvVideoFPS= PakeTime;


                                flvTimes.TagType = 0x09;
                                    if (flvTimes.FlvVideoTagCount == 0)
                                    {
                                        FlvScriptTag.FistVbody = new byte[data.Length];
                                        FlvScriptTag.FistVbody = data;
                                    }

                                   flvTimes.FlvVideoTagCount++;
                                
                                return data;
                            }
                        default:
                            {
                                Len = 15;
                                PakeTime = 0;
                                return data;
                            }
                    }

                }
                else
                {
                    flvTimes.IsTagHeader = !flvTimes.IsTagHeader;
                    if (flvTimes.FlvTotalTagCount < 2)
                    {
                        FlvScriptTag.TagaData = data;
                    }
                    else
                    {
                        switch (flvTimes.TagType)
                        {
                            case 0x08:
                                if (flvTimes.FlvAudioTagCount == 1)
                                {
                                    byte[] TEST = new byte[FlvScriptTag.FistAbody.Length + data.Length];
                                    FlvScriptTag.FistAbody.CopyTo(TEST, 0);
                                    data.CopyTo(TEST, FlvScriptTag.FistAbody.Length);
                                    FlvScriptTag.FistAbody = TEST;
                                }
                                break;
                            case 0x09:
                                if (flvTimes.FlvVideoTagCount == 1)
                                {
                                    byte[] TEST = new byte[FlvScriptTag.FistVbody.Length + data.Length];
                                    FlvScriptTag.FistVbody.CopyTo(TEST, 0);
                                    data.CopyTo(TEST, FlvScriptTag.FistVbody.Length);
                                    FlvScriptTag.FistVbody = TEST;
                                }
                                break;
                        }
                        flvTimes.FlvAudioTagCount++;
                    }
                    Len = 15;
                    //IsCuttingPake = false;
                    TagType = 0x00;
                    PakeTime = 0;
                    return data;
                }
            }
            else
            {
                FlvHeader.Header = new byte[data.Length];
                FlvHeader.Header = data;
                FlvHeader.Signature[0] = data[0];
                FlvHeader.Signature[1] = data[1];
                FlvHeader.Signature[2] = data[2];
                FlvHeader.Version = data[3];
                FlvHeader.TypeFlagsAudio = GetBitValue(data[4], 2);
                FlvHeader.TypeFlagsVideo = GetBitValue(data[4], 0);
                FlvHeader.Type = data[4];
                FlvHeader.FlvHeaderOffset[0] = data[5];
                FlvHeader.FlvHeaderOffset[1] = data[6];
                FlvHeader.FlvHeaderOffset[2] = data[7];
                FlvHeader.FlvHeaderOffset[3] = data[8];
                Len = 15;
                TagType = 0x00;
                PakeTime = 0;
                return data;
            }
        }
        private static bool GetBitValue(byte value, byte bit)
        {
            return (value & (byte)Math.Pow(2, bit)) > 0 ? true : false;
        }
    }
}
