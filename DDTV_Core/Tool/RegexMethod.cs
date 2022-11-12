using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DDTV_Core.Tool
{
    public class RegexMethod
    {
        public static string GetIPAddress(String str)
        {
            //验证ipv4地址
            string reg = @"(?:(?:(25[0-5])|(2[0-4]\d)|((1\d{2})|([1-9]?\d)))\.){3}(?:(25[0-5])|(2[0-4]\d)|((1\d{2})|([1-9]?\d)))";
            string ip = "";
            Match m = Regex.Match(str, reg);
            if (m.Success)
            {
                ip = m.Value;
            }
            return ip;
        }
    }
}
