using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using static DDTV_Core.InitDDTV_Core;

namespace DDTV_Core.SystemAssembly.ConfigModule
{
    public class CoreConfig
    {
        public static bool GUI_FirstStart = bool.Parse(GetValue(CoreConfigClass.Key.GUI_FirstStart, "true", CoreConfigClass.Group.Core));
        public static bool WEB_FirstStart = bool.Parse(GetValue(CoreConfigClass.Key.WEB_FirstStart, "true", CoreConfigClass.Group.Core));
        public static string AccessControlAllowOrigin = CoreConfig.GetValue(CoreConfigClass.Key.AccessControlAllowOrigin, "*", CoreConfigClass.Group.WEB_API);
        public static string AccessControlAllowCredentials = CoreConfig.GetValue(CoreConfigClass.Key.AccessControlAllowCredentials, "true", CoreConfigClass.Group.WEB_API);
        /// <summary>
        /// 初始化配置文件
        /// </summary>
        public static void ConfigInit(SatrtType satrtType)
        {
            //初始化读取配置文件
            CoreConfigFile.ReadConfigFile();
            //初始化读取房间配置
            RoomConfigFile.ReadRoomConfigFile();

          


            switch (satrtType)
            {
                case SatrtType.DDTV_GUI:
                    if(GUI_FirstStart)
                    {

                    }
                    else
                    {
                        Log.Log.AddLog(nameof(CoreConfig), Log.LogClass.LogType.Debug, $"配置文件初始化完成");
                        //初始化哔哩哔哩账号系统
                        BilibiliUserConfig.Init(satrtType);
                        //开始房间巡逻
                        Rooms.UpdateRoomInfo();
                        RoomPatrolModule.RoomPatrol.Init();
                        ClientAID = GetValue(CoreConfigClass.Key.ClientAID, Guid.NewGuid().ToString(), CoreConfigClass.Group.Core) + "-" + BilibiliUserConfig.account.uid;
                    }
                    break;
                default:             
                    Log.Log.AddLog(nameof(CoreConfig), Log.LogClass.LogType.Debug, $"配置文件初始化完成");
                    //初始化哔哩哔哩账号系统
                    BilibiliUserConfig.Init(satrtType);
                    Log.Log.AddLog(nameof(CoreConfig), Log.LogClass.LogType.Debug, $"哔哩哔哩本地User信息加载完成");
                    //开始房间巡逻
                    Rooms.UpdateRoomInfo();
                    RoomPatrolModule.RoomPatrol.Init();
                    ClientAID = GetValue(CoreConfigClass.Key.ClientAID, Guid.NewGuid().ToString(), CoreConfigClass.Group.Core) + "-" + BilibiliUserConfig.account.uid;
                    break;
            }
            
            //开一个线程用于定时自动储存配置
            Task.Run(() => {
                while (true)
                {
                    try
                    {
                        CoreConfigFile.WriteConfigFile();
                    }
                    catch (Exception e)
                    {
                        Log.Log.AddLog(nameof(CoreConfig), Log.LogClass.LogType.Warn, $"配置文件定时储存出现错误", true, e);
                    }
                    Thread.Sleep(10 * 1000);
                }
            });
        }
        public static void InitConfig()
        {
            
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
