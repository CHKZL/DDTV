// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Desktop.Models;
using Desktop.Views.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Desktop.Views.Pages;

/// <summary>
/// Interaction logic for DataView.xaml
/// </summary>
public partial class DataPage
{
    public static SortableObservableCollection<DataCard> CardsCollection { get; private set; }

    public static Timer Timer_DataPage;
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
}
