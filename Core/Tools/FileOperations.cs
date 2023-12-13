using Core.LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tools
{
    public class FileOperations
    {
        /// <summary>
        /// 异步循环递归删除文件
        /// </summary>
        /// <param name="targetDir"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static async Task DeletePathFile(string targetDir)
        {
            await Task.Run(() =>
            {
                DirectoryInfo dir = new DirectoryInfo(targetDir);

                if (!dir.Exists)
                {
                    Log.Warn(nameof(DeletePathFile),$"要删除的路径不存在");
                }
                dir.Delete(true);
            });
        }
    }
}
