using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.ConfigModule
{
    internal class ConfigClass
    {
        internal static Config config = new();
        internal class Config
        {
            internal List<Group> groups = new();
            internal class Group
            {
                internal string GroupName { set; get; }
                internal List<Data> datas = new();
                internal class Data
                {
                    internal string Key { set; get; }
                    internal string Value { set; get; } = "";
                    internal bool Enabled  { set; get; } = false;//是否有效
                }
            }
        }
        public enum ConfigType
        {

        }
    }
}
