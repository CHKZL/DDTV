// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Desktop.Models;
using Desktop.Views.Windows;
using Masuit.Tools;
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
        Init();
        
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
}
