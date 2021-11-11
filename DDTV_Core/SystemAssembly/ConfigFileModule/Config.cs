using DDTV_Core.SystemAssembly.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.ConfigFileModule
{
    /// <summary>
    /// 配置文件操作方法类
    /// </summary>
    public class Config
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="Key">配置名</param>
        /// <param name="state">是否成功</param>
        /// <param name="Value">配置值，如果失败则是失败原因</param>
        /// <param name="Group">可选：值所对应的组别</param>
        public static void GetValue(string Key, out bool state, out string Value, string Group = "")
        {
            if (string.IsNullOrEmpty(Group))
            {
                foreach (var key in ConfigClass.config.groups)
                {
                    foreach (var item in key.datas)
                    {
                        if (item.Key==Key)
                        {
                            if(item.Enabled)
                            {
                                state=true;
                                Value=item.Value;
                            }
                            else
                            {
                                state = false;
                                Value="该配置在配置文件中未使能";
                            }
                            return;
                        }
                    }
                }
                state=false;
                Value="配置文件中没有该Key对应的键值对";
            }
            else
            {
                foreach (var ConfigGroup in ConfigClass.config.groups)
                {
                    if (ConfigGroup.GroupName==Group)
                    {
                        foreach (var KeyValue in ConfigGroup.datas)
                        {
                            if (KeyValue.Key==Key)
                            {
                                if (KeyValue.Enabled)
                                {
                                    state=true;
                                    Value=KeyValue.Value;
                                }
                                else
                                {
                                    state = false;
                                    Value="该配置在配置文件中未使能";
                                }
                                return;
                            }
                        }
                    }
                }
                state=false;
                Value="配置文件中有该Group，但没有该Key对应的键值对";
            }
            return;
        }
        /// <summary>
        /// 添加\修改配置
        /// </summary>
        /// <param name="Key">配置名</param>
        /// <param name="Value">配置值</param>
        /// <param name="Group">所属组别</param>
        /// <returns></returns>
        public static bool SetValue(string Key, string Value, string Group,out string Message)
        {
            foreach (var ConfigGroup in ConfigClass.config.groups)
            {
                if (ConfigGroup.GroupName==Group)
                {
                    foreach (var data in ConfigGroup.datas)
                    {
                        
                        if (data.Key==Key)
                        {
                            if (data.Enabled)
                            {
                                data.Value=Value;
                                Message="修改完成";
                                return true;
                            }
                            else
                            {
                                Message="未修改，因为该配置在配置文件中未使能";
                                return false;
                            }
                        }
                    }
                    ConfigGroup.datas.Add(new ConfigClass.Config.Group.Data()
                    {
                        Key=Key,
                        Value=Value,
                        Enabled=true
                    });
                    Message="增加配置完成";
                    return false;
                }
            }
            ConfigClass.Config.Group group = new()
            {
                GroupName=Group,
                datas=new List<ConfigClass.Config.Group.Data>()
            };
            group.datas.Add(new ConfigClass.Config.Group.Data()
            {
                Key= Key,
                Value=Value,
                Enabled=true
            });
            Message="增加配置完成";
            ConfigClass.config.groups.Add(group);
            return false;
        }
        /// <summary>
        /// 写入配置文件
        /// </summary>
        public static void WriteConfigFile(string ConfigFile= "./DDTV_Config.ini")
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
                Log.Log.AddLog(nameof(Config), LogClass.LogType.Info, "配置文件不存在，读取失败");
                return;
            }
            ConfigClass.config=new ConfigClass.Config();

            string[] ConfigLine = File.ReadAllLines(ConfigFile);
            string Group = "default";
            foreach (var ConfigText in ConfigLine)
            {
                if(!string.IsNullOrEmpty(ConfigText)&&ConfigText.Length>2)
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
