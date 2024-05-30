// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Core;
using Desktop.Models;
using Masuit.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace Desktop.Views.Pages;

/// <summary>
/// Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage
{
    public static Config.RunConfig configViewModel { get; set; } = new();

    private IContentDialogService _contentDialogService = new ContentDialogService();
    public SettingsPage()
    {
        InitializeComponent();
        _contentDialogService.SetDialogHost(RootContentDialogPresenter);
        this.DataContext = configViewModel;
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
        #region 远程连接相关设置检查
        if (DesktopRemoteServer_SwitchControl.IsChecked == true ? true : false)
        {
            if (string.IsNullOrEmpty(DesktopIP_InputControl.Text) || string.IsNullOrEmpty(DesktopPort_InputControl.Text) || string.IsNullOrEmpty(DesktopAccessKeyId_InputControl.Text) || string.IsNullOrEmpty(DesktopAccessKeySecret_InputControl.Text))
            {
                MainWindow.SnackbarService.Show("保存失败", "请检查基础设置中远程服务器相关配置正确且不为空", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.SaveSearch20), TimeSpan.FromSeconds(8));
                return false;
            }
            string url = DesktopIP_InputControl.Text;
            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (!result)
            {
                MainWindow.SnackbarService.Show("保存失败", "请检查填写的远端服务地址是否符合Url格式，例如：“http://example.com”或者“http://127.0.0.1”，Url字符串请勿携带端口", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.SaveSearch20), TimeSpan.FromSeconds(8));
                return false;
            }
        }
        #endregion

        #region 文件路径相关设置检查
        if (string.IsNullOrEmpty(RecPathInputBox.Text) || string.IsNullOrEmpty(DefaultLiverFolderNameInputBox.Text) || string.IsNullOrEmpty(DefaulFileNameFormatInputBox.Text))
        {
            MainWindow.SnackbarService.Show("保存失败", "请检查录制文件夹保存路径相关配置格式正确且不为空", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.SaveSearch20), TimeSpan.FromSeconds(5));
            return false;
        }
        try
        {
            string RecPath = $"{RecPathInputBox.Text}{DefaultLiverFolderNameInputBox.Text}/{DefaulFolderNameFormatInputBox.Text}{(string.IsNullOrEmpty(DefaulFolderNameFormatInputBox.Text)?"":"/")}{DefaulFileNameFormatInputBox.Text}";
            // 尝试获取完整路径，如果路径无效，将抛出异常
            string fullPath = Path.GetFullPath(RecPath);
            // 检查路径中是否包含无效字符
            bool containsInvalidChars = RecPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0;
            if (containsInvalidChars)
            {
                MainWindow.SnackbarService.Show("保存失败", "请检查录制文件夹保存路径相关配置格式正确且不为空", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.SaveSearch20), TimeSpan.FromSeconds(5));
                return false;
            }
        }
        catch (Exception)
        {
            MainWindow.SnackbarService.Show("保存失败", "请检查录制文件夹保存路径相关配置格式正确且不为空", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.SaveSearch20), TimeSpan.FromSeconds(5));
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
            var result = await _contentDialogService.ShowAsync(cd, cancellationToken);
            if (result != ContentDialogResult.Primary)
            {
                return;
            }
        }

        #region 保存远程连接相关设置 
        Config.Core_RunConfig._DesktopRemoteServer = (bool)DesktopRemoteServer_SwitchControl.IsChecked;
        if (DesktopRemoteServer_SwitchControl.IsChecked == true ? true : false)
        {
            Config.Core_RunConfig._DesktopIP = DesktopIP_InputControl.Text;
            Config.Core_RunConfig._DesktopPort = int.Parse(DesktopPort_InputControl.Text);
            Config.Core_RunConfig._DesktopAccessKeyId = DesktopAccessKeyId_InputControl.Text;
            Config.Core_RunConfig._DesktopAccessKeySecret = DesktopAccessKeySecret_InputControl.Text;
        }
        #endregion

        #region 保存录制路径相关设置
        Config.Core_RunConfig._RecFileDirectory = RecPathInputBox.Text;
        Config.Core_RunConfig._DefaultLiverFolderName = DefaultLiverFolderNameInputBox.Text;
        Config.Core_RunConfig._DefaultDataFolderName = DefaulFolderNameFormatInputBox.Text;
        Config.Core_RunConfig._DefaultFileName = DefaulFileNameFormatInputBox.Text;
        #endregion

        if (IsReboot)
        {
            MainWindow.SnackbarService.Show("保存成功", "多项配置需要重启后生效，5秒后自动重启DDTV", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.SaveSync20), TimeSpan.FromSeconds(30));
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
        await Core.Tools.ProgramUpdates.CheckForNewVersions(true,true);
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
            RecPathInputBox.Text= selectedPath.Replace("\\","/")+"/";
        }
    }

    private void ViewFileFormatExamples_Click(object sender, RoutedEventArgs e)
    {
         MainWindow.SnackbarService.Show("可用关键字", "{ROOMID}|{NAME}|{TITLE}|{DATE}|{TIME}　　　 说明:房间号|昵称|标题|日期(2016_12_01)|时间(11:22:33)\n{YYYY}|{YY}|{MM}|{DD}|{HH}|{mm}|{SS}|{FFF}　　说明:年(2016)|年(16)|月|日|时|分|秒|毫秒", ControlAppearance.Success, new SymbolIcon(SymbolRegular.ClipboardTextEdit20), TimeSpan.FromSeconds(30));
    }
}
