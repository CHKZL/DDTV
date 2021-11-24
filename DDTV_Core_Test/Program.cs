using DDTV_Core;
using System;

namespace DDTV_Core_Test
{
    internal class Program
    {
        private static void Main(string[] arg)
        {
            InitDDTV_Core.Core_Init();
            while(true)
            {
                Console.ReadKey();
            }
        }
    }
}
