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
    }
}
