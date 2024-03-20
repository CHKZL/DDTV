using Downloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Core.RuntimeObject.Download.Basics;
using static FastExpressionCompiler.ImTools.FHashMap;

namespace Core.RuntimeObject.Download
{
    public class FLV
    {
        /// <summary>
        /// 录制flv_avc制式的flv文件
        /// </summary>
        /// <param name="card">房间卡片信息</param>
        /// <returns>[TaskStatus]任务状态；[FileName]下载成功的文件名</returns>
        public static async Task<(DlwnloadTaskState hlsState, string FileName)> DlwnloadHls_avc_flv(RoomCardClass card)
        {
            DlwnloadTaskState hlsState = DlwnloadTaskState.Default;
            string File = string.Empty;
            await Task.Run(async () =>
            {
                
                InitializeDownload(card,RoomCardClass.TaskType.FLV_AVC);
                string title = Tools.KeyCharacterReplacement.CheckFilenames(RoomInfo.GetTitle(card.UID));
                long roomId = card.RoomId;
                File = $"{Config.Core._RecFileDirectory}{Core.Tools.KeyCharacterReplacement.ReplaceKeyword(card.UID, Core.Config.Core._DefaultFilePathNameFormat)}_original.flv";
                CreateDirectoryIfNotExists(File.Substring(0, File.LastIndexOf('/')));
                Thread.Sleep(5);
                #region 实例化下载对象

                var downloadOpt = new DownloadConfiguration()
                {
                    ChunkCount = 1, // 下载文件的部分数量，默认值为1
                    ParallelDownload = false, // 是否并行下载文件的各个部分，默认值为false
                    MaxTryAgainOnFailover = 3, //最大失败次数   
                    Timeout = 3000,

                };
                downloadOpt.RequestConfiguration = new RequestConfiguration()
                {
                    Accept = "*/*",
                    Headers = new WebHeaderCollection(), // { 你的自定义头 }
                    ContentType = "application/x-www-form-urlencoded",
                    KeepAlive = true, // 默认值为false
                    ProtocolVersion = HttpVersion.Version11, // 默认值为HTTP 1.1
                    Referer="https://www.bilibili.com/",
                    UserAgent = Config.Core._HTTP_UA,
                    //Proxy = new WebProxy()
                    //{
                    //    Address = new Uri("http://YourProxyServer/proxy.pac"),
                    //    UseDefaultCredentials = false,
                    //    Credentials = System.Net.CredentialCache.DefaultNetworkCredentials,
                    //    BypassProxyOnLocal = true
                    //}
                };
                if (RuntimeObject.Account.AccountInformation.State)
                {
                    downloadOpt.RequestConfiguration.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                }
                var downloader = new DownloadService(downloadOpt);
                // 提供有关下载进度的任何信息，
                // 如总块的进度百分比、总速度、
                // 平均速度、总接收字节数和接收字节数组以进行实时流。
                downloader.DownloadProgressChanged += (s, e) =>
                {
                    card.DownInfo.RealTimeDownloadSpe = e.AverageBytesPerSecondSpeed;
                    card.DownInfo.DownloadSize += e.ProgressedByteSize;
                };

                #endregion
                HostClass hostClass = GetFlvHost_avc(card);
                string DlwnloadURL = $"{hostClass.host}{hostClass.base_url}{hostClass.uri_name}{hostClass.extra}";
                LogDownloadStart(card,"FLV");
                Task _stopTask = new Task(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(200);
                        if (card.DownInfo.Unmark || card.DownInfo.IsCut)
                        {
                            if (downloader != null && (downloader.Status == DownloadStatus.Running || downloader.Status == DownloadStatus.Created))
                            {
                                downloader.CancelAsync();
                                break;
                            }
                        }
                        if (downloader.Status == DownloadStatus.Stopped || downloader.Status == DownloadStatus.Completed || downloader.Status == DownloadStatus.Failed)
                        {
                            break;
                        }
                    }
                });
                await downloader.DownloadFileTaskAsync(DlwnloadURL, File);
                hlsState = CheckAndHandleFile(File, ref card);
            });
            return (hlsState, File);
        }


    }
}
