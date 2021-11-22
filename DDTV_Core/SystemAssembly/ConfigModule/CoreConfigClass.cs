using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.ConfigModule
{
    public class CoreConfigClass
    {

        internal static Config config = new();
        internal class Config
        {
            internal List<Data> datas = new();
            internal class Data
            {
                /// <summary>
                /// 配置键
                /// </summary>
                internal Key Key { set; get; }
                /// <summary>
                /// 配置分组
                /// </summary>
                internal Group Group { set; get; } = Group.Default;
                /// <summary>
                /// 配置值
                /// </summary>
                internal string Value { set; get; } = "";
                /// <summary>
                /// 是否有效
                /// </summary>
                internal bool Enabled { set; get; } = false;
                
            }
        }
        internal static Dictionary<int, string> ConfigType = new Dictionary<int, string>();
        /// <summary>
        /// 配置分组
        /// </summary>
        public enum Group
        {
            Default,
            Core,
            Download,
        }
        /// <summary>
        /// 配置键
        /// </summary>
        public enum Key
        {
            /// <summary>
            /// 房间配置文件
            /// </summary>
            RoomListConfig,
            /// <summary>
            /// 默认下载总文件夹
            /// </summary>
            DownloadPath,
            /// <summary>
            /// 默认下载文件夹名字格式
            /// </summary>
            DownloadDirectoryName,
            /// <summary>
            /// 默认下载文件名格式
            /// </summary>
            DownloadFileName,
        }
    }
}
