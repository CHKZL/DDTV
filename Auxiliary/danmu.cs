using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    public class danmu
    {
        public static string 返回ASS字幕文件头(string Title,string roomId,string name)
        {
            return "[Script Info]\r\n" +
                "Title: "+ Title + "\r\n" +
                "Original Script: 根据 "+ name + "["+ roomId + "] 的直播间的弹幕信息，由 DDTV录制生成 生成\r\n" +
                "ScriptType: v4.00 +\r\n" +
                "Collisions: Normal\r\n" +
                "PlayResX: 560\r\n" +
                "PlayResY: 420\r\n" +
                "Timer: 10.0000\r\n" +
                "[V4+ Styles]\r\n\r\n" +
                "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding\r\n" +
                "Style: Fix,Microsoft YaHei UI,25,&H66FFFFFF,&H66FFFFFF,&H66000000,&H66000000,1,0,0,0,100,100,0,0,1,2,0,2,20,20,2,0\r\n" +
                "Style: R2L,Microsoft YaHei UI,25,&H66FFFFFF,&H66FFFFFF,&H66000000,&H66000000,1,0,0,0,100,100,0,0,1,2,0,2,20,20,2,0\r\n\r\n" +
                "[Events]\r\n" +
                "Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text";
        }
    }
}
