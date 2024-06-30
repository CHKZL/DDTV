// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Core;
using Desktop.Models;
using Desktop.Views.Windows;
using Masuit.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Navigation;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using static Core.RuntimeObject.Download.Basics;

namespace Desktop.Views.Pages;

/// <summary>
/// Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage
{
    public static Config.RunConfig configViewModel { get; set; } = new();

    public SettingsPage()
    {
        InitializeComponent();
        this.DataContext = configViewModel;


        SaveTipsAnimation();

    }

    private void SaveTipsAnimation()
    {
        int basicTime = 400;

        // 创建一个颜色动画
        ColorAnimationUsingKeyFrames colorAnimation = new ColorAnimationUsingKeyFrames();
        colorAnimation.KeyFrames.Add(new LinearColorKeyFrame(Colors.Red, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(basicTime * 0))));
        colorAnimation.KeyFrames.Add(new LinearColorKeyFrame(Colors.Orange, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(basicTime * 1))));
        colorAnimation.KeyFrames.Add(new LinearColorKeyFrame(Colors.Yellow, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(basicTime * 2))));
        colorAnimation.KeyFrames.Add(new LinearColorKeyFrame(Colors.Green, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(basicTime * 3))));
        colorAnimation.KeyFrames.Add(new LinearColorKeyFrame(Colors.Cyan, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(basicTime * 4))));
        colorAnimation.KeyFrames.Add(new LinearColorKeyFrame(Colors.Blue, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(basicTime * 5))));
        colorAnimation.KeyFrames.Add(new LinearColorKeyFrame(Colors.Purple, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(basicTime * 6))));
        colorAnimation.KeyFrames.Add(new LinearColorKeyFrame(Colors.Blue, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(basicTime * 7))));
        colorAnimation.KeyFrames.Add(new LinearColorKeyFrame(Colors.Cyan, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(basicTime * 8))));
        colorAnimation.KeyFrames.Add(new LinearColorKeyFrame(Colors.Green, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(basicTime * 9))));
        colorAnimation.KeyFrames.Add(new LinearColorKeyFrame(Colors.Yellow, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(basicTime * 10))));
        colorAnimation.KeyFrames.Add(new LinearColorKeyFrame(Colors.Orange, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(basicTime * 11))));
        colorAnimation.KeyFrames.Add(new LinearColorKeyFrame(Colors.Red, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(basicTime * 12))));
        colorAnimation.RepeatBehavior = RepeatBehavior.Forever;

        // 创建一个线性渐变刷，并将动画应用到其中一个渐变停止的颜色上
        LinearGradientBrush brush = new LinearGradientBrush();
        GradientStop stop = new GradientStop();
        brush.GradientStops.Add(stop);
        stop.BeginAnimation(GradientStop.ColorProperty, colorAnimation);

        // 将刷子应用到TextBlock的前景色上
        SaveTips.Foreground = brush;
    }

    /// <summary>
    /// openRecordingFolderInExplorer
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OpenRecordingFolderInExplorer_Click(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", Path.GetFullPath(Config.Core_RunConfig._RecFileDirectory));
    }

    private async void Config_Save_Button_Click(object sender, RoutedEventArgs e)
    {
        SaveConfiguration();
    }



    #region SaveOperation
    public bool CheckConfiguration(ref bool Reboot)
    {
        #region 基础设置
        //远程连接相关设置检查
        if (DesktopRemoteServer_SwitchControl.IsChecked == true ? true : false)
        {
            if (string.IsNullOrEmpty(DesktopIP_InputControl.Text) || string.IsNullOrEmpty(DesktopPort_InputControl.Text) || string.IsNullOrEmpty(DesktopAccessKeyId_InputControl.Text) || string.IsNullOrEmpty(DesktopAccessKeySecret_InputControl.Text))
            {
                MainWindow.SnackbarService.Show("保存失败", "请检查确保基础设置中远程服务器相关配置正确且不为空", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.SaveSearch20), TimeSpan.FromSeconds(8));
                return false;
            }
            string url = DesktopIP_InputControl.Text;
            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (!result)
            {
                MainWindow.SnackbarService.Show("保存失败", "请检查确保填写的远端服务地址是否符合Url格式，例如：“http://example.com”或者“http://127.0.0.1”，Url字符串请勿携带端口", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.SaveSearch20), TimeSpan.FromSeconds(8));
                return false;
            }
        }
        //远程地址检测
        if (string.IsNullOrEmpty(DesktopIP_InputControl.Text) || string.IsNullOrEmpty(DesktopPort_InputControl.Text))
        {
            MainWindow.SnackbarService.Show("保存失败", "请检查确保远程服务器地址配置正确且不为空", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.SaveSearch20), TimeSpan.FromSeconds(8));
            return false;
        }
        //API_Url配置保存
        if (string.IsNullOrEmpty(MainDomainName_TextBox.Text) || string.IsNullOrEmpty(LiveDomainName_TextBox.Text))
        {
            MainWindow.SnackbarService.Show("保存失败", "请检查确保API地址配置正确且不为空", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.SaveSearch20), TimeSpan.FromSeconds(8));
            return false;
        }
        //WebHook配置保存
        if ((bool)WebHook_SwitchControl.IsChecked && string.IsNullOrEmpty(WebHookUrl_InputControl.Text))
        {
            MainWindow.SnackbarService.Show("保存失败", "请检查确保WebHook地址配置正确且不为空", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.SaveSearch20), TimeSpan.FromSeconds(8));
            return false;
        }     
        #endregion

        #region 文件路径相关设置检查
        if (string.IsNullOrEmpty(RecPathInputBox.Text) || string.IsNullOrEmpty(DefaultLiverFolderNameInputBox.Text) || string.IsNullOrEmpty(DefaulFileNameFormatInputBox.Text))
        {
            MainWindow.SnackbarService.Show("保存失败", "请检查确保录制文件夹保存路径相关配置格式正确且不为空", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.SaveSearch20), TimeSpan.FromSeconds(5));
            return false;
        }
        try
        {
            string RecPath = $"{RecPathInputBox.Text}{DefaultLiverFolderNameInputBox.Text}/{DefaulFolderNameFormatInputBox.Text}{(string.IsNullOrEmpty(DefaulFolderNameFormatInputBox.Text) ? "" : "/")}{DefaulFileNameFormatInputBox.Text}";
            // 尝试获取完整路径，如果路径无效，将抛出异常
            string fullPath = Path.GetFullPath(RecPath);
            // 检查路径中是否包含无效字符
            bool containsInvalidChars = RecPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0;
            if (containsInvalidChars)
            {
                MainWindow.SnackbarService.Show("保存失败", "请检查确保录制文件夹保存路径相关配置格式正确且不为空", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.SaveSearch20), TimeSpan.FromSeconds(5));
                return false;
            }
        }
        catch (Exception)
        {
            MainWindow.SnackbarService.Show("保存失败", "请检查确保录制文件夹保存路径相关配置格式正确且不为空", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.SaveSearch20), TimeSpan.FromSeconds(5));
            return false;
        }
        #endregion

        #region 录制功能设置

        //录制模式
        if (RecordingMode_ComboBox.SelectedIndex != 1 && string.IsNullOrEmpty(HlsWaitingTime_InputControl.Text))
        {
            MainWindow.SnackbarService.Show("保存失败", "请检查确保HLS等待时间配置正确且不为空", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.SaveSearch20), TimeSpan.FromSeconds(8));
            return false;
        }

        //自动转码参数检查
        if (string.IsNullOrEmpty(AutomaticRepair_Arguments_InputBox.Text))
        {
            MainWindow.SnackbarService.Show("保存失败", "请检查确保修复和转码的执行参数配置正确且不为空", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.SaveSearch20), TimeSpan.FromSeconds(8));
            return false;
        }


        #endregion

        #region 播放窗口设置
        //弹幕速度设置
        if (string.IsNullOrEmpty(PlayWindowDanmaSpeed_InputBox.Text) && int.Parse(PlayWindowDanmaSpeed_InputBox.Text)>0)
        {
            MainWindow.SnackbarService.Show("保存失败", "请检查确播放窗口弹幕速度参数配置正确且不为空", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.SaveSearch20), TimeSpan.FromSeconds(8));
            return false;
        }
        #endregion

        #region 需要重启的关键参数修改检查
        Reboot = Config.Core_RunConfig._DesktopRemoteServer != (bool)DesktopRemoteServer_SwitchControl.IsChecked
            || Config.Core_RunConfig._DesktopIP != DesktopIP_InputControl.Text
            || Config.Core_RunConfig._DesktopPort != int.Parse(DesktopPort_InputControl.Text)
            || Config.Core_RunConfig._DesktopAccessKeyId != DesktopAccessKeyId_InputControl.Text
            || Config.Core_RunConfig._DesktopAccessKeySecret != DesktopAccessKeySecret_InputControl.Text
            || Config.Core_RunConfig._RecFileDirectory != RecPathInputBox.Text
            || Config.Core_RunConfig._DefaultLiverFolderName != DefaultLiverFolderNameInputBox.Text
            || Config.Core_RunConfig._DefaultDataFolderName != DefaulFolderNameFormatInputBox.Text
            || Config.Core_RunConfig._DefaultFileName != DefaulFileNameFormatInputBox.Text;
        #endregion


        return true;
    }

    /// <summary>
    /// 保存当前设置页内容
    /// </summary>
    public async void SaveConfiguration()
    {
        bool IsReboot = false;
        if (!CheckConfiguration(ref IsReboot))
        {
            return;
        }

        if (IsReboot)
        {
            var cd = new ContentDialog
            {
                Title = "保存配置",
                Content = "修改了关键配置，需要重启生效，确认要保存配置文件么？\n\r确认保存后DDTV将在5秒后自动重新启动。",
                PrimaryButtonText = "确认保存",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Close
            };
            var cancellationToken = new CancellationToken();
            var result = await MainWindow._contentDialogService.ShowAsync(cd, cancellationToken);
            if (result != ContentDialogResult.Primary)
            {
                return;
            }
        }

        #region 保存远程连接相关设置 
        //远程连接配置保存
        Config.Core_RunConfig._DesktopRemoteServer = (bool)DesktopRemoteServer_SwitchControl.IsChecked;
        if (DesktopRemoteServer_SwitchControl.IsChecked == true ? true : false)
        {
            Config.Core_RunConfig._DesktopIP = DesktopIP_InputControl.Text;
            Config.Core_RunConfig._DesktopPort = int.Parse(DesktopPort_InputControl.Text);
            Config.Core_RunConfig._DesktopAccessKeyId = DesktopAccessKeyId_InputControl.Text;
            Config.Core_RunConfig._DesktopAccessKeySecret = DesktopAccessKeySecret_InputControl.Text;
        }
        //缩小行为配置保存
        if (Config.Core_RunConfig._ZoomOutMode != ZoomOutMode_ComboBox.SelectedIndex)
        {
            Config.Core_RunConfig._ZoomOutMode = ZoomOutMode_ComboBox.SelectedIndex;
        }
        //开播卡片配置保存
        if (Config.Core_RunConfig._SystemCardReminder != SystemCardReminder_ToggleSwitch.IsChecked)
        {
            Config.Core_RunConfig._SystemCardReminder = (bool)SystemCardReminder_ToggleSwitch.IsChecked;
        }
        //API_Url配置保存
        Config.Core_RunConfig._MainDomainName = MainDomainName_TextBox.Text;
        Config.Core_RunConfig._LiveDomainName = LiveDomainName_TextBox.Text;
        //开发版更新配置
        if (Config.Core_RunConfig._DevelopmentVersion != DevelopmentVersion_ToggleSwitch.IsChecked)
        {
            Config.Core_RunConfig._DevelopmentVersion = (bool)DevelopmentVersion_ToggleSwitch.IsChecked;
        }
        //WebHook配置保存
        if (Config.Core_RunConfig._WebHookSwitch != WebHook_SwitchControl.IsChecked)
        {
            Config.Core_RunConfig._WebHookSwitch = (bool)WebHook_SwitchControl.IsChecked;
        }
        if (Config.Core_RunConfig._WebHookAddress != WebHookUrl_InputControl.Text)
        {
            Config.Core_RunConfig._WebHookAddress = WebHookUrl_InputControl.Text;
        }

        #endregion

        #region 文件路径相关设置保存
        if (Config.Core_RunConfig._RecFileDirectory != RecPathInputBox.Text)
            Config.Core_RunConfig._RecFileDirectory = RecPathInputBox.Text;
        if (Config.Core_RunConfig._DefaultLiverFolderName != DefaultLiverFolderNameInputBox.Text)
            Config.Core_RunConfig._DefaultLiverFolderName = DefaultLiverFolderNameInputBox.Text;
        if (Config.Core_RunConfig._DefaultDataFolderName != DefaulFolderNameFormatInputBox.Text)
            Config.Core_RunConfig._DefaultDataFolderName = DefaulFolderNameFormatInputBox.Text;
        if (Config.Core_RunConfig._DefaultFileName != DefaulFileNameFormatInputBox.Text)
            Config.Core_RunConfig._DefaultFileName = DefaulFileNameFormatInputBox.Text;
        #endregion

        #region 录制功能设置保存
        //录制分辨率
        if (Config.Core_RunConfig._DefaultResolution_For_ComboBox != DefaultResolution_For_ComboBox.SelectedIndex)
        {
            Config.Core_RunConfig._DefaultResolution_For_ComboBox = DefaultResolution_For_ComboBox.SelectedIndex;
        }
        //录制模式
        if ((int)Config.Core_RunConfig._RecordingMode - 1 != RecordingMode_ComboBox.SelectedIndex)
        {
            Config.Core_RunConfig._RecordingMode = (RecordingMode)RecordingMode_ComboBox.SelectedIndex + 1;
        }
        Config.Core_RunConfig._HlsWaitingTime = int.Parse(HlsWaitingTime_InputControl.Text);

        //自动转码
        if (Config.Core_RunConfig._AutomaticRepair != AutomaticRepair_SwitchControl.IsChecked)
        {
            Config.Core_RunConfig._AutomaticRepair = (bool)AutomaticRepair_SwitchControl.IsChecked;
        }
        Config.Core_RunConfig._AutomaticRepair_Arguments = AutomaticRepair_Arguments_InputBox.Text;

        //文件按大小分割
        if (Config.Core_RunConfig.CutAccordingToSize_For_ComboBox != CutAccordingToSize_For_ComboBox.SelectedIndex)
        {
            Config.Core_RunConfig.CutAccordingToSize_For_ComboBox = CutAccordingToSize_For_ComboBox.SelectedIndex;
        }
        //文件按时间分割
        if (Config.Core_RunConfig.CutAccordingToTime_For_ComboBox != CutAccordingToTime_For_ComboBox.SelectedIndex)
        {
            Config.Core_RunConfig.CutAccordingToTime_For_ComboBox = CutAccordingToTime_For_ComboBox.SelectedIndex;
        }
        //保存直播间封面
        if (Config.Core_RunConfig._SaveCover != SaveCover_SwitchControl.IsChecked)
        {
            Config.Core_RunConfig._SaveCover = (bool)SaveCover_SwitchControl.IsChecked;
        }

        #endregion

        #region 播放窗口设置
        //弹幕默认开关设置
        if (Config.Core_RunConfig._PlayWindowDanmaSwitch != PlayWindowDanmaSwitch_CheckBox.IsChecked)
        {
            Config.Core_RunConfig._PlayWindowDanmaSwitch = (bool)PlayWindowDanmaSwitch_CheckBox.IsChecked;
        }
        //弹幕速度设置
        if (Config.Core_RunConfig._PlayWindowDanmaSpeed != int.Parse(PlayWindowDanmaSpeed_InputBox.Text))
        {
            Config.Core_RunConfig._PlayWindowDanmaSpeed = int.Parse(PlayWindowDanmaSpeed_InputBox.Text);
        }
        if (Config.Core_RunConfig._PlayDanmaSpeed_Dynamically != PlayWindowDanmaSpeed_CheckBox.IsChecked)
        {
            Config.Core_RunConfig._PlayDanmaSpeed_Dynamically = (bool)PlayWindowDanmaSpeed_CheckBox.IsChecked;
        }
        //弹幕字号设置
        if (Config.Core_RunConfig._PlayWindowDanmaFontSize != int.Parse(PlayWindowDanmaSize.Text))
        {
            Config.Core_RunConfig._PlayWindowDanmaFontSize = int.Parse(PlayWindowDanmaSize.Text);
        }
        #endregion

        if (IsReboot)
        {
            MainWindow.SnackbarService.Show("保存成功", "多项配置需要重启后生效，5秒后自动重启DDTV", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.SaveSync20), TimeSpan.FromSeconds(30));
            MainWindow.IsProgrammaticClose = true;
            Task.Run(() =>
               {
                   Thread.Sleep(6000);
                   Dispatcher.BeginInvoke(new Action(delegate
                   {
                       System.Windows.Forms.Application.Restart();
                       Application.Current.Shutdown();
                   }));

               });
        }
        else
        {
            MainWindow.SnackbarService.Show("保存成功", "5秒后写入配置文件持久化保存", ControlAppearance.Success, new SymbolIcon(SymbolRegular.DocumentSave20), TimeSpan.FromSeconds(3));
        }
    }

    #endregion



    private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.SnackbarService.Show("检查更新", "正在检测更新，请稍候...", ControlAppearance.Info, new SymbolIcon(SymbolRegular.ArrowSync20), TimeSpan.FromSeconds(5));
        if (await Core.Tools.ProgramUpdates.CheckForNewVersions(false, true))
        {
            var cd = new ContentDialog
            {
                Title = "检测到更新，是否更新？",
                Content = "确认更新将会关闭DDTV，然后进行更新。",
                PrimaryButtonText = "确认更新",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Close
            };
            var cancellationToken = new CancellationToken();
            var result = await MainWindow._contentDialogService.ShowAsync(cd, cancellationToken);
            if (result != ContentDialogResult.Primary)
            {
                return;
            }
            Core.Tools.ProgramUpdates.CallUpUpdateProgram();
        }
        else
        {
            MainWindow.SnackbarService.Show("检查更新", "当前已是最新版本", ControlAppearance.Success, new SymbolIcon(SymbolRegular.ArrowSync20), TimeSpan.FromSeconds(3));
        }


    }

    private void SelectRecordingFolder_Click(object sender, RoutedEventArgs e)
    {
        // 创建一个FolderBrowserDialog对象
        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

        // 显示对话框
        DialogResult result = folderBrowserDialog.ShowDialog();

        // 检查用户是否点击了“确定”按钮
        if (result == DialogResult.OK)
        {
            // 用户选择的文件夹的绝对路径
            string selectedPath = folderBrowserDialog.SelectedPath;
            RecPathInputBox.Text = selectedPath.Replace("\\", "/") + "/";
        }
    }

    private void ViewFileFormatExamples_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.SnackbarService.Show("可用关键字", "{ROOMID}|{NAME}|{TITLE}|{DATE}|{TIME}　　　 说明:房间号|昵称|标题|日期(2016_12_01)|时间(11:22:33)\n{YYYY}|{YY}|{MM}|{DD}|{HH}|{mm}|{SS}|{FFF}　　说明:年(2016)|年(16)|月|日|时|分|秒|毫秒", ControlAppearance.Success, new SymbolIcon(SymbolRegular.ClipboardTextEdit20), TimeSpan.FromSeconds(30));
    }

    private async void ReLogin_Button_ClickAsync(object sender, RoutedEventArgs e)
    {
        var cd = new ContentDialog
        {
            Title = "重新登陆",
            Content = "确认要触发重新登陆么？\n\r【警告】请注意确认后将会清空当前登录态。",
            PrimaryButtonText = "确认",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Close
        };
        var cancellationToken = new CancellationToken();
        var result = await MainWindow._contentDialogService.ShowAsync(cd, cancellationToken);
        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        if (!DataSource.LoginStatus.LoginWindowDisplayStatus)
        {
            DataSource.LoginStatus.LoginWindowDisplayStatus = true;
            Dispatcher.Invoke(() =>
            {
                QrLogin qrLogin = new QrLogin();
                qrLogin.ShowDialog();
            });
        }
    }
}
