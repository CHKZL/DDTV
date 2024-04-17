using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Tools.FileOperations;

namespace Core.RuntimeObject
{
    public class RecordingFiles
    {
        #region Private Properties

        #endregion

        #region Public Method
        /// <summary>
        /// 获取目标路径下所有文件的结构以json格式返回
        /// </summary>
        /// <param name="Path">目标路径</param>
        /// <param name="DirectoryStructureJson">out值，返回的json内容</param>
        /// <returns>路径是否存在</returns>
        public static bool GetDirectoryStructure(string Path, out DirectoryNode DirectoryStructureJson)
        {
            if (Directory.Exists(Path))
            {
                DirectoryStructureJson = DirectoryHelper.GetDirectoryStructure(Path);
                return true;
            }
            else
            {
                DirectoryStructureJson = new();
                return false;
            }
        }
        #endregion

        #region Private Method

        #endregion

        #region Public Class

        #endregion

        #region Public Enmu

        #endregion
    }
}
