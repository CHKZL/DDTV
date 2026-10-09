using Core;
using Core.LogModule;
using Core.RuntimeObject;
using Desktop.Models;
using Desktop.Views.Windows;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Controls;
using Log = Core.LogModule.Log;

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
            var contextMenu = menuItem.Parent as System.Windows.Controls.ContextMenu;
            var grid = contextMenu?.PlacementTarget as Grid;
            if (grid != null)
            {
                try
                {
                    if (grid.DataContext.GetType() != typeof(Models.DataCard))
                    {
                        Log.Warn(nameof(GetDataCard), "因为快速操作，导致UI关键对象跟踪失败");
                    }
                    Models.DataCard dataContext = (Models.DataCard)grid.DataContext;
                    return dataContext;
                }
                catch (Exception e)
                {
                    Log.Warn(nameof(GetDataCard), "获取房间卡片快照失败:1", e, true);
                    return new Models.DataCard();
                }
            }
            else
            {
                Log.Warn(nameof(GetDataCard), "获取房间卡片快照失败:2");
                return new Models.DataCard();
            }
        }

        private async void MenuItem_PlayWindow_Click(object sender, RoutedEventArgs e)
        {
            Models.DataCard dataCard = GetDataCard(sender);
            try
            {
                string hlsUrl = await GetHlsStreamUrlAsync(dataCard.Uid);
                if (!string.IsNullOrEmpty(hlsUrl))
                {
                    //取流结果直接传给播放窗复用，避免开窗后再发起一次重复的取流请求
                    Windows.VlcPlayWindow vlcPlayWindow = new Windows.VlcPlayWindow(dataCard.Uid, hlsUrl);
                    vlcPlayWindow.Show();
                }
                else
                {
                    Windows.WebPlayWindow WebPlayWindow = new Windows.WebPlayWindow(dataCard.Room_Id, dataCard.Uid);
                    WebPlayWindow.Show();
                }
            }
            catch (Exception ex)
            {
                Log.Warn(nameof(MenuItem_PlayWindow_Click), "检测HLS流失败", ex);
                Windows.WebPlayWindow WebPlayWindow = new Windows.WebPlayWindow(dataCard.Room_Id, dataCard.Uid);
                WebPlayWindow.Show();
            }
        }

        /// <summary>
        /// 黑听模式（纯音频）打开播放窗口：只播放直播音频，不渲染视频画面和弹幕
        /// </summary>
        private async void MenuItem_AudioOnlyPlayWindow_Click(object sender, RoutedEventArgs e)
        {
            Models.DataCard dataCard = GetDataCard(sender);
            try
            {
                string hlsUrl = await GetHlsStreamUrlAsync(dataCard.Uid);
                if (!string.IsNullOrEmpty(hlsUrl))
                {
                    //取流结果直接传给播放窗复用，避免开窗后再发起一次重复的取流请求
                    Windows.VlcPlayWindow vlcPlayWindow = new Windows.VlcPlayWindow(dataCard.Uid, hlsUrl, true);
                    vlcPlayWindow.Show();
                }
                else
                {
                    Windows.WebPlayWindow WebPlayWindow = new Windows.WebPlayWindow(dataCard.Room_Id, dataCard.Uid);
                    WebPlayWindow.Show();
                }
            }
            catch (Exception ex)
            {
                Log.Warn(nameof(MenuItem_AudioOnlyPlayWindow_Click), "检测HLS流失败", ex);
                Windows.WebPlayWindow WebPlayWindow = new Windows.WebPlayWindow(dataCard.Room_Id, dataCard.Uid);
                WebPlayWindow.Show();
            }
        }

        /// <summary>
        /// 异步获取HLS流地址，获取不到（无HLS流或请求失败）时返回null
        /// </summary>
        public async Task<string> GetHlsStreamUrlAsync(long uid)
        {
            return await Task.Run(() =>
            {
                RoomCardClass roomCard = new();
                _Room.GetCardForUID(uid, ref roomCard);
                string url = "";
                if (roomCard != null && Core.RuntimeObject.Download.HLS.GetHlsAvcUrl(roomCard, Core.Config.Core_RunConfig._DefaultPlayResolution, out url) && !string.IsNullOrEmpty(url))
                {
                    return url;
                }
                return null;
            });
        }

        private async void Border_DoubleClickToOpenPlaybackWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var border = (Border)sender;
                var grid = (Grid)border.Parent;
                Models.DataCard dataCard = (Models.DataCard)grid.DataContext;
                try
                {
                    string hlsUrl = await GetHlsStreamUrlAsync(dataCard.Uid);
                    if (!string.IsNullOrEmpty(hlsUrl))
                    {
                        //取流结果直接传给播放窗复用，避免开窗后再发起一次重复的取流请求
                        Windows.VlcPlayWindow vlcPlayWindow = new Windows.VlcPlayWindow(dataCard.Uid, hlsUrl);
                        vlcPlayWindow.Show();
                    }
                    else
                    {
                        Windows.WebPlayWindow WebPlayWindow = new Windows.WebPlayWindow(dataCard.Room_Id, dataCard.Uid);
                        WebPlayWindow.Show();
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn(nameof(Border_DoubleClickToOpenPlaybackWindow), "检测HLS流失败", ex);
                    Windows.WebPlayWindow WebPlayWindow = new Windows.WebPlayWindow(dataCard.Room_Id, dataCard.Uid);
                    WebPlayWindow.Show();
                }
            }
        }

        private async void MenuItem_ModifyRoom_AutoRec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Models.DataCard dataCard = GetDataCard(sender);
                bool success = await DataSource.RetrieveData.RoomInfo.ModifyRoomSettingsAsync(dataCard.Uid, !dataCard.IsRec, dataCard.IsDanmu, dataCard.IsRemind);
                if (!success)
                {
                    System.Windows.MessageBox.Show("修改房间设置失败，请检查网络连接或服务器状态。", "网络请求失败", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Core.LogModule.Log.Error(nameof(MenuItem_ModifyRoom_AutoRec_Click), "修改自动录制设置时发生异常", ex);
                System.Windows.MessageBox.Show("修改房间设置失败，请检查网络连接或服务器状态。", "操作失败", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async void MenuItem_ModifyRoom_Danmu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Models.DataCard dataCard = GetDataCard(sender);
                bool success = await DataSource.RetrieveData.RoomInfo.ModifyRoomSettingsAsync(dataCard.Uid, dataCard.IsRec, !dataCard.IsDanmu, dataCard.IsRemind);
                if (!success)
                {
                    System.Windows.MessageBox.Show("修改房间设置失败，请检查网络连接或服务器状态。", "网络请求失败", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Core.LogModule.Log.Error(nameof(MenuItem_ModifyRoom_Danmu_Click), "修改弹幕录制设置时发生异常", ex);
                System.Windows.MessageBox.Show("修改房间设置失败，请检查网络连接或服务器状态。", "操作失败", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async void MenuItem_ModifyRoom_Remind_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Models.DataCard dataCard = GetDataCard(sender);
                bool success = await DataSource.RetrieveData.RoomInfo.ModifyRoomSettingsAsync(dataCard.Uid, dataCard.IsRec, dataCard.IsDanmu, !dataCard.IsRemind);
                if (!success)
                {
                    System.Windows.MessageBox.Show("修改房间设置失败，请检查网络连接或服务器状态。", "网络请求失败", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Core.LogModule.Log.Error(nameof(MenuItem_ModifyRoom_Remind_Click), "修改开播提醒设置时发生异常", ex);
                System.Windows.MessageBox.Show("修改房间设置失败，请检查网络连接或服务器状态。", "操作失败", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async void DelRoom_Click(object sender, RoutedEventArgs e)
        {
            Models.DataCard dataCard = GetDataCard(sender);
            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                {"uids", dataCard.Uid.ToString() }
            };

            try
            {
                List<(long key, bool State, string Message)> State;

                if (Core.Config.Core_RunConfig._DesktopRemoteServer || Core.Config.Core_RunConfig._LocalHTTPMode)
                {
                    State = await NetWork.Post.PostBody<List<(long key, bool State, string Message)>>(
                        $"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/set_rooms/batch_delete_rooms", dic);
                }
                else
                {
                    State = Core.RuntimeObject._Room.BatchDeleteRooms(dataCard.Uid.ToString());
                }

                if (State == null)
                {
                    Log.Warn(nameof(DelRoom_Click), "调用Core的API[batch_delete_rooms]删除房间失败，返回的对象为Null");
                    MainWindow.SnackbarService.Show("删除房间失败", $"操作{dataCard.Nickname}({dataCard.Room_Id})时调用Core的API[batch_delete_rooms]删除房间失败", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle20), TimeSpan.FromSeconds(3));
                    return;
                }
                MainWindow.SnackbarService.Show("删除房间成功", $"{dataCard.Nickname}({dataCard.Room_Id})已从房间配置中删除", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Checkmark20), TimeSpan.FromSeconds(3));
            }
            catch (Exception ex)
            {
                Log.Warn(nameof(DelRoom_Click), "删除房间请求失败", ex);
            }
        }

        private async void Cancel_Task_Click(object sender, RoutedEventArgs e)
        {
            Models.DataCard dataCard = GetDataCard(sender);
            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                {"uid", dataCard.Uid.ToString() }
            };

            try
            {
                bool State;

                if (Core.Config.Core_RunConfig._DesktopRemoteServer || Core.Config.Core_RunConfig._LocalHTTPMode)
                {
                    State = await NetWork.Post.PostBody<bool>(
                        $"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/rec_task/cancel_task", dic);
                }
                else
                {
                    State = Core.RuntimeObject._Room.CancelTask(dataCard.Uid).State;
                }

                if (State == false)
                {
                    Log.Warn(nameof(Cancel_Task_Click), "调用Core的API[cancel_task]取消录制任务失败");
                    MainWindow.SnackbarService.Show("取消录制失败", $"操作{dataCard.Nickname}({dataCard.Room_Id})时调用Core的API[cancel_task]取消录制任务失败", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle20), TimeSpan.FromSeconds(3));
                }
                else
                {
                    MainWindow.SnackbarService.Show("取消录制成功", $"已取消{dataCard.Nickname}({dataCard.Room_Id})的录制任务", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Checkmark20), TimeSpan.FromSeconds(3));
                }
            }
            catch (Exception ex)
            {
                Log.Warn(nameof(Cancel_Task_Click), "取消录制请求失败", ex);
            }
        }

        private void MenuItem_DanmaOnly_Click(object sender, RoutedEventArgs e)
        {
            Models.DataCard dataCard = GetDataCard(sender);
            RoomCardClass roomCardClass = new();
            _Room.GetCardForUID(dataCard.Uid, ref roomCardClass);
            Windows.DanmaOnlyWindow danmaOnlyWindow = new(roomCardClass);
            danmaOnlyWindow.Show();
        }

        private void MenuItem_OpenLiveUlr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Models.DataCard dataCard = GetDataCard(sender);
                var psi = new ProcessStartInfo
                {
                    FileName = "https://live.bilibili.com/" + dataCard.Room_Id,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Log.Error(nameof(MenuItem_OpenLiveUlr_Click), "打开直播间网址失败", ex, true);
            }
        }

        private async void Snapshot_Task_Click(object sender, RoutedEventArgs e)
        {
            Models.DataCard dataCard = GetDataCard(sender);
            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                {"uid", dataCard.Uid.ToString() }
            };

            try
            {
                (bool state, string message) message;

                if (Core.Config.Core_RunConfig._DesktopRemoteServer || Core.Config.Core_RunConfig._LocalHTTPMode)
                {
                    message = await NetWork.Post.PostBody<(bool state, string message)>(
                        $"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/rec_task/generate_snapshot", dic, new TimeSpan(0, 1, 0));
                }
                else
                {
                    message = await Core.RuntimeObject.Download.Snapshot.CreateRecordingSnapshot(dataCard.Uid);
                }

                if (!message.state)
                {
                    Log.Info(nameof(Snapshot_Task_Click), $"生成直播间录制快照失败，原因:{message.message}");
                    MainWindow.SnackbarService.Show("快照失败", $"生成直播间录制快照失败，原因:{message.message}", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle20), TimeSpan.FromSeconds(5));
                }
                else
                {
                    MainWindow.SnackbarService.Show("快照完成", $"生成直播间录制快照完成，已输出到DDTV临时文件夹中（{message.message}）", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Checkmark20), TimeSpan.FromSeconds(10));
                }
            }
            catch (Exception ex)
            {
                Log.Warn(nameof(Snapshot_Task_Click), "生成快照请求失败", ex);
            }
        }

        private void MenuItem_Compatible_PlayWindow_Click(object sender, RoutedEventArgs e)
        {
            Models.DataCard dataCard = GetDataCard(sender);
            Windows.WebPlayWindow WebPlayWindow = new Windows.WebPlayWindow(dataCard.Room_Id, dataCard.Uid);
            WebPlayWindow.Show();
        }

        private void MenuItem_OpenRecFolder_Click(object sender, RoutedEventArgs e)
        {
            Models.DataCard dataCard = GetDataCard(sender);
            string folderPath = Path.GetFullPath(Path.Combine(
                Config.Core_RunConfig._RecFileDirectory,
                Core.Tools.KeyCharacterReplacement.ReplaceKeyword(
                    Text: Path.Combine(Config.Core_RunConfig._DefaultLiverFolderName),
                    uid: dataCard.Uid
                )));

            if (System.IO.Directory.Exists(folderPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true
                });
            }
        }
    }
}
