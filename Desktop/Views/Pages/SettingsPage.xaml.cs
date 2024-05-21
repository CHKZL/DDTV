// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Core;
using Desktop.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
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
    public static Core.Config.DesktopClass configViewModel { get; set; } = new();
    public SettingsPage()
    {
        InitializeComponent();
        this.DataContext = configViewModel;
    }

    /// <summary>
    /// openRecordingFolderInExplorer
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OpenRecordingFolderInExplorer_Click(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", Path.GetFullPath(Config.Core._RecFileDirectory));
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (configViewModel._DesktopRemoteServer)
        {
            MainWindow.SnackbarService.Show("1", "保存设置成功（其实没有，现在只是把这个效果写了）", ControlAppearance.Success, null, TimeSpan.FromSeconds(2));
        }
        else
        {
            MainWindow.SnackbarService.Show("2", "保存设置成功（其实没有，现在只是把这个效果写了）", ControlAppearance.Success, null, TimeSpan.FromSeconds(2));
        }
        configViewModel._DesktopRemoteServer = !configViewModel._DesktopRemoteServer;
        //MainWindow.SnackbarService.Show("保存设置", "保存设置成功（其实没有，现在只是把这个效果写了）", ControlAppearance.Success, null, TimeSpan.FromSeconds(2));
    }
}
