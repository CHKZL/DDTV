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
        private static DateTime SaveTime = DateTime.MinValue;
        /// <summary>
        /// 写入配置文件
        /// </summary>
        public static void WriteConfigFile(bool IsUser = false,string ConfigFile = "./DDTV_Config.ini")
        {
            if (InitDDTV_Core.WhetherInitializationIsComplet)
            {
                TimeSpan ts = DateTime.Now - SaveTime;  //计算时间差
                if (ts.TotalSeconds > 5 || IsUser)
                {
                    SaveTime = DateTime.Now;
                    List<ConfigTmp> configTmp = new List<ConfigTmp>();

                    foreach (var datas in CoreConfigClass.config.datas)
                    {
                        bool IsOK = false;
                        foreach (var item in configTmp)
                        {
                            if (item.G == datas.Group)
                            {
                                item.datas.Add(datas);
                                IsOK = true;
                                break;
                            }
                        }
                        if (!IsOK)
                        {
                            CoreConfigClass.Config.Data data = new CoreConfigClass.Config.Data()
                            {
                                Enabled = datas.Enabled,
                                Group = datas.Group,
                                Key = datas.Key,
                                KeyName = datas.KeyName,
                                Value = datas.Value,
                            };
                            ConfigTmp configTmp1 = new ConfigTmp() { G = datas.Group, datas = new List<CoreConfigClass.Config.Data>() };
                            configTmp1.datas.Add(datas);
                            configTmp.Add(configTmp1);
                        }
                    }
                    if (!Tool.FileOperation.SpaceWillBeEnough(ConfigFile))
                    {
                        Log.Log.AddLog(nameof(CoreConfigFile), LogClass.LogType.Error, "配置文件储存路径所属盘符剩余空间不足10MB！放弃更新储存配置文件！", true, null, true);
                        return;
                    }

                    //using FileStream fileStream = File.Create(ConfigFile);
                    string ConfigText = string.Empty;

                    foreach (var item1 in configTmp)
                    {
                        ConfigText += $"[{item1.G}]\r\n";
                        //fileStream.Write(Encoding.UTF8.GetBytes($"[{item1.G}]\r\n"));
                        foreach (var item2 in item1.datas)
                        {
                            ConfigText += (item2.Enabled ? "" : "# ") + $"{item2.Key}={item2.Value}\r\n";
                            //fileStream.Write(Encoding.UTF8.GetBytes((item2.Enabled ? "" : "# ") + $"{item2.Key}={item2.Value}\r\n"));
                        }
                    }
                    File.WriteAllText(ConfigFile, ConfigText);
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
                                KeyName = Key.ToString(),
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
