// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Core.LogModule;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Desktop.Views.Pages;

/// <summary>
/// Interaction logic for DashboardPage.xaml
/// </summary>
public partial class LogPage
{
    public ObservableCollection<LogClass> LogCollection = new();


    public LogPage()
    {
        InitializeComponent();
        LogView.ItemsSource = LogCollection;
        foreach (var log in Log.LogList)
        {
            LogCollection.Add(log);
        }
        Log.LogAddEvent += Log_LogAddEvent;
    }

    private void Log_LogAddEvent(object? sender, EventArgs e)
    {
        LogClass? logClass = (LogClass)sender;
        if (logClass != null && !logClass.Message.Contains("使用内存"))
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogCollection.Insert(0, (LogClass)sender);
                while (LogCollection.Count > 200)
                {
                    LogCollection.RemoveAt(LogCollection.Count - 1);
                }
            });
        }
    }
}
