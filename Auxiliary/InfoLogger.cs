using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
   public class InfoLogger
    {
        public static void SendInfo(string A, string B, string C)
        {
            Console.WriteLine(A + " " + B + " " + C);
            string path = "./Debug.txt";
            FileStream mystream = new FileStream(path, FileMode.OpenOrCreate);
            StreamWriter myWrite = new StreamWriter(mystream);
            myWrite.WriteLine(A + " " + B + " " + C);
            myWrite.Close();
            mystream.Close();
        }
    }
}
