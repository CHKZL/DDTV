// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Core.LogModule;
using Core.Tools;
using Desktop.Models;
using System.IO;
using System.Net.Http;
using System.Windows;

namespace Desktop.Views.Pages;

/// <summary>
/// Interaction logic for DashboardPage.xaml
/// </summary>
public partial class ToolsPage
{

	// mkvmerge.exe 目标路径
	private readonly string mkvmergePath = System.IO.Path.Combine(
		AppDomain.CurrentDomain.BaseDirectory, "Plugins", "MKVToolNix", "mkvmerge.exe");
	// 下载地址
	private readonly string mkvmergeUrl = "https://dl.ddtv-update.top/plugins/MKVToolnix/mkvmerge.exe";

	internal static ToolsPageModels toolsPageModels { get; private set; }
    public ToolsPage()
    {
        InitializeComponent();
        toolsPageModels = new();
        this.DataContext = toolsPageModels;
		CheckMkvmergeExists();
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
	/// <summary>
	/// 检查mkvmerge.exe是否存在，决定按钮显示
	/// </summary>
	private void CheckMkvmergeExists()
	{
		if (File.Exists(mkvmergePath))
		{
			DownloadMkvmergeButton.Visibility = Visibility.Collapsed;
			MKVToolNixFixButton.Visibility = Visibility.Visible;
			FixtimeTextBlock.Visibility = Visibility.Visible;
		}
		else
		{
			DownloadMkvmergeButton.Visibility = Visibility.Visible;
			MKVToolNixFixButton.Visibility = Visibility.Collapsed;
			FixtimeTextBlock.Visibility = Visibility.Collapsed;
		}
	}
	/// <summary>
	/// 下载mkvmerge.exe按钮点击事件（加了一个进度环一个文字显示，点击后直接隐藏下载按钮）
	/// </summary>
	private async void DownloadMkvmerge_Button_Click(object sender, RoutedEventArgs e)
	{
		// 禁用并隐藏下载按钮，防止重复点击
		DownloadMkvmergeButton.IsEnabled = false;
		DownloadMkvmergeButton.Visibility = Visibility.Collapsed;

		// 显示进度环和进度文本，初始为不确定状态
		Dispatcher.Invoke(() =>
		{
			MkvmergeDownloadProgressRing.Visibility = Visibility.Visible;
			MkvmergeDownloadProgressRing.IsIndeterminate = true;
			MkvmergeDownloadProgressRing.Progress = 0;
			MkvmergeDownloadProgressText.Visibility = Visibility.Visible;
			MkvmergeDownloadProgressText.Text = "下载进度：0%";
		});

		try
		{
			// 确保目标目录存在
			Directory.CreateDirectory(System.IO.Path.GetDirectoryName(mkvmergePath)!);

			// 发起 HTTP 请求，获取响应流
			using var httpClient = new HttpClient();
			using var response = await httpClient.GetAsync(mkvmergeUrl, HttpCompletionOption.ResponseHeadersRead);
			response.EnsureSuccessStatusCode();

			// 获取文件总长度，用于判断是否可以显示具体进度
			var total = response.Content.Headers.ContentLength ?? -1L;
			var canReportProgress = total > 0;

			// 如果可以获取总长度，切换为确定进度模式
			if (canReportProgress)
			{
				Dispatcher.Invoke(() =>
				{
					MkvmergeDownloadProgressRing.IsIndeterminate = false;
					MkvmergeDownloadProgressRing.Progress = 0;
				});
			}

			using var stream = await response.Content.ReadAsStreamAsync();
			using var fileStream = new FileStream(mkvmergePath, FileMode.Create, FileAccess.Write, FileShare.None);

			var buffer = new byte[81920];
			long totalRead = 0;
			int read;
			// 循环读取并写入文件，同时更新进度
			while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
			{
				await fileStream.WriteAsync(buffer, 0, read);
				totalRead += read;
				if (canReportProgress)
				{
					double percent = (double)totalRead / total * 100;
					// 在UI线程上更新进度环和进度文本
					Dispatcher.Invoke(() =>
					{
						MkvmergeDownloadProgressRing.Progress = percent;
						MkvmergeDownloadProgressText.Text = $"下载进度：{percent:F0}%";
					});
				}
			}

			// 下载完成，显示100%
			Dispatcher.Invoke(() =>
			{
				MkvmergeDownloadProgressRing.Progress = 100;
				MkvmergeDownloadProgressText.Text = "下载进度：100%";
			});
			await Task.Delay(500);

			// 隐藏进度环和进度文本
			Dispatcher.Invoke(() =>
			{
				MkvmergeDownloadProgressRing.Visibility = Visibility.Collapsed;
				MkvmergeDownloadProgressText.Visibility = Visibility.Collapsed;
			});
			// 检查并切换按钮显示状态
			CheckMkvmergeExists();
		}
		catch (Exception ex)
		{
			// 下载失败，恢复按钮并提示
			Dispatcher.Invoke(() =>
			{
				MkvmergeDownloadProgressRing.Visibility = Visibility.Collapsed;
				MkvmergeDownloadProgressText.Visibility = Visibility.Collapsed;
				DownloadMkvmergeButton.Visibility = Visibility.Visible;
				DownloadMkvmergeButton.IsEnabled = true;
				DownloadMkvmergeButton.Content = "下载失败，点击重试";
			});
			MessageBox.Show("下载mkvmerge.exe失败：" + ex.Message, "下载失败", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
