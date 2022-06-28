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
            //using FileStream OldfileStream = File.Create(ConfigFile+"new");
            using FileStream fileStream = File.Create(ConfigFile);
            foreach (var item1 in configTmp)
            {
                fileStream.Write(Encoding.UTF8.GetBytes($"[{item1.G}]\r\n"));
                foreach (var item2 in item1.datas)
                {
                    fileStream.Write(Encoding.UTF8.GetBytes((item2.Enabled ? "" : "# ")+$"{item2.Key}={item2.Value}\r\n"));
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
                File.WriteAllText(ConfigFile, "");
                Log.Log.AddLog(nameof(CoreConfigFile), LogClass.LogType.Warn, String.Format("配置文件不存在，新建{0}文件", ConfigFile));
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
        /// <summary>
        /// 初始化配置文件的加载
        /// </summary>
        private static void InitConfigLoad()
        {

        }

        internal class ConfigTmp : CoreConfigClass.Config
        {
            internal CoreConfigClass.Group G { set; get; }
        }
    }
}
