using Core;
using Desktop.DataSource;
using Desktop.Views.Pages;
using System.Windows;

namespace Desktop.Services
{
    /// <summary>
    /// 统一的程序退出入口，供主窗口关闭按钮、托盘右键退出等所有退出路径复用。
    /// 退出前统一执行录制中检查（有直播间正在录制时弹确认框并显示录制数量）与清理逻辑。
    /// </summary>
    internal static class ExitService
    {
        /// <summary>
        /// 带录制保护的退出确认：强制刷新一次统计，若有直播间正在录制则弹出确认框，用户选"是"才允许退出。
        /// 没有录制任务时直接放行，不弹任何框。
        /// </summary>
        /// <param name="owner">确认框的属主窗口</param>
        /// <returns>true表示可以退出（无录制或用户已确认）</returns>
        public static async Task<bool> ConfirmExitIfRecordingAsync(Window owner)
        {
            //force跳过后台模式与缓存间隔检查，确保拿到的是当前最新统计
            await RoomStatistics.RefreshAsync(true);
            int recCount = RoomStatistics.Current.RecCount;
            if (recCount <= 0)
            {
                return true;
            }

            var messageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "退出确认",
                Content = $"当前有【{recCount}】个直播间正在录制中。\r\n确认要退出DDTV吗？\r\n退出后所有录制任务以及播放窗口均会结束。",
                PrimaryButtonText = "是",
                SecondaryButtonText = "否",
                IsCloseButtonEnabled = false,
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var result = await messageBox.ShowDialogAsync();
            return result == Wpf.Ui.Controls.MessageBoxResult.Primary;
        }

        /// <summary>
        /// 执行退出前的统一清理并结束进程
        /// </summary>
        public static void Exit()
        {
            DataPage.Timer_DataPage?.Dispose();
            LoginStatus.Timer_LoginStatus?.Dispose();
            MainWindow.IsProgrammaticClose = true;
            Environment.Exit(Core.Init.ExitCodes.FatalError);
        }
    }
}
