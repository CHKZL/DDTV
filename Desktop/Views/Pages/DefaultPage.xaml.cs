// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Core;
using Core.LogModule;
using Desktop.Models;
using SixLabors.ImageSharp.Drawing;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using static Server.WebAppServices.Api.get_system_resources;
using Castle.DynamicProxy;

namespace Desktop.Views.Pages;

/// <summary>
/// Interaction logic for DashboardPage.xaml
/// </summary>
public partial class DefaultPage
{
    internal static HomePageModels PageComboBoxItems { get; private set; }
    private Timer RoomStatisticsTimer;
    private Timer UpdateHardwareResourceUtilizationRateTimer;
    private Timer UpdateRuntimeStatisticsTimer;
    private Timer UpdateAnnouncementTimer;
    private Timer ProxyDetectionTimer;
    private Timer IpvDetectionTimer;

    public DefaultPage()
    {
        InitializeComponent();
        PageComboBoxItems = new();
        this.DataContext = PageComboBoxItems;

        //更新房间统计
        RoomStatisticsTimer = new Timer(UpdateRoomStatistics, null, 1, 3000);
        //更新硬件使用率
        UpdateHardwareResourceUtilizationRateTimer = new Timer(UpdateHardwareResourceUtilizationRate, null, 1000, 60 * 1000);
        //更新运行时长
        UpdateRuntimeStatisticsTimer = new Timer(UpdateRuntimeStatistics, null, 1000, 1000);
        //更新公告
        UpdateAnnouncementTimer = new Timer(UpdateAnnouncement, null, 1, 1000 * 60 * 60);
        //代理状态检测
        ProxyDetectionTimer = new Timer(ProxyDetection, null, 1, 1000 * 60 * 30);
        //IP版本检测
        IpvDetectionTimer = new Timer(IpvDetection, null, 1, 1000 * 60 * 30);

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
    /// 更新公告
    /// </summary>
    /// <param name="state"></param>
    public static void UpdateAnnouncement(object state)
    {

        try
        {
            string announcement = Core.Tools.ProgramUpdates.Get("/announcement.txt");
            PageComboBoxItems.announcement = announcement;
            PageComboBoxItems.OnPropertyChanged("announcement");
        }
        catch (Exception ex)
        {
            Log.Warn(nameof(UpdateAnnouncement), "更新公告出现错误，错误堆栈已写文本记录文件", ex, false);
        }
    }
    
    /// <summary>
    /// 检测代理状态
    /// </summary>
    /// <param name="state"></param>
    public static void ProxyDetection(object state)
    {
        try
        {
            var defaultProxy = System.Net.WebRequest.DefaultWebProxy;
            if (defaultProxy != null)
            {
                var proxyUri = defaultProxy.GetProxy(new Uri(Config.Core_RunConfig._LiveDomainName));
                if(proxyUri==null)
                {
                    SetProxyState("正常，未检测到代理");
                    return;
                }
                if (proxyUri?.AbsoluteUri != Config.Core_RunConfig._LiveDomainName)
                {
                    Log.Info(nameof(ProxyDetection), $"系统代理已启用，代理地址：{proxyUri.AbsoluteUri}", false);
                    SetProxyState("异常：检测到代理生效中");
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
    /// 检测IP版本
    /// </summary>
    /// <param name="state"></param>
    public static void IpvDetection(object state)
    {
        try
        {
            string url = Config.Core_RunConfig._LiveDomainName.ToLower().Replace("https://", "").Replace("http://","");
            IPHostEntry hostEntry = Dns.GetHostEntry(url);
            foreach (IPAddress ipAddress in hostEntry.AddressList)
            {
                Socket tempSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                tempSocket.Connect(new IPEndPoint(ipAddress, 80));

                if (tempSocket.Connected)
                {
                    switch (tempSocket.AddressFamily)
                    {
                        case AddressFamily.InterNetwork:
                            SetIpvState("正常，目前使用的IPv4");
                            break;
                        case AddressFamily.InterNetworkV6:
                            SetIpvState("异常，目前使用的IPv6");
                            Log.Info(nameof(IpvDetection), $"当前为IPv6访问状态", false);
                            break;
                    }
                    
                    tempSocket.Close();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warn(nameof(IpvDetection), $"检测IP版本出现错误,{ex.ToString()}", ex, false);
        }
    }

    /// <summary>
    /// 更新房间统计
    /// </summary>
    /// <param name="state"></param>
    public static void UpdateRoomStatistics(object state)
    {

        try
        {
            (int MonitoringCount, int LiveCount, int RecCount) count = NetWork.Post.PostBody<(int MonitoringCount, int LiveCount, int RecCount)>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/get_rooms/room_statistics").Result;
            SetMonitoringCount(count.MonitoringCount);
            SetLiveCount(count.LiveCount);
            SetRecCount(count.RecCount);
        }
        catch (Exception ex)
        {
            Log.Warn(nameof(UpdateRoomStatistics), "更新房间统计出现错误，错误堆栈已写文本记录文件", ex, false);
        }
    }

    /// <summary>
    /// 更新硬件资源使用率
    /// </summary>
    /// <param name="state"></param>
    public static void UpdateHardwareResourceUtilizationRate(object state)
    {
        try
        {
            SystemResourceClass systemResourceClass = NetWork.Get.GetBody<SystemResourceClass>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/system/get_system_resources");
            if (systemResourceClass != null)
            {
                int memory = (int)((double)(1 - ((double)systemResourceClass.Memory.Available / (double)systemResourceClass.Memory.Total)) * 100);
                SetMemoryUsageRate(memory);
                if (int.TryParse(systemResourceClass.HDDInfo[0].Used.Replace("%", ""), out int hdd))
                {
                    SetHardDiskUsageRate(hdd);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warn(nameof(UpdateHardwareResourceUtilizationRate), "更新硬件资源使用率出现错误，错误堆栈已写文本记录文件", ex, false);
        }
    }

    /// <summary>
    /// 更新运行时间统计
    /// </summary>
    /// <param name="state"></param>
    public static void UpdateRuntimeStatistics(object state)
    {
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
            Log.Warn(nameof(UpdateRuntimeStatistics), "更新公告出现错误，错误堆栈已写文本记录文件", ex, false);
        }
    }
}
