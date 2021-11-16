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
                internal Key Key { set; get; }
                internal Group Group { set; get; } = Group.Default;
                internal string Value { set; get; } = "";
                internal bool Enabled { set; get; } = false;//是否有效
                
            }
        }
        internal static Dictionary<int, string> ConfigType = new Dictionary<int, string>();
        public enum Group
        {
            Default,
            Core,
        }
        public enum Key
        {
            /// <summary>
            /// 房间配置文件
            /// </summary>
            RoomListConfig,

        }
    }
}
