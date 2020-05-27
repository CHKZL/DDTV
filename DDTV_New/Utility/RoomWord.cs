using Auxiliary;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DDTV_New.Utility
{
    public abstract class 直播间文字 : ReactiveObject
    {
        public SolidColorBrush 笔刷 { get; set; }
        public int 大小 { get; set; }
        
        public 直播间文字() : this(null) { }

        public 直播间文字(string 颜色字符串) : this(颜色字符串, 24) { }

        public 直播间文字(string 颜色字符串, int 大小)
        {
            this.大小 = 大小;
            if (颜色字符串 == null)
            {
                笔刷 = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
            }
            else
            {
                笔刷 = new SolidColorBrush(Color.FromArgb(0xFF,
                    Convert.ToByte(颜色字符串.Split(',')[1], 16),
                    Convert.ToByte(颜色字符串.Split(',')[2], 16),
                    Convert.ToByte(颜色字符串.Split(',')[3], 16)));
            }
        }

        public 直播间文字(SolidColorBrush 笔刷, int 大小)
        {
            this.笔刷 = 笔刷;
            this.大小 = 大小;
        }

        /// <summary>
        /// 获取用于存储设置信息的字符串
        /// </summary>
        /// <returns></returns>
        public string 颜色字符串
        {
            get => 笔刷.Color.A.ToString("X2") + ","
                + 笔刷.Color.R.ToString("X2") + ","
                + 笔刷.Color.G.ToString("X2") + ","
                + 笔刷.Color.B.ToString("X2");
        }

        public abstract void 保存至配置文件();
    }

    public class 直播间弹幕 : 直播间文字
    {
        public 直播间弹幕() : base() { }

        public 直播间弹幕(string 颜色字符串) : base(颜色字符串) { }

        public 直播间弹幕(string 颜色字符串, int 大小) : base(颜色字符串, 大小) { }

        public 直播间弹幕(SolidColorBrush 笔刷, int 大小) : base(笔刷, 大小) { }

        public override void 保存至配置文件()
        {
            MMPU.默认弹幕颜色 = 颜色字符串;
            MMPU.setFiles("DanMuColor", 颜色字符串);
            MMPU.默认弹幕大小 = 大小;
            MMPU.setFiles("DanMuSize", 大小.ToString());
        }
    }

    public class 直播间字幕 : 直播间文字
    {
        public 直播间字幕() : base() { }

        public 直播间字幕(string 颜色字符串) : base(颜色字符串) { }

        public 直播间字幕(string 颜色字符串, int 大小) : base(颜色字符串, 大小) { }

        public 直播间字幕(SolidColorBrush 笔刷, int 大小) : base(笔刷, 大小) { }

        public override void 保存至配置文件()
        {
            MMPU.默认字幕颜色 = 颜色字符串;
            MMPU.setFiles("ZiMuColor", 颜色字符串);
            MMPU.默认字幕大小 = 大小;
            MMPU.setFiles("ZiMuSize", 大小.ToString());
        }
    }
}
