using DDTV_Core.SystemAssembly.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.ConfigModule
{
    /// <summary>
    /// 配置文件操作方法类
    /// </summary>
    public class CoreConfigFile
    {
        /// <summary>
        /// 写入配置文件
        /// </summary>
        public static void WriteConfigFile(string ConfigFile = "./DDTV_Config.ini")
        {
            if (File.Exists(ConfigFile))
            {
                File.Delete(ConfigFile);
            }
            List<ConfigTmp> configTmp = new List<ConfigTmp>();

            foreach (var datas in CoreConfigClass.config.datas)
            {
                bool IsOK = false;
                foreach (var item in configTmp)
                {
                    if (item.G==datas.Group)
                    {
                        item.datas.Add(datas);
                        IsOK=true;
                        break;
                    }
                }
                if (!IsOK)
                {
                    CoreConfigClass.Config.Data data = new CoreConfigClass.Config.Data()
                    {
                        Enabled=datas.Enabled,
                        Group=datas.Group,
                        Key=datas.Key,
                        Value =datas.Value,
                    };
                    ConfigTmp configTmp1 = new ConfigTmp() { G=datas.Group, datas=new List<CoreConfigClass.Config.Data>() };
                    configTmp1.datas.Add(datas);
                    configTmp.Add(configTmp1);
                }
            }
            using StreamWriter streamWriter = File.AppendText(ConfigFile);
            foreach (var item1 in configTmp)
            {
                streamWriter.WriteLine($"[{item1.G}]");
                foreach (var item2 in item1.datas)
                {
                    streamWriter.WriteLine((item2.Enabled ? "" : "# ")+$"{item2.Key}={item2.Value}");
                }
            }
        }
        /// <summary>
        /// 读取配置文件
        /// </summary>
        public static void ReadConfigFile(string ConfigFile = "./DDTV_Config.ini")
        {
            if (!File.Exists(ConfigFile))
            {
                Log.Log.AddLog(nameof(CoreConfigFile), LogClass.LogType.Warn, "配置文件不存在，读取失败");
                File.WriteAllText(ConfigFile, "");
            }
            CoreConfigClass.config=new CoreConfigClass.Config();
            string[] ConfigLine = File.ReadAllLines(ConfigFile);
            CoreConfigClass.Group Group = CoreConfigClass.Group.Default;

            foreach (var ConfigText in ConfigLine)
            {
                if (string.IsNullOrEmpty(ConfigText)) continue;
                if (ConfigText[..1]!="[")
                {
                    if (ConfigText.Split('=').Length==2&&ConfigText.Split('=')[0].Length>0&&ConfigText.Split('=')[1].Length>0)
                    {
                        bool Enabled = true;
                        if (ConfigText[..1]=="#")
                        {
                            Enabled=false;
                            ConfigText.Trim('#');
                        }
                        if(Enum.TryParse(typeof(CoreConfigClass.Key), ConfigText.Split('=')[0].Trim('#').Trim(' '), out var Key))
                        {
                            CoreConfigClass.config.datas.Add(new CoreConfigClass.Config.Data()
                            {
                                Enabled = Enabled,
                                Key = (CoreConfigClass.Key)Enum.Parse(typeof(CoreConfigClass.Key), ConfigText.Split('=')[0].Trim('#').Trim(' ')),
                                Value = ConfigText.Split('=')[1].Trim(' '),
                                Group = Group
                            });
                        }
                       
                    }
                }
                else
                {
                    Group=(CoreConfigClass.Group)Enum.Parse(typeof(CoreConfigClass.Group), ConfigText.Substring(1, ConfigText.Length-2));
                }
            }
            Log.Log.AddLog(nameof(CoreConfigFile), Log.LogClass.LogType.Info, $"读取配置文件成功");
        }

        internal class ConfigTmp : CoreConfigClass.Config
        {
            internal CoreConfigClass.Group G { set; get; }
        }
    }
}
