// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Core.LogModule;
using Core.Tools;
using Desktop.Models;
using System.Windows;

namespace Desktop.Views.Pages;

/// <summary>
/// Interaction logic for DashboardPage.xaml
/// </summary>
public partial class ToolsPage
{

    internal static ToolsPageModels toolsPageModels { get; private set; }
    public ToolsPage()
    {
        InitializeComponent();
        toolsPageModels = new();
        this.DataContext = toolsPageModels;
    }

    private void ManualFix_Button_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "录制的视频文件(*.flv, *.mp4)|*.flv;*.mp4";
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            string result = openFileDialog.FileName;
            Task.Run(async () =>
            {
                Transcode transcode = new Transcode();
                try
                {
                    toolsPageModels.FixMessage = "正在修复文件";
                    toolsPageModels.OnPropertyChanged("FixMessage");
                    string before = result;
                    string after = result.Replace(".mp4", "_fix.mp4").Replace(".flv", "_fix.mp4");
                    await transcode.TranscodeAsync(before, after);
                    toolsPageModels.FixMessage = "文件修复完成";
                    toolsPageModels.OnPropertyChanged("FixMessage");
                }
                catch (Exception ex)
                {
                    toolsPageModels.FixMessage = "修复文件发生错误，详情查看日志";
                    toolsPageModels.OnPropertyChanged("FixMessage");
                    Log.Error(nameof(ManualFix_Button_Click), $"手动Fix时出现意外错误，文件:{result}");
                }
            });
        }


    }

	//新按钮对应的事件（MKVToolnix修复）
	private void MKVToolnix_Repair_Button_Click(object sender, RoutedEventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "需要修复时长的视频文件(*.flv,*.mp4)|*.flv;*.mp4";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			string result = openFileDialog.FileName;
			Task.Run(async () =>
			{
				Transcode transcode = new Transcode();
				try
				{
					toolsPageModels.FixTimeMessage = "正在用MKVToolNix修复文件";
					Dispatcher.Invoke(() => FixtimeTextBlock.Text = toolsPageModels.FixTimeMessage);
					toolsPageModels.OnPropertyChanged("FixTimeMessage");
					string before = result;
					string after = System.IO.Path.Combine(
						System.IO.Path.GetDirectoryName(result)!,
						System.IO.Path.GetFileNameWithoutExtension(result) + "_fix.mkv"
					);
					await transcode.FixDurationWithMkvToolnixAsync(before, after);
					toolsPageModels.FixTimeMessage = "MKVToolNix修复完成";
					Dispatcher.Invoke(() => FixtimeTextBlock.Text = toolsPageModels.FixTimeMessage);
					toolsPageModels.OnPropertyChanged("FixTimeMessage");
				}
				catch (Exception ex)
				{
					toolsPageModels.FixTimeMessage = "MKVToolNix修复发生错误，详情查看日志";
					Dispatcher.Invoke(() => FixtimeTextBlock.Text = toolsPageModels.FixTimeMessage);
					toolsPageModels.OnPropertyChanged("FixTimeMessage");
					Log.Error(nameof(MKVToolnix_Repair_Button_Click), $"MKVToolNix修复时出现意外错误，文件:{result}");
				}
			});
		}
	}
}
