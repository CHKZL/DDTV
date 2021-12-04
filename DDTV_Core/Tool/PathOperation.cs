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
        /// <summary>
        /// 在指定路径中创建所有目录
        /// </summary>
        /// <param name="Path">指定的路径</param>
        /// <returns></returns>
        public static string CreateAll(string Path)
        {
            Directory.CreateDirectory(Path);
            return Path;
        }
    }
}
