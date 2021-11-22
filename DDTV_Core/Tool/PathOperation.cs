using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.Tool
{
    internal class PathOperation
    {
        public static string CreateAll(string Path)
        {
            Directory.CreateDirectory(Path);
            return Path;
        }
    }
}
