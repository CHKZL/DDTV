// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Desktop.Models;

namespace Desktop.Views.Pages;

/// <summary>
/// Interaction logic for DataView.xaml
/// </summary>
public partial class ColorPage
{
    public ObservableCollection<DataCard> Cards { get; private set; } = [];

    public ColorPage()
    {
        InitializeData();
    }

    private void InitializeData()
    {
       
    }
}
