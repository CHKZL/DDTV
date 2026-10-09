// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Core.LogModule;
using System.Collections.ObjectModel;

namespace Desktop.Views.Pages;

/// <summary>
/// Interaction logic for DashboardPage.xaml
/// </summary>
public partial class LogPage
{
    public ObservableCollection<LogClass> LogCollection = new();
    static long ErrorCount = 0;

    /// <summary>
    /// 待刷新到界面的日志缓冲（200ms聚合一次批量进UI线程，避免每条日志都Invoke）
    /// </summary>
    private readonly List<LogClass> _pendingLogs = new();
    private readonly object _pendingLock = new();
    private bool _flushScheduled = false;
    private long _pendingErrorCount = 0;

    public LogPage()
    {
        InitializeComponent();
        LogView.ItemsSource = LogCollection;
        foreach (var log in Log.GetLogListSnapshot())
        {
            LogCollection.Add(log);
        }
        Log.LogAddEvent += Log_LogAddEvent;
        //页面卸载时退订日志事件，避免每次导航进入都多挂一个订阅导致更新次数成倍增长
        Unloaded += LogPage_Unloaded;
        LogTips.Text = "Log系统状态:记录中 tag标记:" + ErrorCount;
    }

    private void LogPage_Unloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Log.LogAddEvent -= Log_LogAddEvent;
        Unloaded -= LogPage_Unloaded;
    }

    private void Log_LogAddEvent(object? sender, EventArgs e)
    {
        LogClass? logClass = (LogClass)sender;
        if (logClass != null && !logClass.Message.Contains("使用内存"))
        {
            lock (_pendingLock)
            {
                //Split一次缓存复用，避免每条日志重复分配
                if (logClass.Message.Contains("触发DesktopTips"))
                {
                    var parts = logClass.Message.Split('=');
                    if (parts.Length > 1 && long.TryParse(parts[1], out long Count))
                    {
                        _pendingErrorCount += Count;
                    }
                }
                _pendingLogs.Add(logClass);
                if (_flushScheduled)
                {
                    return;
                }
                _flushScheduled = true;
            }
            //200ms后批量刷新一次，日志洪峰时UI更新次数从每秒数百次降到5次
            _ = Task.Run(async () =>
            {
                await Task.Delay(200);
                try
                {
                    await Dispatcher.InvokeAsync(FlushPendingLogs);
                }
                catch (Exception)
                {
                    //页面已卸载等情况忽略
                }
            });
        }
    }

    /// <summary>
    /// 将缓冲的日志批量刷新到界面（仅在UI线程执行）
    /// </summary>
    private void FlushPendingLogs()
    {
        List<LogClass> logs;
        long errorCountDelta;
        lock (_pendingLock)
        {
            logs = new List<LogClass>(_pendingLogs);
            _pendingLogs.Clear();
            errorCountDelta = _pendingErrorCount;
            _pendingErrorCount = 0;
            _flushScheduled = false;
        }
        if (errorCountDelta != 0)
        {
            ErrorCount += errorCountDelta;
            LogTips.Text = "Log系统状态:记录中 tag标记:" + ErrorCount;
        }
        foreach (var log in logs)
        {
            LogCollection.Insert(0, log);
        }
        while (LogCollection.Count > 200)
        {
            LogCollection.RemoveAt(LogCollection.Count - 1);
        }
    }
}
