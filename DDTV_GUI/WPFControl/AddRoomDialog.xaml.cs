using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDTV_GUI.WPFControl
{
    /// <summary>
    /// AddRoomDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddRoomDialog
    {
        public AddRoomDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string _UID = UIDInputBox.Text;
            if (string.IsNullOrEmpty(_UID))
            {
                Growl.Warning($"UID不能为空");
                return;
            }
            if (long.TryParse(_UID, out long UID))
            {
                if (UID <= 0)
                {
                    Growl.Warning($"房间号或UID不能为负数！");
                    return;
                }
                if (int.TryParse(DDTV_Core.SystemAssembly.BilibiliModule.Rooms.Rooms.GetValue(UID, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.room_id), out int RoomId))
                {
                    string Name = DDTV_Core.SystemAssembly.BilibiliModule.Rooms.Rooms.GetValue(UID, DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass.CacheType.uname);
                    DDTV_Core.SystemAssembly.ConfigModule.RoomConfig.AddRoom(UID, RoomId, Name, true);
                    Growl.Success($"添加成功");
                    UIDInputBox.Clear();
                }
                else
                {
                    Growl.Warning($"该UID不存在！");
                    return;
                }
            }
            else
            {
                Growl.Warning($"UID不符合规范！");
                return;
            }
        }
    }
}
