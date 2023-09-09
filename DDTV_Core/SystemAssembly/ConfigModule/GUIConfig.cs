using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.ConfigModule
{
    public class GUIConfig
    {
        public static int PlayQuality = int.Parse(CoreConfig.GetValue(CoreConfigClass.Key.PlayQuality, "250", CoreConfigClass.Group.Play));
        public static double DefaultVolume = double.Parse(CoreConfig.GetValue(CoreConfigClass.Key.DefaultVolume, "50", CoreConfigClass.Group.Play));
        public static bool HideIconState = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.HideIconState, "False", CoreConfigClass.Group.GUI));
        public static bool ShowDanMuSwitch = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.ShowDanMuSwitch, "True", CoreConfigClass.Group.GUI));
        public static bool ShowGiftSwitch = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.ShowGiftSwitch, "True", CoreConfigClass.Group.GUI));
        public static bool ShowSCSwitch = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.ShowSCSwitch, "True", CoreConfigClass.Group.GUI));
        public static bool ShowGuardSwitch = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.ShowGuardSwitch, "True", CoreConfigClass.Group.GUI));
        public static bool IsXmlToAss = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.IsXmlToAss, "False", CoreConfigClass.Group.GUI));
        public static string DanmukuFactoryParameter = CoreConfig.GetValue(CoreConfigClass.Key.DanmukuFactoryParameter, "-o {AfterFilePath} -i {BeforeFilePath}", CoreConfigClass.Group.GUI);
        public static bool IsExitReminder = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.IsExitReminder, "True", CoreConfigClass.Group.GUI));
        public static string DanMuColor = CoreConfig.GetValue(CoreConfigClass.Key.DanmuColor, "0xFF,0xFF,0xFF", CoreConfigClass.Group.Play);
        public static string SubtitleColor = CoreConfig.GetValue(CoreConfigClass.Key.SubtitleColor, "0x00,0xFF,0xFF", CoreConfigClass.Group.Play);
        public static int DanMuFontSize = int.Parse(CoreConfig.GetValue(CoreConfigClass.Key.DanMuFontSize, "26", CoreConfigClass.Group.Play));
        public static double DanMuFontOpacity = double.Parse(CoreConfig.GetValue(CoreConfigClass.Key.DanMuFontSize, "1", CoreConfigClass.Group.Play));
        public static double DanMuSpeed = int.Parse(CoreConfig.GetValue(CoreConfigClass.Key.DanMuSpeed, "10", CoreConfigClass.Group.Play));
        public static bool DoesShieldTakeEffect = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.DoesShieldTakeEffect, "True", CoreConfigClass.Group.Play));
        public static bool BarrageSendingDefaultStatus = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.BarrageSendingDefaultStatus, "False", CoreConfigClass.Group.Play));
    }
}
