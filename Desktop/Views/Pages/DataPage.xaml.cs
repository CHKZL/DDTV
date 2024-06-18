// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Core;
using Core.LogModule;
using Desktop.Models;
using Desktop.Views.Windows;
using Masuit.Tools;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;
using Wpf.Ui;
using Wpf.Ui.Controls;
using static Core.Network.Methods.Follow;
using MenuItem = Wpf.Ui.Controls.MenuItem;

namespace Desktop.Views.Pages;

/// <summary>
/// Interaction logic for DataView.xaml
/// </summary>
public partial class DataPage
{
    public static SortableObservableCollection<DataCard> CardsCollection { get; private set; }
    public static ObservableCollection<string> PageComboBoxItems { get; private set; }

    public static Timer Timer_DataPage;
    public static int CardType = 0;
    public static int PageCount = 0;
    public static int PageIndex = 1;
    public static string screen_name = string.Empty;
    public static int Width = 0;

    public DataPage()
    {
        InitializeComponent();
        try
        {
            //初始化各种page
            Init();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"UI初始化出现重大错误，错误堆栈{ex.ToString()}");
        }
        Width = (int)CardsItemsControl.ActualWidth;
    }

    public void Init()
    {
        CardsCollection = new SortableObservableCollection<DataCard>()
        {
            SortingSelector = card =>
            {
                if (card.Rec_Status) return 1;
                if (card.Live_Status) return 2;
                if (card.Rest_Status) return 3;
                return 4;
            }
        };
        CardsItemsControl.ItemsSource = CardsCollection;
        PageComboBoxItems = new ObservableCollection<string>();
        PageComboBox.ItemsSource = PageComboBoxItems;
        Add_ImportFromFollowList_Menu();
    }

    /// <summary>
    /// 插入关注列表二级菜单内容
    /// </summary>
    public void Add_ImportFromFollowList_Menu()
    {

        Task.Run(() =>
        {
            try
            {
                var Groups = Core.RuntimeObject.Follow.GetFollowGroups();
                if (Groups.Count > 0)
                {
                    foreach (var item in Groups)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MenuItem childMenuItem = new MenuItem
                            {
                                Header = $"{item.name}({item.Count}个)",
                                Tag = item.tagid
                            };

                            childMenuItem.Click += ImportFromFollowList_ChildMenuItem_Click;
                            ImportFromFollowList_Menu.Items.Add(childMenuItem);
                        });
                    }
                }
                Log.Info(nameof(Add_ImportFromFollowList_Menu), "初始化关注列表二级菜单内容成功");
            }
            catch (Exception ex)
            {
                Log.Error(nameof(Add_ImportFromFollowList_Menu), "初始化关注列表二级菜单内容失败", ex);
            }
        });
    }

    // 从二级菜单导入关注列表点击事件
    private void ImportFromFollowList_ChildMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Task.Run(() =>
        {
            try
            {
                MenuItem clickedMenuItem = sender as MenuItem;
                long tagid = 0;
                Dispatcher.Invoke(() =>
                {
                    tagid = (long)clickedMenuItem.Tag; // 获取被点击的菜单项的索引
                });
                int page = 1;
                List<FollowLists.Data> datas = new();
                while (Core.RuntimeObject.Follow.GetFollowLists(Core.RuntimeObject.Account.AccountInformation.Uid, tagid, page, out List<FollowLists.Data> T) != 0)
                {
                    datas.AddRange(T);
                    page++;
                }
                long[] _uidl = new long[datas.Count];
                for (int i = 0; i < datas.Count; i++)
                {
                    _uidl[i] = datas[i].mid;
                }
                Dictionary<string, string> dic = new()
                {
                    {"uids", string.Join(",",_uidl) },
                    {"auto_rec","false" },
                    {"remind","false" },
                    {"rec_danmu","false" },
                };
                List<(long key, int State, string Message)> State = NetWork.Post.PostBody<List<(long key, int State, string Message)>>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/set_rooms/batch_add_room", dic).Result;
                if (State == null)
                {
                    Log.Warn(nameof(ImportFromFollowList_ChildMenuItem_Click), "调用Core的API[batch_add_room]批量添加房间失败，返回的对象为Null，详情请查看Core日志", null, true);
                    Dispatcher.Invoke(() =>
                    {
                        MainWindow.SnackbarService.Show("导入关注列表", $"增加房间失败，如果一直提示该错误，请联系开发者", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.ArrowImport20), TimeSpan.FromSeconds(10));
                    });

                    return;
                }
                else
                {
                    int Count = _uidl.Count();
                    int Ok = State.Count(item => item.State == 1);
                    int Repeat = State.Count(item => item.State == 2);
                    int NotPresent = State.Count(item => item.State == 3);
                    Dispatcher.Invoke(() =>
                    {
                        MainWindow.SnackbarService.Show("导入关注列表", $"导入完成，所选关注列表成功导入{Ok}个" + (Repeat > 0 ? $",有{Repeat}已存在，跳过导入" : "") + (NotPresent > 0 ? $",有{NotPresent}个关注用户没有开通直播间" : ""), ControlAppearance.Success, new SymbolIcon(SymbolRegular.ArrowImport20), TimeSpan.FromSeconds(10));
                    });

                    Log.Info(nameof(ImportFromFollowList_ChildMenuItem_Click), $"导入完成，所选关注列表成功导入{Ok}个" + (Repeat > 0 ? $",有{Repeat}已存在，跳过导入" : "") + (NotPresent > 0 ? $",有{NotPresent}个关注用户没有开通直播间" : ""));
                }
            }
            catch (Exception ex)
            {
                Log.Error(nameof(ImportFromFollowList_ChildMenuItem_Click), "导入关注列表二级菜单内容失败", ex);
            }
        });
    }

    public static void UpdatePageCount(int PageCount)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (PageComboBoxItems != null)
            {
                PageComboBoxItems.Clear();
                for (int i = 1; i <= PageCount; i++)
                {
                    PageComboBoxItems.Add($"第{i}页");
                }
                PageIndex = 1;
            }
        });
    }

    public static void Refresher(object state)
    {
         DataSource.RetrieveData.UI_RoomCards.RefreshRoomCards();
    }
    public class SortableObservableCollection<T> : ObservableCollection<T>
    {
        public Func<T, int> SortingSelector { get; set; }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                base.OnCollectionChanged(e);
                if (SortingSelector == null || e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset) return;
                var query = this.Select((item, index) => (Item: item, Index: index));
                query = query.OrderBy(tuple => SortingSelector(tuple.Item));
                var map = query.Select((tuple, index) => (OldIndex: tuple.Index, NewIndex: index)).Where(o => o.OldIndex != o.NewIndex);
                using (var enumerator = map.GetEnumerator())
                    if (enumerator.MoveNext())
                        Move(enumerator.Current.OldIndex, enumerator.Current.NewIndex);
            });
        }
    }

    private void AddRoomCardForRoomId_Click(object sender, RoutedEventArgs e)
    {
        AddRoom addRoom = new(AddRoom.Mode.RoomNumberMode);
        addRoom.Show();
    }

    private void AddRoomCardForUid_Click(object sender, RoutedEventArgs e)
    {
        AddRoom addRoom = new(AddRoom.Mode.UidNumberMode);
        addRoom.Show();
    }


    private void CardTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CardType = CardTypeComboBox.SelectedIndex;
        if (PageComboBox != null && string.IsNullOrEmpty(PageComboBox.Text) && PageComboBox.Items.Count > 0)
        {
            PageComboBox.SelectedIndex = 0;
            PageComboBox.Text = "第1页";
        }
    }

    private void PageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PageIndex = PageComboBox.SelectedIndex + 1;
        try
        {
            // 获取ScrollViewer的引用
            ScrollViewer? scrollViewer = VisualTreeHelper.GetChild(CardsItemsControl, 0) as ScrollViewer;

            // 滚动到顶部
            scrollViewer?.ScrollToTop();
        }
        catch (Exception)
        {
        }
    }

    private void ScreenName_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ScreenNameBox.Text))
        {
            screen_name = ScreenNameBox.Text;
        }
        else
        {
            screen_name = string.Empty;
        }
    }

    /// <summary>
    /// 导入其他DDTV房间配置
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ImportHistoricalRoomConfiguration_Click(object sender, RoutedEventArgs e)
    {
        // 创建一个FolderBrowserDialog对象
        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

        // 显示对话框
        DialogResult result = folderBrowserDialog.ShowDialog();

        // 检查用户是否点击了“确定”按钮
        if (result == DialogResult.OK)
        {
            //用户选择的文件夹的绝对路径DirectoryInfo
            string DirectoryPath = folderBrowserDialog.SelectedPath;
            if (FindRoomListConfigFile(DirectoryPath, out string JsonPath))
            {
                if (Core.Config.RoomConfig.ImportRoomConfiguration(JsonPath, out (int Total, int Success, int Fail, int Repeat, int NotPresent) count))
                {
                    MainWindow.SnackbarService.Show("导入房间配置", $"导入完成，所选导入文件成功导入{count.Success}个" + (result > 0 ? $"(有{count.Repeat}已存在，跳过导入)" : ""), ControlAppearance.Secondary, new SymbolIcon(SymbolRegular.ArrowImport20), TimeSpan.FromSeconds(10));
                    return;
                }
            }
            MainWindow.SnackbarService.Show("导入房间配置", $"导入失败，请确保选择的路径为DDTV的文件夹", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.ArrowImport20), TimeSpan.FromSeconds(5));
        }
    }

    /// <summary>
    /// 查找对应路径有没有房间配置文件
    /// </summary>
    /// <param name="DirectoryPath">其他版本的DDTV路径</param>
    /// <param name="configFile">如果存在，out string为Josn文件的绝对路径</param>
    /// <returns>是否存在配置文件</returns>
    private bool FindRoomListConfigFile(string DirectoryPath, out string configFile)
    {
        string[] searchPaths = { DirectoryPath, $"{DirectoryPath}/bin/Config", $"{DirectoryPath}/Config" };
        configFile = null;

        foreach (string path in searchPaths)
        {
            string filePath = Path.Combine(path, "RoomListConfig.json");
            if (File.Exists(filePath))
            {
                configFile = filePath;
                return true;
            }
        }

        return false;
    }

    private void CardDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        Width = (int)CardsItemsControl.ActualWidth;
    }
}
