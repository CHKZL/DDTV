using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;

namespace DDTV_Core.SystemAssembly.ConfigModule
{
    public class CoreConfig
    {

        /// <summary>
        /// 
        /// </summary>
        public static void ConfigInit()
        {
            CoreConfigFile.ReadConfigFile();
            RoomConfigFile.ReadRoomConfigFile();
            Rooms.UpdateRoomInfo();
            //读取完成后最后储存一次配置文件
            //CoreConfigFile.WriteConfigFile();
            Log.Log.AddLog(nameof(CoreConfig), Log.LogClass.LogType.Debug, $"配置文件初始化完成");
            BilibiliUserConfig.Init();
            Task.Run(() => { 
            while(true)
                {
                    try
                    {
                        CoreConfigFile.WriteConfigFile();
                    }
                    catch (Exception)
                    {
                    }
                    Thread.Sleep(30*1000);
                }
            });
        }
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="Key">配置名</param>
        /// <param name="DefaultValue">默认值</param>
        /// <param name="Group">可选：值所对应的组别</param>
        /// <returns>值</returns>
        public static string GetValue(CoreConfigClass.Key Key, string DefaultValue, CoreConfigClass.Group Group = CoreConfigClass.Group.Default)
        {
            string Value = DefaultValue;

            if(CoreConfigClass.config.datas.Count()>0)
            {
                foreach (var item in CoreConfigClass.config.datas)
                {
                    if(item.Key==Key)
                    {
                        if (item.Group==Group&&Group!=CoreConfigClass.Group.Default)
                        {
                            if(item.Enabled)
                            {
                                Value=item.Value;
                                Log.Log.AddLog(nameof(CoreConfig), Log.LogClass.LogType.Debug, $"获取配置键为[{Key}]的值成功，返回值[{Value}]");
                                return Value;
                            }
                            else
                            {
                                Log.Log.AddLog(nameof(CoreConfig), Log.LogClass.LogType.Debug, $"获取配置键为[{Key}]的值失败，因为该值当前为[注释属性]返回值默认值[{Value}]");
                                return Value;
                            }
                        }   
                    }
                }
            }
            if (Group!=CoreConfigClass.Group.Default)
            {
                SetValue(Key, Value, Group);
                Log.Log.AddLog(nameof(CoreConfig), Log.LogClass.LogType.Debug, $"获取配置键为[{Key}]的值失败，未找到该值，已经把默认值[{Value}]增加到配置文件");
            }
            return Value;
        }
        /// <summary>
        /// 添加\修改配置
        /// </summary>
        /// <param name="Key">配置名</param>
        /// <param name="Value">配置值</param>
        /// <param name="Group">所属组别</param>
        /// <returns></returns>
        public static bool SetValue(CoreConfigClass.Key Key, string Value, CoreConfigClass.Group Group, bool IsEnabled = true)
        {
            foreach (var item in CoreConfigClass.config.datas)
            {
                if(item.Key==Key&& item.Group==Group)
                {
                    item.Value=Value;
                    return true;
                }
            }
            CoreConfigClass.config.datas.Add(new CoreConfigClass.Config.Data() {
                Key=Key,
                Group=Group,
                Value=Value,
                Enabled=IsEnabled
            });
            Log.Log.AddLog(nameof(CoreConfig), Log.LogClass.LogType.Debug, $"为配置文件增加[{Group}]组下[{Key}]的值成功，返回值[{Value}]");
            return false;
        }
    }
}
