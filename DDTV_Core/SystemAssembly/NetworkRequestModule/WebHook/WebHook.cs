using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.NetworkRequestModule.WebHook
{
    public class WebHook
    {
        public static void SendHook(HookType cmd, long uid)
        {
            Task.Run(() =>
            {
                string id = Guid.NewGuid().ToString();
                if (!string.IsNullOrEmpty(ConfigModule.CoreConfig.WebHookUrl))
                {
                    int ReNum = 0;
                    do
                    {
                        try
                        {
                            HttpWebRequest request;
                            request = (HttpWebRequest)WebRequest.Create(ConfigModule.CoreConfig.WebHookUrl);
                            if (!DDTV_Core.SystemAssembly.ConfigModule.CoreConfig.WhetherToEnableProxy)
                            {
                                request.Proxy = null;
                            }
                            request.Method = "POST";
                            request.ContentType = "application/json; charset=UTF-8";
                            request.UserAgent = $"{InitDDTV_Core.InitType}/{InitDDTV_Core.Ver}";
                            string paraUrlCoded = MessageProcessing.Processing(cmd,uid, id);
                            byte[] payload;
                            payload = Encoding.UTF8.GetBytes(paraUrlCoded);
                            request.ContentLength = payload.Length;
                            Stream writer = request.GetRequestStream();
                            writer.Write(payload, 0, payload.Length);
                            writer.Close();
                            HttpWebResponse resp = (HttpWebResponse)request.GetResponse();

                            if ((int)resp.StatusCode >= 200 && (int)resp.StatusCode < 300)
                            {
                                Log.Log.AddLog(nameof(SendHook), Log.LogClass.LogType.Info, $"WebHook信息发送完成:{cmd}-{uid}");
                                try
                                {
                                    if (request != null) request.Abort();
                                }
                                catch (Exception) { }
                                return;
                            }
                        }
                        catch (Exception)
                        {
                        }
                        ReNum++;
                        Thread.Sleep(3 * 1000);
                    } while (ReNum > 3);
                }
            });
        }
       

        public enum HookType
        {
            /// <summary>
            /// 开播
            /// </summary>
            StartLive,
            /// <summary>
            /// 下播
            /// </summary>
            StopLive,
            /// <summary>
            /// 开始录制
            /// </summary>
            StartRec,
            /// <summary>
            /// 录制结束
            /// </summary>
            RecComplete,
            /// <summary>
            /// 录制被取消
            /// </summary>
            CancelRec,
            /// <summary>
            /// 完成转码
            /// </summary>
            TranscodingComplete,
            /// <summary>
            /// 保存弹幕文件完成
            /// </summary>
            SaveDanmuComplete,
            /// <summary>
            /// 保存SC文件完成
            /// </summary>
            SaveSCComplete,
            /// <summary>
            /// 保存礼物文件完成
            /// </summary>
            SaveGiftComplete,
            /// <summary>
            /// 保存大航海文件完成
            /// </summary>
            SaveGuardComplete,
            /// <summary>
            /// 执行Shell命令完成
            /// </summary>
            RunShellComplete,
            /// <summary>
            /// 下载任务成功结束
            /// </summary>
            DownloadEndMissionSuccess,
            /// <summary>
            /// 剩余空间不足
            /// </summary>
            SpaceIsInsufficientWarn,
        }
    }
}
