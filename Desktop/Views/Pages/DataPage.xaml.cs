// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Core;
using Core.LogModule;
using Desktop.Models;
using Desktop.Views.Windows;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;
using static Core.Network.Methods.Follow;
using MenuItem = Wpf.Ui.Controls.MenuItem;

namespace Desktop.Views.Pages;

/// <summary>
/// Interaction logic for DataView.xaml
/// </summary>
public partial class DataPage
{
    public static ObservableCollection<DataCard> CardsCollection { get; private set; }
    public static ObservableCollection<string> PageComboBoxItems { get; private set; }

    public static System.Threading.Timer Timer_DataPage;
    public static int CardType = 0;
    //复选框筛选：null=不筛选，true=已开启，false=未开启
    public static bool? RecConfigFilter;
    public static bool? DanmuConfigFilter;
    public static bool? RemindConfigFilter;
    public static int PageCount = 0;
    public static int PageIndex = 1;
    public static string screen_name = string.Empty;
    public static int Width = 0;
    private static DataPage? CurrentPageInstance;
    /// <summary>
    /// 房间列表页当前是否可见（导航进入/离开时由Loaded/Unloaded维护）。
    /// 页面不可见时3秒定时刷新直接跳过——看不见的数据刷了也是白耗CPU和网络请求
    /// </summary>
    public static bool IsPageVisible { get; private set; } = false;

    public DataPage()
    {
        InitializeComponent();
        CurrentPageInstance = this;
        try
        {
            Init();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"UI初始化出现重大错误，错误堆栈{ex.ToString()}");
        }
        Width = (int)CardsItemsControl.ActualWidth;
        //页面被NavigationCacheMode缓存后实例常驻，导航进入/离开只触发Loaded/Unloaded，据此维护可见性标志
        Loaded += DataPage_Loaded;
        Unloaded += DataPage_Unloaded;
    }

    private void DataPage_Loaded(object sender, RoutedEventArgs e)
    {
        IsPageVisible = true;
        //回到本页时立即刷新一次，避免展示离开期间的过期数据
        RequestImmediateRefresh();
    }

    private void DataPage_Unloaded(object sender, RoutedEventArgs e)
    {
        IsPageVisible = false;
    }

    public async void Init()
    {
        CardsCollection = new ObservableCollection<DataCard>();
        CardsItemsControl.ItemsSource = CardsCollection;
        PageComboBoxItems = new ObservableCollection<string>();
        PageComboBox.ItemsSource = PageComboBoxItems;
        Add_ImportFromFollowList_Menu();
        // 首次加载立即刷新一次，避免等待 Timer
        try
        {
            await DataSource.RetrieveData.UI_RoomCards.RefreshRoomCardsAsync();
        }
        catch (Exception ex)
        {
            Core.LogModule.Log.Error(nameof(Init), "初始化加载房间卡片数据失败", ex);
        }
    }

    /// <summary>
    /// 插入关注列表二级菜单内容
    /// </summary>
    public void Add_ImportFromFollowList_Menu()
    {
        Task.Run(async () =>
        {
            try
            {
                var Groups = Core.RuntimeObject.Follow.GetFollowGroups();
                if (Groups.Count > 0)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        foreach (var item in Groups)
                        {
                            MenuItem childMenuItem = new MenuItem
                            {
                                Header = $"{item.name}({item.Count}个)",
                                Tag = item.tagid
                            };

                            childMenuItem.Click += ImportFromFollowList_ChildMenuItem_Click;
                            ImportFromFollowList_Menu.Items.Add(childMenuItem);
                        }
                    });
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
        Task.Run(async () =>
        {
            try
            {
                MenuItem clickedMenuItem = sender as MenuItem;
                long tagid = 0;
                await Dispatcher.InvokeAsync(() =>
                {
                    tagid = (long)clickedMenuItem.Tag;
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

                List<(long key, int State, string Message)> State = new();

                if (Core.Config.Core_RunConfig._DesktopRemoteServer || Core.Config.Core_RunConfig._LocalHTTPMode)
                {
                    State = await NetWork.Post.PostBody<List<(long key, int State, string Message)>>(
                        $"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/set_rooms/batch_add_room", dic);
                }
                else
                {
                    State = Core.RuntimeObject._Room.BatchAddRooms(string.Join(",", _uidl));
                }

                if (State == null)
                {
                    Log.Warn(nameof(ImportFromFollowList_ChildMenuItem_Click), "调用Core的API[batch_add_room]批量添加房间失败，返回的对象为Null");
                    await Dispatcher.InvokeAsync(() =>
                    {
                        MainWindow.SnackbarService.Show("导入关注列表", $"增加房间失败，如果一直提示该错误，请联系开发者", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.ArrowImport20), TimeSpan.FromSeconds(10));
                    });
                    return;
                }

                int Count = _uidl.Count();
                int Ok = State.Count(item => item.State == 1);
                int Repeat = State.Count(item => item.State == 2);
                int NotPresent = State.Count(item => item.State == 3);
                await Dispatcher.InvokeAsync(() =>
                {
                    MainWindow.SnackbarService.Show("导入关注列表", $"导入完成，所选关注列表成功导入{Ok}个" + (Repeat > 0 ? $",有{Repeat}已存在，跳过导入" : "") + (NotPresent > 0 ? $",有{NotPresent}个关注用户没有开通直播间" : ""), ControlAppearance.Success, new SymbolIcon(SymbolRegular.ArrowImport20), TimeSpan.FromSeconds(10));
                });

                Log.Info(nameof(ImportFromFollowList_ChildMenuItem_Click), $"导入完成，所选关注列表成功导入{Ok}个" + (Repeat > 0 ? $",有{Repeat}已存在，跳过导入" : "") + (NotPresent > 0 ? $",有{NotPresent}个关注用户没有开通直播间" : ""));
            }
            catch (Exception ex)
            {
                Log.Error(nameof(ImportFromFollowList_ChildMenuItem_Click), "导入关注列表二级菜单内容失败", ex);
            }
        });
    }

    public static void UpdatePageCount(int PageCount)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            if (PageComboBoxItems != null)
            {
                PageComboBoxItems.Clear();
                for (int i = 1; i <= PageCount; i++)
                {
                    PageComboBoxItems.Add($"第{i}页");
                }
                PageIndex = 1;
                if (PageCount > 0 && CurrentPageInstance?.PageComboBox != null)
                {
                    CurrentPageInstance.PageComboBox.SelectedIndex = 0;
                    CurrentPageInstance.PageComboBox.Text = "第1页";
                }
            }
        });
    }

    //刷新重入保护（0=空闲，1=刷新进行中）：远程模式网络慢时一次刷新可能超过定时器周期，
    //不加保护会让刷新请求无限堆积、乱序执行
    private static int _refreshing = 0;

    public static void Refresher(object state)
    {
        //窗口隐藏到托盘时跳过刷新（UI不可见，刷了也看不见）
        if (Services.UiActivity.IsBackground)
        {
            return;
        }
        RequestImmediateRefresh();
    }

    /// <summary>
    /// 请求立即刷新一次房间卡片（带重入保护，进行中的刷新会跳过本次请求；页面不可见时直接忽略）
    /// </summary>
    public static void RequestImmediateRefresh()
    {
        //页面不可见时刷新纯属浪费：数据刷出来也没人看，等导航回本页时Loaded会补一次刷新
        if (!IsPageVisible)
        {
            return;
        }
        if (Interlocked.Exchange(ref _refreshing, 1) == 1)
        {
            return;
        }
        _ = RefreshRoomCardsSafeAsync();
    }

    private static async Task RefreshRoomCardsSafeAsync()
    {
        try
        {
            await DataSource.RetrieveData.UI_RoomCards.RefreshRoomCardsAsync();
        }
        catch (Exception ex)
        {
            Core.LogModule.Log.Error(nameof(Refresher), "定时刷新房间卡片数据失败", ex);
        }
        finally
        {
            Interlocked.Exchange(ref _refreshing, 0);
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
        ResetFilterPage();
    }

    private void ConfigFilterCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (sender == RecConfigCheckBox)
        {
            RecConfigFilter = GetConfigFilterValue(RecConfigCheckBox);
        }
        else if (sender == DanmuConfigCheckBox)
        {
            DanmuConfigFilter = GetConfigFilterValue(DanmuConfigCheckBox);
        }
        else if (sender == RemindConfigCheckBox)
        {
            RemindConfigFilter = GetConfigFilterValue(RemindConfigCheckBox);
        }
        ResetFilterPage();
    }

    private void ResetFilterPage()
    {
        PageIndex = 1;
        if (PageComboBox != null && PageComboBox.Items.Count > 0)
        {
            PageComboBox.SelectedIndex = 0;
            PageComboBox.Text = "第1页";
        }
        RequestImmediateRefresh();
    }

    public static bool? GetRecConfigFilter() => RecConfigFilter;
    public static bool? GetDanmuConfigFilter() => DanmuConfigFilter;
    public static bool? GetRemindConfigFilter() => RemindConfigFilter;

    private static bool? GetConfigFilterValue(System.Windows.Controls.CheckBox checkBox)
    {
        // 未勾选表示不筛选，横线表示未开启，勾选表示已开启。
        return checkBox.IsChecked switch
        {
            true => true,
            null => false,
            _ => null
        };
    }

    private void PageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PageComboBox.SelectedIndex < 0)
        {
            return;
        }
        PageIndex = PageComboBox.SelectedIndex + 1;
        try
        {
            // 模板尚未应用时 VisualChildrenCount 为 0，此时 GetChild 会抛 ArgumentOutOfRangeException
            if (VisualTreeHelper.GetChildrenCount(CardsItemsControl) > 0)
            {
                ScrollViewer? scrollViewer = VisualTreeHelper.GetChild(CardsItemsControl, 0) as ScrollViewer;
                scrollViewer?.ScrollToTop();
            }
        }
        catch (Exception)
        {
        }
    }

    private void ScreenName_Click(object sender, RoutedEventArgs e)
    {
        ScreenName();
    }

    private void ScreenName()
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
    private void ImportHistoricalRoomConfiguration_Click(object sender, RoutedEventArgs e)
    {
        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
        DialogResult result = folderBrowserDialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            string DirectoryPath = folderBrowserDialog.SelectedPath;
            if (FindRoomListConfigFile(DirectoryPath, out string JsonPath))
            {
                if (Core.Config.RoomConfig.ImportRoomConfiguration(JsonPath, out (int Total, int Success, int Fail, int Repeat, int NotPresent) count))
                {
                    MainWindow.SnackbarService.Show("导入房间配置", $"导入完成，所选导入文件成功导入{count.Success}个" + (count.Repeat > 0 ? $"(有{count.Repeat}已存在，跳过导入)" : ""), ControlAppearance.Secondary, new SymbolIcon(SymbolRegular.ArrowImport20), TimeSpan.FromSeconds(10));
                    return;
                }
            }
            MainWindow.SnackbarService.Show("导入房间配置", $"导入失败，请确保选择的路径为DDTV的文件夹", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.ArrowImport20), TimeSpan.FromSeconds(5));
        }
    }

    /// <summary>
    /// 查找对应路径有没有房间配置文件
    /// </summary>
    private bool FindRoomListConfigFile(string DirectoryPath, out string configFile)
    {
        string[] searchPaths = { DirectoryPath, $"{DirectoryPath}/bin/Config", $"{DirectoryPath}/Config" };
        configFile = null;

        foreach (string path in searchPaths)
        {
            string filePath = System.IO.Path.Combine(path, "RoomListConfig.json");
            if (System.IO.File.Exists(filePath))
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

    private void ScreenNameBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter))
        {
            ScreenName();
        }
    }
}
