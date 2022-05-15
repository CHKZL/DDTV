using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.Tool.FlvModule
{
    public class FlvMerge
    {
        #region Public Methods

        /// <summary>
        /// 合并函数
        /// </summary>
        /// <param name="files">   文件字节数组</param>
        /// <param name="savepath">合成后文件保存位置</param>
        public static void Merge(List<string> FILE, string savepath)
        {
            List<byte[]> files = null;

            foreach (string file in FILE)
            {
                files.Add(File.ReadAllBytes(file));
            }

            int t = 0;//总时间戳
            int i = 0;//指针
            int c = 0;//文件序号

            //打开输出流
            FileStream Fs = File.Open(savepath, FileMode.Create, FileAccess.ReadWrite);

            //取第一个文件的头作为公共头
            int headerLength = ToInt32HL(files[0], 5);            //取头部长度
            Fs.Write(files[0], 0, headerLength);
            i += headerLength;

            //取第一个文件的MetaData作为公共MetaData（如果存在的话）
            if (files[0][i + 4] == 0x12)
            {
                int MetaDataSize = ToInt24HL(files[0], i + 5);                //获取MetaData的大小
                Fs.Write(files[0], i, MetaDataSize + 15);
                i += MetaDataSize + 15;
            }

            //合并
            foreach (byte[] b in files)
            {
                int ct = 0;                //分时间戳

                if (c != 0)
                {
                    i = ToInt32HL(b, 5);
                }

                //遍历Tag
                while (i + 15 < b.Length)
                {
                    //获取Tag长度
                    int tagSize = ToInt24HL(b, i + 5) + 15;

                    if (b[i + 4] != 0x12)                        //跳过MetaDataTag
                    {
                        ct = (int)GetTimeStamp(b, i + 8);
                        if (c != 0)         //判断是否第一个文件
                        {
                            Array.Copy(GetTimeStampB(ct + t), 0, b, i + 8, 4);
                        }
                        if (b.Length >= i + tagSize)
                        {
                            Fs.Write(b, i, tagSize);
                        }
                    }

                    i += tagSize;//移动指针
                }

                i = 0; ;//初始化指针
                t += ct;//更新总时间戳
                c++;//更新文件序号
            }

            //关闭输出流
            Fs.Close();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// 获取FLV Tag的时间戳
        /// </summary>
        /// <param name="data">  字节数组</param>
        /// <param name="offset">偏移，默认为0</param>
        /// <returns>时间戳</returns>
        private static uint GetTimeStamp(byte[] data, int offset = 0)
        {
            return (uint)((data[offset + 2] & 0xFF) | ((data[offset + 1] & 0xFF) << 8) | ((data[offset] & 0xFF) << 16) | ((data[offset + 3] & 0xFF) << 24));
        }

        /// <summary>
        /// 获取FLV Tag时间戳的字节形式
        /// </summary>
        /// <param name="TimeStamp">时间戳</param>
        /// <returns>字节数组</returns>
        private static byte[] GetTimeStampB(int TimeStamp)
        {
            byte[] b = new byte[4];
            b = BitConverter.GetBytes(TimeStamp);
            if (BitConverter.IsLittleEndian)
            {
                return new byte[4] { b[2], b[1], b[0], b[3] };
            }
            else
            {
                return new byte[4] { b[1], b[2], b[3], b[0] };
            }
        }

        /// <summary>
        /// 取24位高位有符号整型
        /// </summary>
        /// <param name="data">  字节数组</param>
        /// <param name="offset">偏移</param>
        /// <returns>整型</returns>
        private static int ToInt24HL(byte[] data, int offset = 0)
        {
            return (int)(data[offset + 2] & 0xFF) | ((data[offset + 1] & 0xFF) << 8) | ((data[offset] & 0xFF) << 16);
        }

        /// <summary>
        /// 取32位高位有符号整型
        /// </summary>
        /// <param name="data">  字节数组</param>
        /// <param name="offset">偏移</param>
        /// <returns>整型</returns>
        private static int ToInt32HL(byte[] data, int offset = 0)
        {
            return (int)((data[offset + 3] & 0xFF) | ((data[offset + 2] & 0xFF) << 8) | ((data[offset + 1] & 0xFF) << 16) | ((data[offset] & 0xFF) << 24));
        }

        /// <summary>
        /// 取32位高位无符号整型
        /// </summary>
        /// <param name="data">  字节数组</param>
        /// <param name="offset">偏移</param>
        /// <returns>整型</returns>
        private static uint ToUInt32HL(byte[] data, int offset = 0)
        {
            return (uint)((data[offset + 3] & 0xFF) | ((data[offset + 2] & 0xFF) << 8) | ((data[offset + 1] & 0xFF) << 16) | ((data[offset] & 0xFF) << 24));
        }

        #endregion Private Methods
    }
}
