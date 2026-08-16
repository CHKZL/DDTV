using Core.LogModule;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Core.RuntimeObject.Download.Basics;

namespace Core.RuntimeObject.Download
{
    public class FLV
    {
        /// <summary>
        /// 录制flv_avc制式的flv文件
        /// </summary>
        /// <param name="card">房间卡片信息</param>
        /// <returns>[TaskStatus]任务状态；[FileName]下载成功的文件名</returns>
        public static async Task<(DownloadTaskState hlsState, string FileName)> DlwnloadHls_avc_flv(RoomCardClass card)
        {
            DownloadTaskState hlsState = DownloadTaskState.Default;
            string File = string.Empty;
            await Task.Run(async () =>
            {
                InitializeDownload(card, RoomCardClass.TaskType.FLV_AVC);
                long roomId = card.RoomId;
                File = $"{Config.Core_RunConfig._RecFileDirectory}{Core.Tools.KeyCharacterReplacement.ReplaceKeyword($"{Config.Core_RunConfig._DefaultLiverFolderName}/{Core.Config.Core_RunConfig._DefaultDataFolderName}{(string.IsNullOrEmpty(Core.Config.Core_RunConfig._DefaultDataFolderName) ? "" : "/")}{Config.Core_RunConfig._DefaultFileName}", DateTime.Now, card.UID)}_original.flv";
                card.DownInfo.DownloadFileList.CurrentOperationVideoFile = string.Empty;
                CreateDirectoryIfNotExists(File.Substring(0, File.LastIndexOf('/')));
                await Task.Delay(5);
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                long DownloadFileSizeForThisTask = 0;
                long startLiveTime = card.live_time.Value;
                string originalTitle = card.Title.Value;
                List<(long size, DateTime time)> speedValues = new();

                void UpdateSpeed(long downloadSizeForThisCycle)
                {
                    speedValues.Add((downloadSizeForThisCycle, DateTime.Now));
                    while (speedValues.Count >= 10)
                    {
                        speedValues.RemoveAt(0);
                    }
                    //窗口时间跨度太小时不更新速度：DateTime.Now分辨率仅~15.6ms，
                    //网络快时一个时钟滴答内就能读满窗口，按此计算会把速度放大到几百Mbps脱离实际
                    double spanMs = speedValues.Count > 1 ? DateTime.Now.Subtract(speedValues[0].time).TotalMilliseconds : 0;
                    if (spanMs >= 500)
                    {
                        card.DownInfo.RealTimeDownloadSpe = (speedValues.Sum(x => x.size) / spanMs) * 1000;
                    }
                    else if (speedValues.Count <= 1)
                    {
                        card.DownInfo.RealTimeDownloadSpe = 0;
                    }
                    card.DownInfo.DownloadSize += downloadSizeForThisCycle;
                }

                using var client = new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                    AllowAutoRedirect = true,
                })
                {
                    Timeout = TimeSpan.FromSeconds(100),
                };
                client.DefaultRequestHeaders.Add("Accept", "*/*");
                client.DefaultRequestHeaders.Add("Referer", "https://www.bilibili.com/");
                client.DefaultRequestHeaders.Add("User-Agent", Config.Core_RunConfig._HTTP_UA);
                if (RuntimeObject.Account.AccountInformation.State)
                {
                    client.DefaultRequestHeaders.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                }

                string DlwnloadURL = string.Empty;

                if (!GetFlvAvcUrl(card, Config.Core_RunConfig._DefaultResolution, out DlwnloadURL))
                {
                    // FLV 流地址获取失败(房间未开播/暂无 FLV 流)，直接返回，不要拿空 URL 去请求。
                    // 补设 StartTime，避免它停在默认值 UnixEpoch(1970) 经 API/Overview 外露
                    card.DownInfo.StartTime = DateTime.Now;
                    hlsState = DownloadTaskState.NoHLSStreamExists;
                    Log.Info(nameof(DlwnloadHls_avc_flv), $"[{card.Name}({card.RoomId})]未获取到有效的FLV流地址，跳过本次FLV下载");
                    return;
                }

                string F_S = Config.Core_RunConfig._RecFileDirectory + (Config.Core_RunConfig._RecFileDirectory.EndsWith("/") || Config.Core_RunConfig._RecFileDirectory.EndsWith("\\") ? "" : "/") + File.Replace(Config.Core_RunConfig._RecFileDirectory, "").Replace("\\", "/");
                card.DownInfo.DownloadFileList.CurrentOperationVideoFile = F_S;
                LogDownloadStart(card, "FLV");

                int retryCount = 0;
                const int maxRetries = 3;
                //单次流读取超时：ResponseHeadersRead模式下HttpClient.Timeout只覆盖到响应头返回为止，
                //CDN下播后保持TCP连接但不再发数据时，ReadAsync会永久阻塞，必须自己控制读取超时。
                //注意CTS必须在每次重试时新建：已取消的CTS无法通过CancelAfter复活，复用会让后续重试的读取立即失败
                const int FlvReadTimeoutSeconds = 30;

                using (FileStream fs = new FileStream(File, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    while (retryCount < maxRetries)
                    {
                        using var readTimeoutCts = new CancellationTokenSource();
                        try
                        {
                            using var request = new HttpRequestMessage(HttpMethod.Get, DlwnloadURL);
                            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                            response.EnsureSuccessStatusCode();
                            using var stream = await response.Content.ReadAsStreamAsync();
                            byte[] buffer = new byte[81920];

                            while (true)
                            {
                                if (Config.Core_RunConfig._SplitOnTitleChange && !string.IsNullOrEmpty(originalTitle) && originalTitle != card.Title.Value)
                                {
                                    Log.Info(nameof(DlwnloadHls_avc_flv), $"[{card.Name}({card.RoomId})]检测到直播间标题变化[{originalTitle}]→[{card.Title.Value}]，进行切割处理");
                                    hlsState = DownloadTaskState.Success;
                                    return;
                                }

                                if (ShouldFinalizeRecording(card, startLiveTime))
                                {
                                    hlsState = CheckAndHandleFile(File, ref card, card.live_time.Value != startLiveTime);
                                    return;
                                }

                                if (card.RoomCutAccordingToSize > 0 && DownloadFileSizeForThisTask > card.RoomCutAccordingToSize)
                                {
                                    Log.Info(nameof(DlwnloadHls_avc_flv), $"{card.Name}({card.RoomId})触发房间文件大小分割");
                                    hlsState = DownloadTaskState.Success;
                                    return;
                                }
                                if (card.RoomCutAccordingToSize == 0 && Config.Core_RunConfig._CutAccordingToSize > 0 && DownloadFileSizeForThisTask > Config.Core_RunConfig._CutAccordingToSize)
                                {
                                    Log.Info(nameof(DlwnloadHls_avc_flv), $"{card.Name}({card.RoomId})触发全局文件大小分割");
                                    hlsState = DownloadTaskState.Success;
                                    return;
                                }
                                if (card.RoomCutAccordingToTime > 0 && stopWatch.Elapsed.TotalSeconds > card.RoomCutAccordingToTime)
                                {
                                    Log.Info(nameof(DlwnloadHls_avc_flv), $"{card.Name}({card.RoomId})触发房间时间分割");
                                    hlsState = DownloadTaskState.Success;
                                    return;
                                }
                                if (card.RoomCutAccordingToTime == 0 && Config.Core_RunConfig._CutAccordingToTime > 0 && stopWatch.Elapsed.TotalSeconds > Config.Core_RunConfig._CutAccordingToTime)
                                {
                                    Log.Info(nameof(DlwnloadHls_avc_flv), $"{card.Name}({card.RoomId})触发全局时间分割");
                                    hlsState = DownloadTaskState.Success;
                                    return;
                                }

                                int read;
                                //每次读取前重置倒计时，只要持续有数据就不会触发超时
                                readTimeoutCts.CancelAfter(TimeSpan.FromSeconds(FlvReadTimeoutSeconds));
                                try
                                {
                                    read = await stream.ReadAsync(buffer, 0, buffer.Length, readTimeoutCts.Token);
                                }
                                catch (OperationCanceledException) when (readTimeoutCts.IsCancellationRequested)
                                {
                                    Log.Warn(nameof(DlwnloadHls_avc_flv), $"[{card.Name}({card.RoomId})]FLV流超过{FlvReadTimeoutSeconds}秒没有新数据，判定流已中断");
                                    read = 0;
                                }
                                if (read == 0)
                                {
                                    break;
                                }

                                await fs.WriteAsync(buffer, 0, read);
                                DownloadFileSizeForThisTask += read;
                                UpdateSpeed(read);
                            }

                            //流中断后确认是否真的下播：绕过缓存连续确认，防止API失败时缓存残留"开播"状态导致无效重试
                            if (ConfirmStopLive(card.RoomId, card))
                            {
                                //与HLS路径的StopLive处理对齐：收尾时检查文件(过小清理/加入文件列表/触发自动修复)，
                                //不能直接返回StopLive状态，否则正常下播的录像永远不会进入文件列表和修复队列
                                hlsState = CheckAndHandleFile(File, ref card);
                                return;
                            }

                            retryCount++;
                            if (retryCount < maxRetries)
                            {
                                int delayMs = (int)Math.Pow(2, retryCount) * 1000;
                                Log.Info(nameof(DlwnloadHls_avc_flv), $"[{card.Name}({card.RoomId})]FLV流意外中断，{delayMs}ms后第{retryCount}次重试");
                                await Task.Delay(delayMs);
                                if (!GetFlvAvcUrl(card, Config.Core_RunConfig._DefaultResolution, out DlwnloadURL))
                                {
                                    hlsState = DownloadTaskState.NoHLSStreamExists;
                                    break;
                                }
                                continue;
                            }
                        }
                        catch (HttpRequestException ex)
                        {
                            retryCount++;
                            if (retryCount < maxRetries)
                            {
                                int delayMs = (int)Math.Pow(2, retryCount) * 1000;
                                Log.Warn(nameof(DlwnloadHls_avc_flv), $"[{card.Name}({card.RoomId})]FLV下载HTTP错误，{delayMs}ms后第{retryCount}次重试：{ex.Message}");
                                await Task.Delay(delayMs);
                                if (!GetFlvAvcUrl(card, Config.Core_RunConfig._DefaultResolution, out DlwnloadURL))
                                {
                                    hlsState = DownloadTaskState.NoHLSStreamExists;
                                    break;
                                }
                                continue;
                            }
                            Log.Error(nameof(DlwnloadHls_avc_flv), $"[{card.Name}({card.RoomId})]FLV下载HTTP错误，重试耗尽", ex);
                            break;
                        }
                        catch (IOException ex)
                        {
                            retryCount++;
                            if (retryCount < maxRetries)
                            {
                                int delayMs = (int)Math.Pow(2, retryCount) * 1000;
                                Log.Warn(nameof(DlwnloadHls_avc_flv), $"[{card.Name}({card.RoomId})]FLV下载IO错误，{delayMs}ms后第{retryCount}次重试：{ex.Message}");
                                await Task.Delay(delayMs);
                                if (!GetFlvAvcUrl(card, Config.Core_RunConfig._DefaultResolution, out DlwnloadURL))
                                {
                                    hlsState = DownloadTaskState.NoHLSStreamExists;
                                    break;
                                }
                                continue;
                            }
                            Log.Error(nameof(DlwnloadHls_avc_flv), $"[{card.Name}({card.RoomId})]FLV下载IO错误，重试耗尽", ex);
                            break;
                        }
                        catch (TaskCanceledException ex) when (ex.CancellationToken == default || !ex.CancellationToken.IsCancellationRequested)
                        {
                            retryCount++;
                            if (retryCount < maxRetries)
                            {
                                int delayMs = (int)Math.Pow(2, retryCount) * 1000;
                                Log.Warn(nameof(DlwnloadHls_avc_flv), $"[{card.Name}({card.RoomId})]FLV下载超时，{delayMs}ms后第{retryCount}次重试");
                                await Task.Delay(delayMs);
                                if (!GetFlvAvcUrl(card, Config.Core_RunConfig._DefaultResolution, out DlwnloadURL))
                                {
                                    hlsState = DownloadTaskState.NoHLSStreamExists;
                                    break;
                                }
                                continue;
                            }
                            Log.Error(nameof(DlwnloadHls_avc_flv), $"[{card.Name}({card.RoomId})]FLV下载超时，重试耗尽");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(nameof(DlwnloadHls_avc_flv), $"[{card.Name}({card.RoomId})]FLV下载发生未预料错误", ex);
                            break;
                        }
                    }
                }

                // 仅在不是"FLV 流地址无效"时收尾文件；否则保留上面重试 break 设的 NoHLSStreamExists，
                // 让 Basics.cs 的 15 秒节流生效，避免 do-while 立刻重进刷屏(本次改动原本想治的死循环)
                if (hlsState != DownloadTaskState.NoHLSStreamExists)
                {
                    hlsState = CheckAndHandleFile(File, ref card);
                }
                try
                {
                    stopWatch.Stop();
                }
                catch (Exception)
                { }
            });
            return (hlsState, File);
        }


        /// <summary>
        /// 获取avc编码FLV的文件URL
        /// </summary>
        /// <param name="roomCard"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static bool GetFlvAvcUrl(RoomCardClass roomCard,int Definition, out string Url)
        {
            Url = "";
            if (!RoomInfo.GetLiveStatus(roomCard.RoomId))
            {
                return false;
            }
            HostClass hostClass = _GetHost(roomCard.RoomId, "http_stream", "flv", "avc", Definition);
            if (hostClass.Effective)
            {
                Url = $"{hostClass.host}{hostClass.base_url}{hostClass.uri_name}{hostClass.extra}";
                return true;
            }
            return false;
        }

    }
}
