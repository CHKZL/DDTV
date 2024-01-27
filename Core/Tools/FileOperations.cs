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
                dir.Delete(true);
            });
        }
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


        private static List<string> _DelFileList = new();
        private static bool DelState = false;
        public static void Delete(string Path)
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
                                    File.Delete(_DelFileList[i]);
                                    _DelFileList.RemoveAt(i);
                                    i--;
                                }
                                catch (Exception)
                                {
                                    if (!File.Exists(_DelFileList[i]))
                                    {
                                        _DelFileList.RemoveAt(i);
                                        i--;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Warn(nameof(Delete), "清理文件出现意外的错误，稍后重试，错误堆栈已写日志", e, true);
                        }
                        Thread.Sleep(1000);
                    }
                });
            }
            _DelFileList.Add(Path);
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
                    Type = "folder"
                };

                foreach (var file in directoryInfo.GetFiles())
                {
                    directoryNode.Children.Add(new DirectoryNode
                    {
                        Name = file.Name,
                        Type = "file",
                        Size = file.Length,
                        RelativePath = Config.Web._RecordingStorageDirectory + "/" + file.FullName.Replace(rootPath, "").Replace("\\", "/"),
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
