// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Core;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Desktop.Views.Pages;

/// <summary>
/// Interaction logic for DashboardPage.xaml
/// </summary>
public partial class HistoryPage
{
    public ObservableCollection<Core.RuntimeObject.Detect.History> histories = new();
    public HistoryPage()
    {
        InitializeComponent();
        HistoryView.ItemsSource = histories;
        foreach (var history in Core.RuntimeObject.Detect.histories)
        {
            histories.Add(history);
        }
        Core.RuntimeObject.Detect.RecEndEvent += Detect_RecEndEvent;



    }

    private void Detect_RecEndEvent(object? sender, EventArgs e)
    {
        (string Name, string Time, string Title) = ((string, string, string))sender;
        Core.RuntimeObject.Detect.History _historyPageModels = new()
        {
            Name = Name,
            Time = Time,
            Title = Title
        };
        Dispatcher.Invoke(() =>
        {
            histories.Insert(0, _historyPageModels);
        });
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", Path.GetFullPath(Config.Core_RunConfig._RecFileDirectory));
    }
}
