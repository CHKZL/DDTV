// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Core;
using Core.LogModule;
using Desktop.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Media.Animation;
using System.Windows.Media;
using static Core.Tools.DokiDoki;
using static Core.Tools.SystemResource.Overview;
using static Server.WebAppServices.Api.get_system_resources;
using Core.RuntimeObject;

namespace Desktop.Views.Pages;

/// <summary>
/// Interaction logic for DashboardPage.xaml
/// </summary>
public partial class DefaultPage
{
    internal static HomePageModels PageComboBoxItems { get; private set; }
    private System.Threading.Timer UpdateHardwareResourceUtilizationRateTimer;
    private System.Threading.Timer UpdateRuntimeStatisticsTimer;
    private System.Threading.Timer UpdateAnnouncementTimer;
    private System.Threading.Timer ProxyDetectionTimer;
    private System.Threading.Timer IpvDetectionTimer;

    public DefaultPage()
    {
        Core.RuntimeObject.Account.LoginFailureEvent += Account_LoginFailureEvent;
        InitializeComponent();
        PageComboBoxItems = new();
        this.DataContext = PageComboBoxItems;

        //房间统计改为订阅共享缓存（RoomStatistics由MainWindow定时器统一驱动，此处不再单独获取）
        DataSource.RoomStatistics.Updated += RoomStatistics_Updated;
        //先用缓存的当前值填充一次：缓存新鲜时RefreshAsync不会触发事件，不填充会显示默认值直到下次数值变化
        RoomStatistics_Updated();
        _ = DataSource.RoomStatistics.RefreshAsync();
        //更新硬件使用率
        UpdateHardwareResourceUtilizationRateTimer = new System.Threading.Timer(async _ => await UpdateHardwareResourceUtilizationRateAsync(), null, 1000, 60 * 1000);
        //更新运行时长
        UpdateRuntimeStatisticsTimer = new System.Threading.Timer(_ => UpdateRuntimeStatistics(), null, 1000, 1000);
        //更新公告
        UpdateAnnouncementTimer = new System.Threading.Timer(async _ => await UpdateAnnouncementAsync(), null, 1, 1000 * 60 * 60);
        //代理状态检测
        ProxyDetectionTimer = new System.Threading.Timer(_ => ProxyDetection(), null, 1, 1000 * 60 * 30);
        //IP版本检测
        IpvDetectionTimer = new System.Threading.Timer(async _ => await IpvDetectionAsync(), null, 1, 1000 * 60 * 30);
        WarningMessageAnimation();
        //页面卸载时释放定时器并退订事件，避免每次导航进入都多挂6个Timer和1个事件订阅
        Unloaded += DefaultPage_Unloaded;
    }

    private void DefaultPage_Unloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        UpdateHardwareResourceUtilizationRateTimer?.Dispose();
        UpdateRuntimeStatisticsTimer?.Dispose();
        UpdateAnnouncementTimer?.Dispose();
        ProxyDetectionTimer?.Dispose();
        IpvDetectionTimer?.Dispose();
        Core.RuntimeObject.Account.LoginFailureEvent -= Account_LoginFailureEvent;
        DataSource.RoomStatistics.Updated -= RoomStatistics_Updated;
        Unloaded -= DefaultPage_Unloaded;
    }

    /// <summary>
    /// 警告信息跑马灯
    /// </summary>
    private void WarningMessageAnimation()
    {
        int basicTime = 400;

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

        LinearGradientBrush brush = new LinearGradientBrush();
        GradientStop stop = new GradientStop();
        brush.GradientStops.Add(stop);
        stop.BeginAnimation(GradientStop.ColorProperty, colorAnimation);
        WarningMessage.Foreground = brush;
    }

    public static void SetMonitoringCount(int count)
    {
        PageComboBoxItems.MonitoringCount = count;
        PageComboBoxItems.OnPropertyChanged("MonitoringCount");
    }
    public static void SetLiveCount(int count)
    {
        PageComboBoxItems.LiveCount = count;
        PageComboBoxItems.OnPropertyChanged("LiveCount");
    }
    public static void SetRecCount(int count)
    {
        PageComboBoxItems.RecCount = count;
        PageComboBoxItems.OnPropertyChanged("RecCount");
    }
    public static void SetWarningMessage(string str)
    {
        PageComboBoxItems.WarningMessage = str;
        PageComboBoxItems.OnPropertyChanged("WarningMessage");
    }
    public static void SetHardDiskUsageRate(int count)
    {
        PageComboBoxItems.HardDiskUsageRate = count;
        PageComboBoxItems.OnPropertyChanged("HardDiskUsageRate");
    }
    public static void SetMemoryUsageRate(int count)
    {
        PageComboBoxItems.MemoryUsageRate = count;
        PageComboBoxItems.OnPropertyChanged("MemoryUsageRate");
    }
    public static void SetRunTime(string str)
    {
        PageComboBoxItems.RunTime = str;
        PageComboBoxItems.OnPropertyChanged("RunTime");
    }
    public static void SetProxyState(string str)
    {
        PageComboBoxItems.ProxyState = str;
        PageComboBoxItems.OnPropertyChanged("ProxyState");
    }
    public static void SetIpvState(string str)
    {
        PageComboBoxItems.IpvState = str;
        PageComboBoxItems.OnPropertyChanged("IpvState");
    }
    public static void SetProxyUrl(string str)
    {
        PageComboBoxItems.ProxyUrl = str;
        PageComboBoxItems.OnPropertyChanged("ProxyUrl");
    }

    /// <summary>
    /// 异步更新公告
    /// </summary>
    public static async Task UpdateAnnouncementAsync()
    {
        try
        {
            string announcement = await Task.Run(() => Core.Tools.ProgramUpdates.Get("/announcement.txt"));
            PageComboBoxItems.announcement = announcement;
            PageComboBoxItems.OnPropertyChanged("announcement");
        }
        catch (Exception ex)
        {
            Log.Warn(nameof(UpdateAnnouncementAsync), "更新公告出现错误", ex, false);
        }
    }

    /// <summary>
    /// 检测代理状态
    /// </summary>
    public static void ProxyDetection()
    {
        try
        {
            var defaultProxy = System.Net.WebRequest.DefaultWebProxy;
            if (defaultProxy != null)
            {
                var proxyUri = defaultProxy.GetProxy(new Uri(Config.Core_RunConfig._LiveDomainName));
                if (proxyUri == null)
                {
                    SetProxyState("正常，未检测到代理");
                    return;
                }
                if (proxyUri?.AbsoluteUri != Config.Core_RunConfig._LiveDomainName)
                {
                    Log.Info(nameof(ProxyDetection), $"系统代理已启用，代理地址：{proxyUri.AbsoluteUri}", false);
                    SetProxyState("检测到系统代理已启用");
                    SetProxyUrl($"当前代理地址:{proxyUri.AbsoluteUri}");
                    return;
                }
            }
            SetProxyState("正常，未检测到代理");
        }
        catch (Exception ex)
        {
            Log.Warn(nameof(ProxyDetection), $"检测到系统代理出现错误,{ex.ToString()}", ex, false);
        }
    }

    /// <summary>
    /// 异步检测IP版本（移到低优先级线程执行）
    /// </summary>
    public static async Task IpvDetectionAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                string url = Config.Core_RunConfig._LiveDomainName.ToLower().Replace("https://", "").Replace("http://", "");
                IPHostEntry hostEntry = Dns.GetHostEntry(url);
                foreach (IPAddress ipAddress in hostEntry.AddressList)
                {
                    using Socket tempSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    tempSocket.Connect(new IPEndPoint(ipAddress, 80));

                    if (tempSocket.Connected)
                    {
                        switch (tempSocket.AddressFamily)
                        {
                            case AddressFamily.InterNetwork:
                                SetIpvState("目前使用的IPv4协议");
                                break;
                            case AddressFamily.InterNetworkV6:
                                SetIpvState("目前使用的IPv6协议");
                                Log.Info(nameof(IpvDetectionAsync), $"当前为IPv6访问状态", false);
                                break;
                        }
                        break;
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Log.Warn(nameof(IpvDetectionAsync), $"检测IP版本出现错误,{ex.ToString()}", ex, false);
        }
    }

    /// <summary>
    /// 共享统计缓存更新事件：直接读取缓存值刷新面板（事件在后台线程触发，
    /// WPF对标量属性的PropertyChanged会自动封送到UI线程，与原定时器线程直接调用的行为一致）
    /// </summary>
    private static void RoomStatistics_Updated()
    {
        var count = DataSource.RoomStatistics.Current;
        SetMonitoringCount(count.MonitoringCount);
        SetLiveCount(count.LiveCount);
        SetRecCount(count.RecCount);
    }

    /// <summary>
    /// 异步更新硬件资源使用率
    /// </summary>
    public static async Task UpdateHardwareResourceUtilizationRateAsync()
    {
        //窗口隐藏到托盘时跳过（UI不可见）
        if (Desktop.Services.UiActivity.IsBackground)
        {
            return;
        }
        try
        {
            SystemResourceClass systemResourceClass = new();
            if (Core.Config.Core_RunConfig._DesktopRemoteServer || Core.Config.Core_RunConfig._LocalHTTPMode)
            {
                systemResourceClass = await NetWork.Get.GetBodyAsync<SystemResourceClass>(
                    $"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/system/get_system_resources");
            }
            else
            {
                systemResourceClass = Core.Tools.SystemResource.Overview.GetOverview();
            }

            if (systemResourceClass != null)
            {
                int memory = (int)((double)(1 - ((double)systemResourceClass.Memory.Available / (double)systemResourceClass.Memory.Total)) * 100);
                SetMemoryUsageRate(memory);
                //无磁盘信息（无机械盘/WMI查询失败）时HDDInfo可能为空，直接索引会越界
                if (systemResourceClass.HDDInfo != null && systemResourceClass.HDDInfo.Count > 0
                    && int.TryParse(systemResourceClass.HDDInfo[0].Used.Replace("%", ""), out int hdd))
                {
                    SetHardDiskUsageRate(hdd);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warn(nameof(UpdateHardwareResourceUtilizationRateAsync), "更新硬件资源使用率出现错误", ex, false);
        }
    }

    /// <summary>
    /// 更新运行时间统计
    /// </summary>
    public static void UpdateRuntimeStatistics()
    {
        //窗口隐藏到托盘时跳过（UI不可见）
        if (Desktop.Services.UiActivity.IsBackground)
        {
            return;
        }
        try
        {
            TimeSpan t = TimeSpan.FromSeconds(Core.Init.GetRunTime());
            string answer = string.Format("{0:D2}天{1:D2}小时{2:D2}分钟{3:D2}秒",
                       t.Days,
                       t.Hours,
                       t.Minutes,
                       t.Seconds);
            SetRunTime(answer.Replace("00天", "").Replace("00小时", "").Replace("00分钟", ""));
        }
        catch (Exception ex)
        {
            Log.Warn(nameof(UpdateRuntimeStatistics), "更新运行时间出现错误", ex, false);
        }
    }

    private static void Account_LoginFailureEvent(object? sender, EventArgs e)
    {
        string Message = $"警告：登录态已失效！请在设置界面重新扫码登陆";
        SetWarningMessage(Message);
    }
}
