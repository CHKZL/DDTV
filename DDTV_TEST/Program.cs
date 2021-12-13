using System;
using System.Collections.Generic;
using System.Linq;

namespace DDTV_TEST
{
    public class Program
    {
        public static void Main(string[] args)
        {
            List<string> list = new List<string>()
                {
                    "F:/1.flv",
                    "F:/2.flv",
                    "F:/3.flv"
                };
            DDTV_Core.Tool.FlvModule.Sum.FlvFileSum(list, "test");
        }
    }
}