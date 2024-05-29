using Core.RuntimeObject;
using Masuit.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tools
{
    public class KeyCharacterReplacement
    {
        /// <summary>
        /// 检查字符串是否符合文件路径标准,并返回安全字符串
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
        public static string ReplaceKeyword(string Text, DateTime dateTime = default,long uid=-1)
        {
            RoomCardClass roomCardClass = new RoomCardClass();
            if (uid != -1)
            {
                _Room.GetCardForUID(uid, ref roomCardClass);
            }
            else
            {
                roomCardClass = new()
                {
                    RoomId = 1473830,
                    Name="AIChannel",
                    Title = new() { Value = "【自我介绍】大家好！我叫绊(kizuna)爱(ai)" },
                };
            }
            Text = Text
                .Replace("{ROOMID}", roomCardClass.RoomId.ToString())
                .Replace("{YYYY}", dateTime.ToString("yyyy"))
                .Replace("{YY}", dateTime.ToString("yy"))
                .Replace("{MM}", dateTime.ToString("MM"))
                .Replace("{DD}", dateTime.ToString("dd"))
                .Replace("{HH}", dateTime.ToString("HH"))
                .Replace("{mm}", dateTime.ToString("mm"))
                .Replace("{SS}", dateTime.ToString("ss"))
                .Replace("{FFF}", dateTime.ToString("fff"))
                .Replace("{yyyy}", dateTime.ToString("yyyy"))
                .Replace("{yy}", dateTime.ToString("yy"))
                .Replace("{MM}", dateTime.ToString("MM"))
                .Replace("{dd}", dateTime.ToString("dd"))
                .Replace("{HH}", dateTime.ToString("HH"))
                .Replace("{mm}", dateTime.ToString("mm"))
                .Replace("{ss}", dateTime.ToString("ss"))
                .Replace("{fff}", dateTime.ToString("fff"))
                .Replace("{NAME}", roomCardClass.Name)
                .Replace("{DATE}", dateTime.ToString("yyyy_MM_dd"))
                .Replace("{TIME}", dateTime.ToString("HH_mm_ss"))
                .Replace("{TITLE}", roomCardClass.Title.Value);
            return Text;
        }

        /// <summary>
        /// 随机字符串
        /// </summary>
        /// <param name="chars">可选候选字符</param>
        /// <param name="length">生成长度</param>
        /// <returns></returns>
        public static string GetRandomStr(string chars, int length)
        {
            Random random = new Random();
            if (string.IsNullOrEmpty(chars))
            {
                chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghizklmnopqrstuvwxyz0123456789";
            }
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
