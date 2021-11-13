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
    public class ConfigFile
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
            StreamWriter streamWriter = File.AppendText(ConfigFile);
            foreach (var Group in ConfigClass.config.groups)
            {
                streamWriter.WriteLine($"[{Group.GroupName}]");
                foreach (var Data in Group.datas)
                {
                    streamWriter.WriteLine((Data.Enabled ? "" : "# ")+$"{Data.Key}={Data.Value}");
                }
            }
            //streamWriter.Flush();
            streamWriter.Close();
        }
        /// <summary>
        /// 读取配置文件
        /// </summary>
        public static void ReadConfigFile(string ConfigFile = "./DDTV_Config.ini")
        {
            if (!File.Exists(ConfigFile))
            {
                Log.Log.AddLog(nameof(ConfigModule.ConfigFile), LogClass.LogType.Info, "配置文件不存在，读取失败");
                return;
            }
            ConfigClass.config=new ConfigClass.Config();


            string[] ConfigLine = File.ReadAllLines(ConfigFile);
            string Group = "default";
            foreach (var ConfigText in ConfigLine)
            {
                if (!string.IsNullOrEmpty(ConfigText)&&ConfigText.Length>2)
                {
                    if (ConfigText[..1]=="[")
                    {
                        Group=ConfigText[1..].Split(']')[0];
                        ConfigClass.config.groups.Add(new ConfigClass.Config.Group() { GroupName=Group });
                    }
                    else
                    {
                        if (ConfigText.Split('=').Length==2&&ConfigText.Split('=')[0].Length>0&&ConfigText.Split('=')[1].Length>0)
                        {
                            if (ConfigClass.config.groups.Count<1)
                            {
                                ConfigClass.config.groups.Add(new ConfigClass.Config.Group() { GroupName="default" });
                            }
                            foreach (var item in ConfigClass.config.groups)
                            {
                                if (item.GroupName==Group)
                                {
                                    bool Enabled = true;
                                    if (ConfigText[..1]=="#")
                                    {
                                        Enabled=false;
                                        ConfigText.Trim('#');
                                    }
                                    item.datas.Add(new ConfigClass.Config.Group.Data()
                                    {
                                        Enabled=Enabled,
                                        Key=ConfigText.Split('=')[0].Trim('#').Trim(' '),
                                        Value=ConfigText.Split('=')[1]
                                    });
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
