// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Desktop.Models;
using Wpf.Ui.Controls;

namespace Desktop.Views.Pages;

/// <summary>
/// Interaction logic for DataView.xaml
/// </summary>
public partial class DataPage
{
    public static ObservableCollection<DataCard> CardsCollection { get; private set; } = [];
    public static Timer T_R;
    public DataPage()
    {
        InitializeData();
        InitializeComponent();

        CardsItemsControl.ItemsSource = CardsCollection;
    }

    private void InitializeData()
    {
        //DataSource.RetrieveData.UI_RoomCards.RefreshRoomCards();
    }
    public static void Refresher(object state)
    {
        Application.Current.Dispatcher.Invoke(() => DataSource.RetrieveData.UI_RoomCards.RefreshRoomCards());
    }
}
