// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
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
using Desktop.Models;
using Desktop.Views.Windows;
using Masuit.Tools;
using Newtonsoft.Json;
using Wpf.Ui;
using Wpf.Ui.Controls;

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
    }

    public static void UpdatePageCount(int PageCount)
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
    }

    public static void Refresher(object state)
    {
        Application.Current.Dispatcher.Invoke(() => DataSource.RetrieveData.UI_RoomCards.RefreshRoomCards());
    }
    public class SortableObservableCollection<T> : ObservableCollection<T>
    {
        public Func<T, int> SortingSelector { get; set; }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            if (SortingSelector == null || e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset) return;
            var query = this.Select((item, index) => (Item: item, Index: index));
            query = query.OrderBy(tuple => SortingSelector(tuple.Item));
            var map = query.Select((tuple, index) => (OldIndex: tuple.Index, NewIndex: index)).Where(o => o.OldIndex != o.NewIndex);
            using (var enumerator = map.GetEnumerator())
                if (enumerator.MoveNext())
                    Move(enumerator.Current.OldIndex, enumerator.Current.NewIndex);
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
        if(PageComboBox!=null && string.IsNullOrEmpty(PageComboBox.Text) && PageComboBox.Items.Count>0)
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
        if(!string.IsNullOrEmpty(ScreenNameBox.Text))
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
                    MainWindow.SnackbarService.Show("导入房间配置", $"导入完成，所选导入文件成功导入{count.Success}个"+(result>0?"(有{count.Repeat}已存在，跳过导入)":""), ControlAppearance.Secondary, new SymbolIcon(SymbolRegular.ArrowImport20), TimeSpan.FromSeconds(10));
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
    private bool FindRoomListConfigFile(string DirectoryPath,out string configFile)
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
}
