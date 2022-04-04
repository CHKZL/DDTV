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
        public static bool HideIconState = bool.Parse(CoreConfig.GetValue(CoreConfigClass.Key.HideIconState, "false", CoreConfigClass.Group.GUI));
    }
}
