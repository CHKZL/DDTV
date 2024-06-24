using Core;
using Core.LogModule;
using Core.RuntimeObject;
using Desktop.Models;
using Desktop.Views.Windows;
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
using Wpf.Ui.Controls;

namespace Desktop.Views.Control
{
    /// <summary>
    /// CardControl.xaml 的交互逻辑
    /// </summary>
    public partial class CardControl : UserControl
    {
        public CardControl()
        {
            InitializeComponent();
        }
        private Models.DataCard GetDataCard(object sender)
        {
            var menuItem = (System.Windows.Controls.MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var grid = (Grid)contextMenu.PlacementTarget;
            Models.DataCard dataContext = (Models.DataCard)grid.DataContext;
            return dataContext;
        }

        private void MenuItem_PlayWindow_Click(object sender, RoutedEventArgs e)
        {
            Models.DataCard dataCard = GetDataCard(sender);
            Task.Run(() =>
            {
                if (IsThereHLVPresent(dataCard.Uid))
                {
                    Dispatcher.Invoke(() =>
                    {
                        Windows.VlcPlayWindow vlcPlayWindow = new Windows.VlcPlayWindow(dataCard.Uid);
                        vlcPlayWindow.Show();
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        Windows.WebPlayWindow WebPlayWindow = new Windows.WebPlayWindow(dataCard.Room_Id);
                        WebPlayWindow.Show();
                    });
                }
            });
        }

        /// <summary>
        /// 是否有HLS流
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public bool IsThereHLVPresent(long uid)
        {
            RoomCardClass roomCard = new();
            _Room.GetCardForUID(uid, ref roomCard);
            string url = "";
            if (roomCard != null && (Core.RuntimeObject.Download.HLS.GetHlsAvcUrl(roomCard, out url)) && !string.IsNullOrEmpty(url))
            {

                return true;

            }
            return false;
        }


        private void Border_DoubleClickToOpenPlaybackWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var border = (Border)sender;
                var grid = (Grid)border.Parent;

                Models.DataCard dataCard = (Models.DataCard)grid.DataContext;
                if (IsThereHLVPresent(dataCard.Uid))
                {
                    Windows.VlcPlayWindow vlcPlayWindow = new Windows.VlcPlayWindow(dataCard.Uid);
                    vlcPlayWindow.Show();
                }
                else
                {
                    Windows.WebPlayWindow WebPlayWindow = new Windows.WebPlayWindow(dataCard.Room_Id);
                    WebPlayWindow.Show();
                }

            }
        }


        private void MenuItem_ModifyRoom_AutoRec_Click(object sender, RoutedEventArgs e)
        {
            Models.DataCard dataCard = GetDataCard(sender);
            DataSource.RetrieveData.RoomInfo.ModifyRoomSettings(dataCard.Uid, !dataCard.IsRec, dataCard.IsDanmu, dataCard.IsRemind);
        }

        private void MenuItem_ModifyRoom_Danmu_Click(object sender, RoutedEventArgs e)
        {
            Models.DataCard dataCard = GetDataCard(sender);
            DataSource.RetrieveData.RoomInfo.ModifyRoomSettings(dataCard.Uid, dataCard.IsRec, !dataCard.IsDanmu, dataCard.IsRemind);
        }

        private void MenuItem_ModifyRoom_Remind_Click(object sender, RoutedEventArgs e)
        {
            Models.DataCard dataCard = GetDataCard(sender);
            DataSource.RetrieveData.RoomInfo.ModifyRoomSettings(dataCard.Uid, dataCard.IsRec, dataCard.IsDanmu, !dataCard.IsRemind);
        }

        private void DelRoom_Click(object sender, RoutedEventArgs e)
        {
            Models.DataCard dataCard = GetDataCard(sender);
            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                {"uids", dataCard.Uid.ToString() }
            };
            Task.Run(() =>
            {
                List<(long key, bool State, string Message)> State = NetWork.Post.PostBody<List<(long key, bool State, string Message)>>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/set_rooms/batch_delete_rooms", dic).Result;
                if (State == null)
                {
                    Log.Warn(nameof(DelRoom_Click), "调用Core的API[batch_delete_rooms]删除房间失败，返回的对象为Null，详情请查看Core日志", null, true);
                    Dispatcher.Invoke(() =>
                    {
                        MainWindow.SnackbarService.Show("删除房间失败", $"操作{dataCard.Nickname}({dataCard.Room_Id})时调用Core的API[batch_delete_rooms]删除房间失败", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle20), TimeSpan.FromSeconds(3));
                    });
                    return;
                }
                Dispatcher.Invoke(() =>
                {
                    MainWindow.SnackbarService.Show("删除房间成功", $"{dataCard.Nickname}({dataCard.Room_Id})已从房间配置中删除", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Checkmark20), TimeSpan.FromSeconds(3));
                });

            });

        }

        private void Cancel_Task_Click(object sender, RoutedEventArgs e)
        {
            Models.DataCard dataCard = GetDataCard(sender);
            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                {"uid", dataCard.Uid.ToString() }
            };
            Task.Run(() =>
            {
                bool State = NetWork.Post.PostBody<bool>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/rec_task/cancel_task", dic).Result;
                if (State == false)
                {
                    Log.Warn(nameof(DelRoom_Click), "调用Core的API[cancel_task]取消录制任务失败，详情请查看Core日志", null, true);
                    Dispatcher.Invoke(() =>
                    {
                        MainWindow.SnackbarService.Show("取消录制失败", $"操作{dataCard.Nickname}({dataCard.Room_Id})时调用Core的API[cancel_task]取消录制任务失败", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle20), TimeSpan.FromSeconds(3));
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        MainWindow.SnackbarService.Show("取消录制成功", $"已取消{dataCard.Nickname}({dataCard.Room_Id})的录制任务", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Checkmark20), TimeSpan.FromSeconds(3));
                    });
                }
            });
        }

        private void MenuItem_DanmaOnly_Click(object sender, RoutedEventArgs e)
        {
            Models.DataCard dataCard = GetDataCard(sender);
            RoomCardClass roomCardClass = new();
            _Room.GetCardForUID(dataCard.Uid, ref roomCardClass);
            Windows.DanmaOnlyWindow danmaOnlyWindow = new(roomCardClass);
            danmaOnlyWindow.Show();
        }
    }
}
