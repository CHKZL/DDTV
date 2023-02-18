using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.DownloadModule;
using DDTV_GUI.DDTV_Window;
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
using MessageBox = HandyControl.Controls.MessageBox;

namespace DDTV_GUI.WPFControl
{
    /// <summary>
    /// AddRoomDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SearchListDialog
    {
        EventHandler<EventArgs> SearchEndEvent;
        public SearchListDialog(EventHandler<EventArgs> _)
        {
            InitializeComponent();
            SearchEndEvent = _;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UIDInputBox.Text))
            {
                MessageBox.Show("搜索内容不能为空！");
                return;
            }
            for (int i = 0; i < UpdateInterface.Main.liveList.Count; i++)
            {
                if ((bool)RoomIdRadio.IsChecked)
                {
                    if (int.TryParse(UIDInputBox.Text, out int roomid))
                    {
                        if (UpdateInterface.Main.liveList[i].RoomId == roomid)
                        {
                            MainWindow.SearchIndex = i;
                            SearchEndEvent.Invoke(this, EventArgs.Empty);
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("搜索的房间号输入不正确，请检查输入的内容");
                        return;
                    }
                }
                else if ((bool)UIDRadio.IsChecked)
                {
                    if (int.TryParse(UIDInputBox.Text, out int UID))
                    {
                        if (UpdateInterface.Main.liveList[i].Uid == UID)
                        {
                            MainWindow.SearchIndex = i;
                            SearchEndEvent.Invoke(this, EventArgs.Empty);
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("搜索的UID输入不正确，请检查输入的内容");
                        return;
                    }
                }
                else if ((bool)Uname.IsChecked)
                {
                    if (UpdateInterface.Main.liveList[i].Name.Contains(UIDInputBox.Text))
                    {
                        MainWindow.SearchIndex = i;
                        SearchEndEvent.Invoke(this, EventArgs.Empty);
                        return;
                    }
                }
            }
            MessageBox.Show("对象不存在");
            return;
        }
    }
}
