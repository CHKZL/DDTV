using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DDTV_Core.SystemAssembly.DownloadModule.DownloadClass;

namespace DDTV_Core.Tool.FlvModule
{
    public class SteamFix
    {
        private static long FlvAudioFPS = 0;
        private static long FlvVideoFPS = 0;
        private static long ErrorAudioFPS = 0;
        private static long ErrorVideoFPS = 0;

        private static byte[] EndFlvAudioData = null;
        private static byte[] EndFlvVideoData = null;
        public static byte[] FixWrite(byte[] data, Downloads downloads, out uint Len,out bool IsError)
        {
            uint PakeTime = 0;
            IsError=false;
            if (downloads.DownloadCount>9)
            {
                bool IsErrorLen = false;
              
                if (downloads.flvTimes.IsTagHeader)
                {
                    downloads.flvTimes.IsTagHeader=!downloads.flvTimes.IsTagHeader;
                    downloads.flvTimes.FlvTotalTagCount++;
                    if (data.Length > 15 && data[7] == 0xFF && data[6] == 0xFF && data[5] == 0xFF)
                    {
                        IsError = true;
                       
                    }

                    switch (data[4])
                    {
                        case 0x12:
                            {
                                //downloads.FlvScriptTag.tag=new byte[data.Length];
                                //downloads.FlvScriptTag.tag=data;
                                Len =BitConverter.ToUInt32(new byte[] {  data[7], data[6], data[5], 0x00 }, 0);
                                downloads.FlvScriptTag.PreTagsize[0] = data[0];
                                downloads.FlvScriptTag.PreTagsize[1] = data[1];
                                downloads.FlvScriptTag.PreTagsize[2] = data[2];
                                downloads.FlvScriptTag.PreTagsize[3] = data[3];

                                downloads.FlvScriptTag.TagType=data[4];
                                downloads.FlvScriptTag.TagDataSize[0]= data[5];
                                downloads.FlvScriptTag.TagDataSize[1]= data[6];
                                downloads.FlvScriptTag.TagDataSize[2]= data[7];
                                
                                downloads.FlvScriptTag.Timestamp[0]= 0x00;
                                downloads.FlvScriptTag.Timestamp[1]= 0x00;
                                downloads.FlvScriptTag.Timestamp[2]= 0x00;
                                downloads.FlvScriptTag.Timestamp[3]= 0x00;
                                data[8]=0x00;
                                data[9]=0x00;
                                data[10]=0x00;
                                data[11]=0x00;

                                data[12]=0x00;
                                data[13]=0x00;
                                data[14]=0x00;
                                downloads.flvTimes.TagType=0x12;
                                return data;
                            }
                        case 0x08:
                            {
                                
                                Len=BitConverter.ToUInt32(new byte[] { data[7], data[6], data[5], 0x00 }, 0);
                                byte[] TD = new byte[] { data[11], data[8] , data[9] , data[10] };
                                Array.Reverse(TD);
                                uint Time=BitConverter.ToUInt32(TD, 0);
                                if (Time>0&&downloads.flvTimes.ErrorAudioTimes==0)
                                {
                                    downloads.flvTimes.ErrorAudioTimes=Time;
                                }
                                data[4]=0x08;
                                byte[] c = BitConverter.GetBytes(Time-downloads.flvTimes.ErrorAudioTimes);
                                data[8]=c[2];
                                data[9]=c[1];
                                data[10]=c[0];
                                data[11]=c[3];
                                //PakeTime = BitConverter.ToUInt32(new byte[] { data[10], data[9], data[8], data[11] }, 0);

                                //if (PakeTime - FlvAudioFPS > 500)
                                //{
                                //    data = null;
                                //    ErrorAudioFPS = PakeTime - FlvAudioFPS;
                                //    FlvAudioFPS = PakeTime;
                                //    return data;
                                //}

                                downloads.flvTimes.TagType=0x08;
                                if (downloads.flvTimes.FlvVideoTagCount==0)
                                {
                                    downloads.FlvScriptTag.FistAbody=new byte[data.Length];
                                    downloads.FlvScriptTag.FistAbody= data;
                                }
                                downloads.flvTimes.FlvAudioTagCount++;
                                return data;
                            }
                        case 0x09:
                            {
                                Len=BitConverter.ToUInt32(new byte[] { data[7], data[6], data[5], 0x00 }, 0);
                                byte[] TD = new byte[] { data[11], data[8], data[9], data[10] };
                                Array.Reverse(TD);
                                uint Time = BitConverter.ToUInt32(TD, 0);
                                if (Time>0&&downloads.flvTimes.ErrorVideoTimes==0)
                                {
                                    downloads.flvTimes.ErrorVideoTimes=Time;
                                }
                                data[4]=0x09;
                                byte[] c = BitConverter.GetBytes(Time-downloads.flvTimes.ErrorVideoTimes);
                                data[8]=c[2];
                                data[9]=c[1];
                                data[10]=c[0];
                                data[11]=c[3];
                                downloads.RecordingDuration = BitConverter.ToUInt32(new byte[] { data[10], data[9], data[8], data[11] }, 0);
                                //PakeTime = BitConverter.ToUInt32(new byte[] { data[10], data[9], data[8], data[11] }, 0);
                                //if (PakeTime - FlvVideoFPS > 500)
                                //{
                                //    data = null;
                                //    ErrorVideoFPS = PakeTime - FlvVideoFPS;
                                //    FlvVideoFPS = PakeTime;
                                //    return data;
                                //}

                                downloads.flvTimes.TagType=0x09;
                                if(downloads.flvTimes.FlvVideoTagCount==0)
                                {
                                    downloads.FlvScriptTag.FistVbody=new byte[data.Length];
                                    downloads.FlvScriptTag.FistVbody= data;
                                }
                                
                                downloads.flvTimes.FlvVideoTagCount++;
                                return data;
                            }
                        default:
                            {
                                Len=15;
                                return data;
                            }
                    }             
                }
                else
                {     
                    downloads.flvTimes.IsTagHeader=!downloads.flvTimes.IsTagHeader;
                    if (downloads.flvTimes.FlvTotalTagCount<2)
                    {
                        downloads.FlvScriptTag.TagaData=data;
                    }
                    else
                    {
                        switch(downloads.flvTimes.TagType)
                        {
                            case 0x08:
                                if (downloads.flvTimes.FlvAudioTagCount==1)
                                {
                                    byte[] TEST = new byte[downloads.FlvScriptTag.FistAbody.Length+data.Length];
                                    downloads.FlvScriptTag.FistAbody.CopyTo(TEST,0);
                                    data.CopyTo(TEST, downloads.FlvScriptTag.FistAbody.Length);
                                    downloads.FlvScriptTag.FistAbody=TEST;
                                }
                                downloads.flvTimes.FlvAudioTagCount++;
                                break;
                            case 0x09:
                                if (downloads.flvTimes.FlvVideoTagCount==1)
                                {
                                    byte[] TEST = new byte[downloads.FlvScriptTag.FistVbody.Length+data.Length];
                                    downloads.FlvScriptTag.FistVbody.CopyTo(TEST, 0);
                                    data.CopyTo(TEST, downloads.FlvScriptTag.FistVbody.Length);
                                    downloads.FlvScriptTag.FistVbody=TEST;
                                }
                                downloads.flvTimes.FlvVideoTagCount++;
                                break;
                        }
                        
                    }
                    Len=15;
                    return data;
                }
            }
            else
            {
                downloads.FlvHeader.Header=new byte[data.Length];
                downloads.FlvHeader.Header=data;
                downloads.FlvHeader.Signature[0]=data[0];
                downloads.FlvHeader.Signature[1]=data[1];
                downloads.FlvHeader.Signature[2]=data[2];
                downloads.FlvHeader.Version=data[3];
                downloads.FlvHeader.TypeFlagsAudio = GetBitValue(data[4], 2);
                downloads.FlvHeader.TypeFlagsVideo = GetBitValue(data[4], 0);
                downloads.FlvHeader.Type=data[4];
                downloads.FlvHeader.FlvHeaderOffset[0]=data[5];
                downloads.FlvHeader.FlvHeaderOffset[1]=data[6];
                downloads.FlvHeader.FlvHeaderOffset[2]=data[7];
                downloads.FlvHeader.FlvHeaderOffset[3]=data[8];
                Len=15;
                return data;
            }
        }
        private static bool GetBitValue(byte value, byte bit)
        {
            return (value & (byte)Math.Pow(2, bit)) > 0 ? true : false;
        }
    }
}
