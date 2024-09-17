using Core.LogModule;
using Core.RuntimeObject;
using Masuit.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Core.Network.Methods.Room;

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
                    Log.Warn(nameof(DeletePathFile), $"要删除的路径不存在");
                }
                string[] Files = GetAllFiles(targetDir);
                foreach (var item in Files)
                {
                    Delete(item);
                }
            });
        }

        /// <summary>
        /// 获取目标文件夹的所有子文件结构树
        /// </summary>
        /// <param name="targetDirectory"></param>
        /// <returns></returns>
        public static string[] GetAllFiles(string targetDirectory)
        {
            if (Directory.Exists(targetDirectory))
            {
                return Directory.GetFiles(targetDirectory, "*", SearchOption.AllDirectories);
            }
            else
            {
                return new string[0]; // 如果目标文件夹不存在，返回一个空数组
            }
        }

        /// <summary>
        /// 在指定路径中创建所有目录
        /// </summary>
        /// <param name="Path">指定的路径</param>
        /// <returns></returns>
        public static string CreateAll(string path)
        {
            try
            {
                // 检查路径是否存在
                if (Directory.Exists(path))
                {
                    return path;
                }
                else
                {
                    // 尝试创建新的文件夹
                    Directory.CreateDirectory(path);
                    return path;
                }
            }
            catch (Exception)
            {
                // 如果出现异常，比如权限问题，返回空
                return string.Empty;
            }
        }


        private static List<(string File, string Message)> _DelFileList = new();
        private static bool DelState = false;

        /// <summary>
        /// 删除文件队列
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="Message"></param>
        public static void Delete(string Path, string Message = "")
        {
            if (!DelState)
            {
                DelState = true;
                Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            for (int i = 0; i < _DelFileList.Count; i++)
                            {
                                try
                                {
                                    File.Delete(_DelFileList[i].File);
                                    Log.Info(nameof(Delete), $"删除文件:[{_DelFileList[i].Message}]{_DelFileList[i]}");
                                    _DelFileList.RemoveAt(i);
                                    i--;
                                }
                                catch (Exception)
                                {
                                    if (!File.Exists(_DelFileList[i].File))
                                    {
                                        Log.Info(nameof(Delete), $"要删除文件不存在[{_DelFileList[i].Message}]:{_DelFileList[i]}");
                                        _DelFileList.RemoveAt(i);
                                        i--;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error(nameof(Delete), "清理文件出现意外的错误，稍后重试，错误堆栈已写日志", e, true);
                        }
                        Thread.Sleep(1000);
                    }
                });
            }
            _DelFileList.Add((Path, Message));
        }





        /// <summary>
        /// 获取目标路径的文件结构和信息
        /// </summary>
        public class DirectoryHelper
        {
            public static DirectoryNode GetDirectoryStructure(string directoryPath)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
                var directoryStructure = GetDirectoryStructure(directoryInfo, directoryInfo.FullName, directoryPath);
                return directoryStructure;
            }

            private static DirectoryNode GetDirectoryStructure(DirectoryInfo directoryInfo, string rootPath, string directoryPath)
            {
                var directoryNode = new DirectoryNode
                {
                    Name = directoryInfo.Name,
                    Type = "folder",
                    RelativePath = Config.Core_RunConfig._RecordingStorageDirectory + "/" + directoryInfo.FullName.Replace(rootPath, "").Replace("\\", "/"),
                };

                foreach (var file in directoryInfo.GetFiles())
                {
                    directoryNode.Children.Add(new DirectoryNode
                    {
                        Name = file.Name,
                        Type = "file",
                        Size = file.Length,
                        RelativePath = Config.Core_RunConfig._RecordingStorageDirectory + "/" + file.FullName.Replace(rootPath, "").Replace("\\", "/"),
                        Extension = file.Extension
                    });
                }

                foreach (var directory in directoryInfo.GetDirectories())
                {
                    directoryNode.Children.Add(GetDirectoryStructure(directory, rootPath, directoryPath));
                }

                return directoryNode;
            }
        }

        /// <summary>
        /// 复制源文件夹中的所有文件到目标文件夹
        /// </summary>
        /// <param name="sourceDirectory">源文件夹</param>
        /// <param name="destinationDirectory">目标文件夹</param>
        public static void CopyAllFiles(string sourceDirectory, string destinationDirectory)
        {
            // 确保目标文件夹存在
            Directory.CreateDirectory(destinationDirectory);

            // 获取源文件夹中的所有文件
            string[] files = Directory.GetFiles(sourceDirectory);

            foreach (string file in files)
            {
                // 获取文件名
                string fileName = Path.GetFileName(file);

                // 生成目标文件路径
                string destFile = Path.Combine(destinationDirectory, fileName);

                // 复制文件
                File.Copy(file, destFile, true);
            }
        }

        public class DirectoryNode
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public long? Size { get; set; }
            public string RelativePath { get; set; }
            public string Extension { get; set; }
            public List<DirectoryNode> Children { get; set; } = new List<DirectoryNode>();
        }
    }
}
