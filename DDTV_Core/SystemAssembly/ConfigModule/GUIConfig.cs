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
    }
}
