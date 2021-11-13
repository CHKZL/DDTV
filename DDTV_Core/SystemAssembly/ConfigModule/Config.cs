using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.ConfigModule
{
    public class Config
    {

        /// <summary>
        /// 
        /// </summary>
        public static void HotOverload()
        {

        }
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
                            if (item.Enabled)
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
        public static bool SetValue(string Key, string Value, string Group, out string Message)
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
    }
}
