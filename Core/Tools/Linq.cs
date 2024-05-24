using Core.LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.RuntimeObject.Download.Basics;

namespace Core.Tools
{
    public class Linq
    {
        internal static RuntimeObject.Download.Basics.HostClass SerializedM3U8(string str, ref HostClass hostClass)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                    return hostClass;
                int Ver = 0;
                int TimeOffSet = 0;
                long MediaSequence = 0;
                double Targetduration = 0;
                string[] list = str.Split('\n');
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i].Contains("#EXT-X-VERSION"))
                    {
                        int.TryParse(list[i].Split(':')[1], out Ver);
                        hostClass.eXTM3U.Version = Ver;
                    }
                    else if (list[i].Contains("#EXT-X-START:TIME-OFFSET"))
                    {
                        int.TryParse(list[i].Split('=')[1], out TimeOffSet);
                        hostClass.eXTM3U.TimeOffSet = TimeOffSet;
                    }
                    else if (list[i].Contains("#EXT-X-MEDIA-SEQUENCE"))
                    {
                        long.TryParse(list[i].Split(':')[1], out MediaSequence);
                        hostClass.eXTM3U.MediaSequence = MediaSequence;
                    }
                    else if (list[i].Contains("#EXT-X-TARGETDURATION"))
                    {
                        double.TryParse(list[i].Split(':')[1], out Targetduration);
                        hostClass.eXTM3U.Targetduration = Targetduration;
                    }
                    else if (list[i].Contains("#EXT-X-MAP:URI"))
                    {
                        hostClass.eXTM3U.Map_URI = list[i].Split('=')[1].Replace("\"", "");
                    }
                    else if (list[i].Contains("#EXTINF"))
                    {
                        double.TryParse(list[i].Split(':')[1].Split(',')[0], out double Duration);
                        hostClass.eXTM3U.eXTINFs.Add(new()
                        {
                            Duration = Duration,
                            Aux = list[i - 1].Split(':')[1],
                            FileName = list[i + 1].Split('.')[0],
                            ExtensionName = list[i + 1].Split(".")[1]
                        });
                    }
                    else if (list[i].Contains("#EXT-X-ENDLIST"))
                    {
                        hostClass.eXTM3U.IsEND = true;
                    }
                    else if (list[i].Contains("#EXT-X-STREAM-INF"))
                    {
                        hostClass.SteramInfo = list[i + 1];
                    }
                }
                return hostClass;
            }
            catch (Exception e)
            {
                Log.Error(nameof(SerializedM3U8), $"M3U8解析发生致命错误，错误的解析原文：{str}", e, true);
                return hostClass;
            }
        }

        /// <summary>
        /// 文件大小单位转换
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string ConversionSize(double size, ConversionSizeType conversionSizeType = ConversionSizeType.String)
        {

            if (conversionSizeType == ConversionSizeType.BitRate)
            {
                size = size * 8.0;
            }
            if (size < 0)
            {
                return "未知";
            }
            if (size <= 1024)
            {
                switch (conversionSizeType)
                {
                    case ConversionSizeType.String:
                        return size.ToString("F2") + "B";
                    case ConversionSizeType.BitRate:
                        return size.ToString("F2") + "bps";
                    case ConversionSizeType.DownloadSpe:
                        return size.ToString("F2") + "B/s";
                    default:
                        return size.ToString("F2") + "B";
                }
            }
            if (size <= 1048576)
            {
                switch (conversionSizeType)
                {
                    case ConversionSizeType.String:
                        return (size / 1024.0).ToString("F2") + "KB";
                    case ConversionSizeType.BitRate:
                        return (size / 1024.0).ToString("F2") + "Kbps";
                    case ConversionSizeType.DownloadSpe:
                        return (size / 1024.0).ToString("F2") + "KB/s";
                    default:
                        return (size / 1024.0).ToString("F2") + "KB";
                }
            }
            if (size <= 1073741824)
            {
                switch (conversionSizeType)
                {
                    case ConversionSizeType.String:
                        return (size / 1048576.0).ToString("F2") + "MB";
                    case ConversionSizeType.BitRate:
                        return (size / 1048576.0).ToString("F2") + "Mbps";
                    case ConversionSizeType.DownloadSpe:
                        return (size / 1048576.0).ToString("F2") + "MB/s";
                    default:
                        return (size / 1048576.0).ToString("F2") + "MB";
                }
            }
            if (size <= 1099511627776)
            {
                switch (conversionSizeType)
                {
                    case ConversionSizeType.String:
                        return (size / 1073741824.0).ToString("F2") + "GB";
                    case ConversionSizeType.BitRate:
                        return (size / 1073741824.0).ToString("F2") + "Gbps";
                    case ConversionSizeType.DownloadSpe:
                        return (size / 1073741824.0).ToString("F2") + "GB/s";
                    default:
                        return (size / 1073741824.0).ToString("F2") + "GB";
                }
            }
            switch (conversionSizeType)
            {
                case ConversionSizeType.String:
                    return (size / 1099511627776.0).ToString("F2") + "TB";
                case ConversionSizeType.BitRate:
                    return (size / 1099511627776.0).ToString("F2") + "Tbps";
                case ConversionSizeType.DownloadSpe:
                    return (size / 1099511627776.0).ToString("F2") + "TB/s";
                default:
                    return (size / 1099511627776.0).ToString("F2") + "TB";
            }
        }
        public enum ConversionSizeType
        {
            /// <summary>
            /// 例：1MB
            /// </summary>
            String,
            /// <summary>
            /// 例：1Mbps
            /// </summary>
            BitRate,
            /// <summary>
            /// 例 1MB/s
            /// </summary>
            DownloadSpe,
        }
    }
}
