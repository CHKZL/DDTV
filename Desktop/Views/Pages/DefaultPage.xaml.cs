// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Core;
using Desktop.Models;
using System.Collections.ObjectModel;
using System.Windows;
using static Server.WebAppServices.Api.get_system_resources;

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

    public DefaultPage()
    {
        InitializeComponent();
        PageComboBoxItems = new();
        this.DataContext = PageComboBoxItems;

        //更新房间统计
        RoomStatisticsTimer = new Timer(UpdateRoomStatistics, null, 1, 3000);
        //更新硬件使用率
        UpdateHardwareResourceUtilizationRateTimer = new Timer(UpdateHardwareResourceUtilizationRate, null, 1, 60 * 1000);
        //更新运行时长
        UpdateRuntimeStatisticsTimer = new Timer(UpdateRuntimeStatistics, null, 1, 1000);

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
    public static void SetrunningState(string str)
    {
        PageComboBoxItems.runningState = str;
        PageComboBoxItems.OnPropertyChanged("runningState");
    }

    /// <summary>
    /// 更新房间统计
    /// </summary>
    /// <param name="state"></param>
    public static void UpdateRoomStatistics(object state)
    {
        (int MonitoringCount, int LiveCount, int RecCount) count = NetWork.Post.PostBody<(int MonitoringCount, int LiveCount, int RecCount)>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/get_rooms/room_statistics");
        SetMonitoringCount(count.MonitoringCount);
        SetLiveCount(count.LiveCount);
        SetRecCount(count.RecCount);
    }

    /// <summary>
    /// 更新硬件资源使用率
    /// </summary>
    /// <param name="state"></param>
    public static void UpdateHardwareResourceUtilizationRate(object state)
    {
        SystemResourceClass systemResourceClass = NetWork.Get.GetBody<SystemResourceClass>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/system/get_system_resources");
        int memory = (int)((double)(1 - ((double)systemResourceClass.Memory.Available / (double)systemResourceClass.Memory.Total)) * 100);
        SetMemoryUsageRate(memory);
        if (int.TryParse(systemResourceClass.HDDInfo[0].Used.Replace("%", ""), out int hdd))
        {
            SetHardDiskUsageRate(hdd);
        }
    }

    /// <summary>
    /// 更新运行时间统计
    /// </summary>
    /// <param name="state"></param>
    public static void UpdateRuntimeStatistics(object state)
    {
        
        TimeSpan t = TimeSpan.FromMilliseconds(Core.Init.GetRunTime());
         string answer = string.Format("{0:D2}天{1:D2}小时{2:D2}分钟{3:D2}秒",
                    t.Days,
                    t.Hours,
                    t.Minutes,
                    t.Seconds);
        SetRunTime(answer.Replace("00天", "").Replace("00小时", "").Replace("00分钟", ""));
    }
}
