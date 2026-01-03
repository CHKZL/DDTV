using AngleSharp.Dom;
using AngleSharp.Io;
using Core.LogModule;
using Microsoft.AspNetCore.Components.RenderTree;
using SharpCompress.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Core.Network
{
    internal class Download
    {
        public class File
        {
            public static bool DownloadFile(string URL, string SavePath, bool IsCookie = false, string referer = "", int maxRetries = 3)
            {
                //#if DEBUG
                //                Log.Debug(nameof(DownloadFile), $"发起Get请求，目标:{URL}");
                //#endif
                const int requestTimeoutSeconds = 1;
                const int bufferSize = 81920;

                for (int attempt = 0; attempt <= maxRetries; attempt++)
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(requestTimeoutSeconds)))
                    {
                        try
                        {
                            var requestMessage = new HttpRequestMessage(HttpMethod.Get, URL);

                            if (!string.IsNullOrEmpty(referer))
                                requestMessage.Headers.Referrer = new Uri(referer);

                            if (IsCookie && RuntimeObject.Account.AccountInformation != null && RuntimeObject.Account.AccountInformation.State)
                                requestMessage.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);

                            using (HttpResponseMessage response = _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cts.Token).GetAwaiter().GetResult())
                            {
                                response.EnsureSuccessStatusCode();

                                // 确保目标目录存在
                                var directory = Path.GetDirectoryName(SavePath);
                                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                                    Directory.CreateDirectory(directory);

                                using (var fs = new FileStream(SavePath, FileMode.Create, FileAccess.Write, FileShare.None))
                                {
                                    using (Stream contentStream = response.Content.ReadAsStreamAsync(cts.Token).GetAwaiter().GetResult())
                                    {
                                        // 复制并支持取消
                                        contentStream.CopyToAsync(fs, bufferSize, cts.Token).GetAwaiter().GetResult();
                                        }
                                    }
                                }

                            return true;
                        }
                        catch (OperationCanceledException ex)
                        {
                            Log.Warn(nameof(DownloadFile), $"下载超时或被取消 (Attempt {attempt + 1}/{maxRetries})。URL: {URL}。错误: {ex.Message}");
                        }
                        catch (HttpRequestException ex)
                        {
                            Log.Warn(nameof(DownloadFile), $"HTTP 请求失败 (Attempt {attempt + 1}/{maxRetries})。URL: {URL}。错误: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(nameof(DownloadFile), $"发生未知错误 (Attempt {attempt + 1}/{maxRetries})。URL: {URL}，详细堆栈:{ex}", ex, false);
                            // 对未知异常仍然尝试重试（和原实现风格一致）
                        }

                        if (attempt < maxRetries)
                        {
                            int delayMs = (int)Math.Pow(2, attempt) * 200; // 200ms, 400ms, 800ms...
                            Thread.Sleep(delayMs);
                        }
                    }
                }

                Log.Debug(nameof(DownloadFile), $"重试{maxRetries}次均失败:{URL}");
                return false;
            }



            public static HttpWebRequest DownloadFileAsync(string URL, string SavePath, bool IsCookie = false, string referer = "", int maxRetries = 3)
            {
                //#if DEBUG
                //                Log.Debug(nameof(DownloadFileAsync), $"发起Get请求，目标:{URL}");
                //#endif
                // 由于原方法返回 HttpWebRequest（且最终会被 Abort），这里保持签名兼容：
                // 我们在后台启动一个异步下载任务，方法立即返回 default (与旧实现返回 request 后被 Abort 的效果接近)。
                Task.Run(() =>
                {
                    try
                    {
                        // 复用同步实现的逻辑（但作为后台任务运行）
                        DownloadFile(URL, SavePath, IsCookie, referer, maxRetries);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(nameof(DownloadFileAsync), $"后台下载任务发生错误: {URL}", ex, false);
                    }
                });

                return default;
            }



            public static string GetFileToString(string URL, bool IsCookie = false, string referer = "", int maxRetries = 3)
            {
                //#if DEBUG
                //                Log.Debug(nameof(GetFileToString), $"发起Get请求，目标:{URL}");
                //#endif
                const int requestTimeoutSeconds = 1;

                for (int attempt = 0; attempt <= maxRetries; attempt++)
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(requestTimeoutSeconds)))
                    {
                        try
                        {
                            var requestMessage = new HttpRequestMessage(HttpMethod.Get, URL);

                            if (!string.IsNullOrEmpty(referer))
                                requestMessage.Headers.Referrer = new Uri(referer);

                            if (IsCookie && RuntimeObject.Account.AccountInformation != null && RuntimeObject.Account.AccountInformation.State)
                                requestMessage.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);

                            using (HttpResponseMessage response = _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cts.Token).GetAwaiter().GetResult())
                            {
                                response.EnsureSuccessStatusCode();
                                // 强制使用 UTF8（与原实现一致），如果需要自动检测可以调整
                                var bytes = response.Content.ReadAsByteArrayAsync(cts.Token).GetAwaiter().GetResult();
                                return Encoding.UTF8.GetString(bytes);
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            Log.Warn(nameof(GetFileToString), $"获取字符串超时或被取消 (Attempt {attempt + 1}/{maxRetries})。URL: {URL}。错误: {ex.Message}");
                        }
                        catch (HttpRequestException ex)
                        {
                            Log.Warn(nameof(GetFileToString), $"HTTP 请求失败 (Attempt {attempt + 1}/{maxRetries})。URL: {URL}。错误: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(nameof(GetFileToString), $"发生未知错误 (Attempt {attempt + 1}/{maxRetries})。URL: {URL}，详细堆栈:{ex}", ex);
                        }

                        if (attempt < maxRetries)
                        {
                            int delayMs = (int)Math.Pow(2, attempt) * 200;
                            Thread.Sleep(delayMs);
                        }
                    }
                }

                Log.Debug(nameof(GetFileToString), $"重试{maxRetries}次均失败:{URL}");
                return null;
            }

            /// <summary>
            /// 获取网络byte数据流（保留旧签名作为兼容）：等价于调用新的 GetFileToByte 并返回写入字节数。
            /// </summary>
            public static long Abandon_GetFileToByte(FileStream fs, string URL, bool IsCookie = false, string referer = "", int maxRetries = 3)
            {
                // 直接复用新实现，保持行为一致
                return GetFileToByte(fs, URL, IsCookie, referer, maxRetries);
            }


            // 使用 Lazy<T> 确保 HttpClient 实例是线程安全的单例
            private static readonly Lazy<HttpClient> _httpClientLazy = new Lazy<HttpClient>(() =>
            {
                var handler = new HttpClientHandler
                {
                    // 忽略 SSL 证书错误（与你原代码行为一致）
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                    AllowAutoRedirect = true,
                };

                var client = new HttpClient(handler)
                {
                    // 设置默认请求超时时间（但我们在每次请求中使用 CancellationTokenSource 覆盖）
                    Timeout = TimeSpan.FromSeconds(3)
                };

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                if (!string.IsNullOrEmpty(Config.Core_RunConfig._HTTP_UA))
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(Config.Core_RunConfig._HTTP_UA);
                }
                else
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                }

                return client;
            });

            private static HttpClient _httpClient => _httpClientLazy.Value;

            /// <summary>
            /// 同步获取网络byte数据流，并写入文件流（单次请求超时1秒，最多重试maxRetries次）。
            /// 额外：将下载内容同时再保存一份到 "./Temporary/"，文件名为 "yyyyMMddTHHmmssfff.mp4"。
            /// </summary>
            /// <param name="fs">目标文件流</param>
            /// <param name="URL">目标地址</param>
            /// <param name="IsCookie">是否携带cookie</param>
            /// <param name="referer">Referer</param>
            /// <param name="maxRetries">最大重试次数</param>
            /// <returns>写入的字节数</returns>
            public static long GetFileToByte(FileStream fs, string URL, bool IsCookie = false, string referer = "", int maxRetries = 3, string DebugExpInfo = "")
            {
                const int bufferSize = 81920;
                // 1. 将超时时间设置为1秒
                const int requestTimeoutSeconds = 1;

                for (int retries = 0; retries <= maxRetries; retries++)
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(requestTimeoutSeconds)))
                    {
                        try
                        {
                            var requestMessage = new HttpRequestMessage(HttpMethod.Get, URL);

                            if (!string.IsNullOrEmpty(referer))
                                requestMessage.Headers.Referrer = new Uri(referer);

                            if (IsCookie && RuntimeObject.Account.AccountInformation != null && RuntimeObject.Account.AccountInformation.State)
                                requestMessage.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);

                            using (HttpResponseMessage response = _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cts.Token).GetAwaiter().GetResult())
                            {
                                response.EnsureSuccessStatusCode(); // 如果HTTP状态码不是成功的，会抛出HttpRequestException

                                long oldPosition = fs.Position;


                                if (!string.IsNullOrEmpty(DebugExpInfo))
                                {
                                    string tempPath = Path.Combine(Core.Config.Core_RunConfig._TemporaryFileDirectory, DebugExpInfo);

                                    using (Stream dataStream = response.Content.ReadAsStreamAsync(cts.Token).GetAwaiter().GetResult())
                                    {
                                        var buffer = new byte[bufferSize];
                                        int read;
                                        while ((read = dataStream.ReadAsync(buffer, 0, buffer.Length, cts.Token).GetAwaiter().GetResult()) > 0)
                                        {
                                            using (var tempFs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                                            {
                                                fs.WriteAsync(buffer, 0, read, cts.Token).GetAwaiter().GetResult();
                                                tempFs.WriteAsync(buffer, 0, read, cts.Token).GetAwaiter().GetResult();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    using (Stream dataStream = response.Content.ReadAsStreamAsync(cts.Token).GetAwaiter().GetResult())
                                    {
                                        // CopyToAsync 支持 CancellationToken
                                        dataStream.CopyToAsync(fs, bufferSize, cts.Token).GetAwaiter().GetResult();
                                    }
                                }



                                long bytesWritten = fs.Position - oldPosition;
                                //Log.Debug(nameof(GetFileToByte), $"成功下载 {bytesWritten} 字节。URL: {URL}");
                                return bytesWritten;
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            // 超时或被取消
                            Log.Warn(nameof(GetFileToByte), $"下载超时 (Attempt {retries + 1}/{maxRetries})。URL: {URL}。错误: {ex.Message}");
                        }
                        catch (HttpRequestException httpEx)
                        {
                            // HTTP错误，如404, 500等
                            Log.Warn(nameof(GetFileToByte), $"HTTP请求失败 (Attempt {retries + 1}/{maxRetries})。URL: {URL}。错误: {httpEx.Message}");
                        }
                        catch (Exception ex)
                        {
                            // 捕获非上述类型的异常
                            Log.Error(nameof(GetFileToByte), $"发生未知错误 (Attempt {retries + 1}/{maxRetries})。URL: {URL}", ex, false);
                        }

                        // 如果还没到最大重试次数，则等待后继续循环（指数退避）
                        if (retries < maxRetries)
                        {
                            int delayMilliseconds = (int)Math.Pow(2, retries) * 500; // 等待时间：500ms, 1000ms, 2000ms...
                            Log.Debug(nameof(GetFileToByte), $"将在 {delayMilliseconds}ms 后进行第 {retries + 2} 次重试。");
                            Thread.Sleep(delayMilliseconds);
                        }
                    }
                }

                // 如果循环结束都没有成功，则返回0（与原方法在失败时返回0的语义一致）
                Log.Debug(nameof(GetFileToByte), $"重试 {maxRetries} 次后均失败。URL: {URL}");
                return 0;
            }
        
            /// <summary>
            /// 从网络获取byte数据流（单次请求超时1秒，最多重试maxRetries次）。
            /// </summary>
            /// <param name="URL">目标地址</param>
            /// <param name="IsCookie">是否携带cookie</param>
            /// <param name="referer">Referer</param>
            /// <param name="maxRetries">最大重试次数</param>
            /// <returns>写入的字节数</returns>
            public static byte[] GetNetworkByte(string URL, bool IsCookie = false, string referer = "", int maxRetries = 3)
            {
                const int bufferSize = 81920;
                // 1. 将超时时间设置为1秒
                const int requestTimeoutSeconds = 1;

                for (int retries = 0; retries <= maxRetries; retries++)
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(requestTimeoutSeconds)))
                    {
                        try
                        {
                            var requestMessage = new HttpRequestMessage(HttpMethod.Get, URL);

                            if (!string.IsNullOrEmpty(referer))
                                requestMessage.Headers.Referrer = new Uri(referer);

                            if (IsCookie && RuntimeObject.Account.AccountInformation != null && RuntimeObject.Account.AccountInformation.State)
                                requestMessage.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);

                            using (HttpResponseMessage response = _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cts.Token).GetAwaiter().GetResult())
                            {
                                response.EnsureSuccessStatusCode(); // 如果HTTP状态码不是成功的，会抛出HttpRequestException
                                byte[] allData = response.Content.ReadAsByteArrayAsync(cts.Token).GetAwaiter().GetResult();
                                return allData;
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            // 超时或被取消
                            Log.Warn(nameof(GetFileToByte), $"下载超时 (Attempt {retries + 1}/{maxRetries})。URL: {URL}。错误: {ex.Message}");
                        }
                        catch (HttpRequestException httpEx)
                        {
                            // HTTP错误，如404, 500等
                            Log.Warn(nameof(GetFileToByte), $"HTTP请求失败 (Attempt {retries + 1}/{maxRetries})。URL: {URL}。错误: {httpEx.Message}");
                        }
                        catch (Exception ex)
                        {
                            // 捕获非上述类型的异常
                            Log.Error(nameof(GetFileToByte), $"发生未知错误 (Attempt {retries + 1}/{maxRetries})。URL: {URL}", ex, false);
                        }

                        // 如果还没到最大重试次数，则等待后继续循环（指数退避）
                        if (retries < maxRetries)
                        {
                            int delayMilliseconds = (int)Math.Pow(2, retries) * 500; // 等待时间：500ms, 1000ms, 2000ms...
                            Log.Debug(nameof(GetFileToByte), $"将在 {delayMilliseconds}ms 后进行第 {retries + 2} 次重试。");
                            Thread.Sleep(delayMilliseconds);
                        }
                    }
                }

                // 如果循环结束都没有成功，则返回0（与原方法在失败时返回0的语义一致）
                Log.Debug(nameof(GetFileToByte), $"重试 {maxRetries} 次后均失败。URL: {URL}");
                return Array.Empty<byte>();
            }
        
        }
    }
}
