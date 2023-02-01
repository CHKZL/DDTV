using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static DDTV_Core.InitDDTV_Core;

namespace DDTV_Core.SystemAssembly.ConfigModule
{
    public class CoreConfig
    {
        public static bool GUI_FirstStart = bool.Parse(GetValue(CoreConfigClass.Key.GUI_FirstStart, "True", CoreConfigClass.Group.Core));
        public static bool WEB_FirstStart = bool.Parse(GetValue(CoreConfigClass.Key.WEB_FirstStart, "True", CoreConfigClass.Group.Core));
        public static string WebHookUrl = GetValue(CoreConfigClass.Key.WebHookUrl, "", CoreConfigClass.Group.Core);
        public static string InstanceAID = GetValue(CoreConfigClass.Key.InstanceAID, Guid.NewGuid().ToString().Substring(0, 10).ToUpper(), CoreConfigClass.Group.Core);
        public static bool ConsoleMonitorMode = bool.Parse(GetValue(CoreConfigClass.Key.ConsoleMonitorMode, "False", CoreConfigClass.Group.Core));
        public static string ReplaceAPI = GetValue(CoreConfigClass.Key.ReplaceAPI, "https://api.live.bilibili.com", CoreConfigClass.Group.Core);
        public static int APIVersion = int.Parse(GetValue(CoreConfigClass.Key.APIVersion, "1", CoreConfigClass.Group.Core));
        public static bool AutoInsallUpdate = bool.Parse(GetValue(CoreConfigClass.Key.AutoInsallUpdate, "True", CoreConfigClass.Group.Core));
        public static bool WhetherToEnableProxy =  bool.Parse(GetValue(CoreConfigClass.Key.WhetherToEnableProxy, "True", CoreConfigClass.Group.Core));
        public static bool MandatoryUseIPv4 = bool.Parse(GetValue(CoreConfigClass.Key.MandatoryUseIPv4, "False", CoreConfigClass.Group.Core));
        public static bool IsBypass_SSL= bool.Parse(GetValue(CoreConfigClass.Key.IsBypass_SSL, "False", CoreConfigClass.Group.Core));
        public static bool IsDev = bool.Parse(GetValue(CoreConfigClass.Key.IsDev, "False", CoreConfigClass.Group.Core));
        public static int DanMuSaveType= int.Parse(GetValue(CoreConfigClass.Key.DanMuSaveType, "2", CoreConfigClass.Group.Core));
        public static string HighRiskWebAPIFixedCheckSign = GetValue(CoreConfigClass.Key.HighRiskWebAPIFixedCheckSign, Guid.NewGuid().ToString(), CoreConfigClass.Group.Core);

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        public static void ConfigInit(SatrtType satrtType)
        {
            //初始化读取配置文件
            CoreConfigFile.ReadConfigFile();
            //初始化读取房间配置
            RoomConfigFile.ReadRoomConfigFile();
            //预生成所有配置项
            BuildConfig();
            switch (satrtType)
            {
                case SatrtType.DDTV_GUI:
                    if (GUI_FirstStart)
                    {

                    }
                    else
                    {
                        Log.Log.AddLog(nameof(CoreConfig), Log.LogClass.LogType.Debug, $"配置文件初始化完成");
                        //初始化哔哩哔哩账号系统
                        BilibiliUserConfig.Init(satrtType);
                        //开始房间巡逻
                        //Rooms.UpdateRoomInfo();
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
                    //Rooms.UpdateRoomInfo();
                    RoomPatrolModule.RoomPatrol.Init();
                    ClientAID = GetValue(CoreConfigClass.Key.ClientAID, Guid.NewGuid().ToString(), CoreConfigClass.Group.Core) + "-" + BilibiliUserConfig.account.uid;
                    break;
            }
            Tool.Dokidoki.DoNotSleepWhileDownloading();
            //开一个线程用于定时自动储存配置
            Task.Run(() =>
            {

                while (true)
                {
                    if (WhetherInitializationIsComplet)
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
                    else
                    {
                        Thread.Sleep(3 * 1000);
                    }
                }
            });
        }
        /// <summary>
        /// 生成所有配置项，防止用户打开配置文件时配置信息缺失
        /// </summary>
        public static void BuildConfig()
        {
            var _RoomFile = DDTV_Core.SystemAssembly.ConfigModule.RoomConfig.RoomFile;
            var _DownloadPath = DDTV_Core.SystemAssembly.DownloadModule.Download.DownloadPath;
            var _TmpPath = DDTV_Core.SystemAssembly.DownloadModule.Download.TmpPath;
            var _DownloadDirectoryName = DDTV_Core.SystemAssembly.DownloadModule.Download.DownloadDirectoryName;
            var _DownloadFileName = DDTV_Core.SystemAssembly.DownloadModule.Download.DownloadFileName;
            var _TranscodParmetrs = DDTV_Core.Tool.TranscodModule.Transcod.TranscodParmetrs;
            var _IsAutoTranscod = DDTV_Core.Tool.TranscodModule.Transcod.IsAutoTranscod;
            var _WEB_API_SSL = DDTV_Core.SystemAssembly.ConfigModule.WebServerConfig.IsSSL;
            var _pfxFileName = DDTV_Core.SystemAssembly.ConfigModule.WebServerConfig.pfxFileName;
            var _pfxPasswordFileName = DDTV_Core.SystemAssembly.ConfigModule.WebServerConfig.pfxPasswordFileName;
            var _GUI_FirstStart = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.GUI_FirstStart;
            var _WEB_FirstStart = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.WEB_FirstStart;
            var _RecQuality = DDTV_Core.SystemAssembly.DownloadModule.Download.RecQuality;
            var _PlayQuality = DDTV_Core.SystemAssembly.ConfigModule.GUIConfig.PlayQuality;
            var _IsRecDanmu = DDTV_Core.SystemAssembly.DownloadModule.Download.IsRecDanmu;
            var _IsRecGift = DDTV_Core.SystemAssembly.DownloadModule.Download.IsRecGift;
            var _IsRecGuard = DDTV_Core.SystemAssembly.DownloadModule.Download.IsRecGuard;
            var _IsRecSC = DDTV_Core.SystemAssembly.DownloadModule.Download.IsRecSC;
            var _IsFlvSplit = DDTV_Core.SystemAssembly.DownloadModule.Download.IsFlvSplit;
            var _FlvSplitSize = DDTV_Core.SystemAssembly.DownloadModule.Download.FlvSplitSize;
            var _WebUserName = DDTV_Core.SystemAssembly.ConfigModule.WebServerConfig.WebUserName;
            var _WebPassword = DDTV_Core.SystemAssembly.ConfigModule.WebServerConfig.WebPassword;
            var _AccessKeyId = DDTV_Core.SystemAssembly.ConfigModule.WebServerConfig.AccessKeyId;
            var _AccessKeySecret = DDTV_Core.SystemAssembly.ConfigModule.WebServerConfig.AccessKeySecret;
            var _ServerAID = DDTV_Core.SystemAssembly.ConfigModule.WebServerConfig.ServerAID;
            var _ServerName = DDTV_Core.SystemAssembly.ConfigModule.WebServerConfig.ServerName;
            var _HideIconState = DDTV_Core.SystemAssembly.ConfigModule.GUIConfig.HideIconState;
            var _DefaultVolume = DDTV_Core.SystemAssembly.ConfigModule.GUIConfig.DefaultVolume;
            var _AccessControlAllowOrigin = DDTV_Core.SystemAssembly.ConfigModule.WebServerConfig.AccessControlAllowOrigin;
            var _AccessControlAllowCredentials = DDTV_Core.SystemAssembly.ConfigModule.WebServerConfig.AccessControlAllowCredentials;
            var _IsDoNotSleepState = DDTV_Core.SystemAssembly.DownloadModule.Download.IsDoNotSleepState;
            var _CookieDomain = DDTV_Core.SystemAssembly.ConfigModule.WebServerConfig.CookieDomain;
            var _Shell = DDTV_Core.SystemAssembly.DownloadModule.Download.Shell;
            var _WebHookUrl = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.WebHookUrl;
            var _InstanceAID = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.InstanceAID;
            var _DDcenterSwitch = DDTV_Core.Tool.DDcenter.DDcenterSwitch;
            var _TranscodingCompleteAutoDeleteFiles = DDTV_Core.Tool.TranscodModule.Transcod.TranscodingCompleteAutoDeleteFiles;
            var _ForceCDNResolution = DDTV_Core.SystemAssembly.BilibiliModule.API.RoomInfo.ForceCDNResolution;
            var _ConsoleMonitorMode = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.ConsoleMonitorMode;
            var _SpaceIsInsufficientWarn = DDTV_Core.Tool.FileOperation.SpaceIsInsufficientWarn;
            var _ReplaceAPI = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.ReplaceAPI;
            var _APIVersion = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.APIVersion;
            var _AutoInsallUpdate = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.AutoInsallUpdate;
            var _DanMuColor = DDTV_Core.SystemAssembly.ConfigModule.GUIConfig.DanMuColor;
            var _SubtitleColor = DDTV_Core.SystemAssembly.ConfigModule.GUIConfig.SubtitleColor;
            var _DanMuFontSize = DDTV_Core.SystemAssembly.ConfigModule.GUIConfig.SubtitleColor;
            var _DanMuFontOpacity = DDTV_Core.SystemAssembly.ConfigModule.GUIConfig.DanMuFontOpacity;
            var _WhetherToEnableProxy = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.WhetherToEnableProxy;
            var _IsDev = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.IsDev;
            var _MandatoryUseIPv4 = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.MandatoryUseIPv4;
            var _IsBypass_SSL = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.IsBypass_SSL;
            var _SCSaveType = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.DanMuSaveType;
            var _IsHls = DDTV_Core.SystemAssembly.DownloadModule.Download.IsHls;
            var _WaitHLSTime = DDTV_Core.SystemAssembly.DownloadModule.Download.WaitHLSTime;
            var _HighRiskWebAPIFixedCheckSign = DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.HighRiskWebAPIFixedCheckSign;
            var _DanmukuFactoryParameter = DDTV_Core.SystemAssembly.ConfigModule.GUIConfig.DanmukuFactoryParameter;
            var _IsXmlToAss = DDTV_Core.SystemAssembly.ConfigModule.GUIConfig.IsXmlToAss;
            var _IsExitReminder =DDTV_Core.SystemAssembly.ConfigModule.GUIConfig.IsExitReminder;
            var _ShowDanMuSwitch = DDTV_Core.SystemAssembly.ConfigModule.GUIConfig.ShowDanMuSwitch;
            var _ShowGiftSwitch = DDTV_Core.SystemAssembly.ConfigModule.GUIConfig.ShowGiftSwitch;
            var _ShowSCSwitch = DDTV_Core.SystemAssembly.ConfigModule.GUIConfig.ShowSCSwitch;
            var _ShowGuardSwitch = DDTV_Core.SystemAssembly.ConfigModule.GUIConfig.ShowGuardSwitch;
            var _RealTimeTitleFileName = DDTV_Core.SystemAssembly.DownloadModule.Download.RealTimeTitleFileName;
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

            if (CoreConfigClass.config.datas.Count() > 0)
            {
                foreach (var item in CoreConfigClass.config.datas)
                {
                    if (item.Key == Key)
                    {
                        if (item.Group == Group && Group != CoreConfigClass.Group.Default)
                        {
                            if (item.Enabled)
                            {
                                Value = item.Value;
                                Log.Log.AddLog(nameof(CoreConfig), Log.LogClass.LogType.Debug, $"获取配置键为[{Key}]的值成功，返回值[{Value}]", false, null, false);
                                return Value;
                            }
                            else
                            {
                                Log.Log.AddLog(nameof(CoreConfig), Log.LogClass.LogType.Debug, $"获取配置键为[{Key}]的值失败，因为该值当前为[注释属性]返回值默认值[{Value}]", false, null, false);
                                return Value;
                            }
                        }
                    }
                }
            }
            if (Group != CoreConfigClass.Group.Default)
            {
                SetValue(Key, Value, Group);
                Log.Log.AddLog(nameof(CoreConfig), Log.LogClass.LogType.Debug, $"获取配置键为[{Key}]的值失败，未找到该值，已经把默认值[{Value}]增加到配置文件", false, null, false);
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
                if (item.Key == Key && item.Group == Group)
                {
                    item.Value = Value;
                    return true;
                }
            }
            CoreConfigClass.config.datas.Add(new CoreConfigClass.Config.Data()
            {
                Key = Key,
                KeyName=Key.ToString(),
                Group = Group,
                Value = Value,
                Enabled = IsEnabled
            });
            Log.Log.AddLog(nameof(CoreConfig), Log.LogClass.LogType.Debug, $"为配置文件增加[{Group}]组下[{Key}]的值成功，返回值[{Value}]");
            return false;
        }
    }
}
