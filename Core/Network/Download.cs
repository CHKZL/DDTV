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
using System.Threading.Tasks;
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
                int retries = 0;
                while (retries < maxRetries)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                    try
                    {

                        request.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                        request.ServicePoint.Expect100Continue = false;
                        request.UserAgent = Config.Core_RunConfig._HTTP_UA;
                        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                        if (!string.IsNullOrEmpty(referer)) request.Referer = referer;
                        if (IsCookie && RuntimeObject.Account.AccountInformation != null && RuntimeObject.Account.AccountInformation.State) request.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                        request.Method = "GET";
                        request.Timeout = 1000;

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            using (Stream dataStream = response.GetResponseStream())
                            {
                                using (FileStream fileStream = new FileStream(SavePath, FileMode.Create))
                                {
                                    dataStream.CopyTo(fileStream);
                                }
                            }
                        }
                        return true;
                    }
                    catch (WebException ex)
                    {
                        Log.Debug(nameof(DownloadFile), $"{ex.Status.ToString()}:{URL}");
                        retries++;
                        Thread.Sleep(300);
                    }
                    catch (Exception ex)
                    {
                        retries++;
                        Thread.Sleep(300);
                        Log.Error(nameof(DownloadFile), $"发生未知错误，详细堆栈:{ex.ToString()}", ex, false);
                    }
                    finally
                    {
                        if (request != null) request.Abort();
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
                int retries = 0;
                while (retries < maxRetries)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                    try
                    {

                        request.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                        request.ServicePoint.Expect100Continue = false;
                        request.UserAgent = Config.Core_RunConfig._HTTP_UA;
                        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                        if (!string.IsNullOrEmpty(referer)) request.Referer = referer;
                        if (IsCookie && RuntimeObject.Account.AccountInformation != null && RuntimeObject.Account.AccountInformation.State) request.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                        request.Method = "GET";
                        request.Timeout = 1000;

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            using (Stream dataStream = response.GetResponseStream())
                            {
                                using (FileStream fileStream = new FileStream(SavePath, FileMode.Create))
                                {
                                    dataStream.CopyTo(fileStream);
                                }
                            }
                        }
                        return request;
                    }
                    catch (WebException ex)
                    {
                        Log.Debug(nameof(DownloadFile), $"{ex.Status.ToString()}:{URL}");
                        retries++;
                        Thread.Sleep(300);
                    }
                    catch (Exception ex)
                    {
                        retries++;
                        Thread.Sleep(300);
                        Log.Error(nameof(DownloadFileAsync), $"发生未知错误，详细堆栈:{ex.ToString()}", ex, false);
                    }
                    finally
                    {
                        if (request != null) request.Abort();
                    }
                }
                Log.Debug(nameof(DownloadFileAsync), $"重试{maxRetries}次均失败:{URL}");
                return default;
            }



            public static string GetFileToString(string URL, bool IsCookie = false, string referer = "", int maxRetries = 3)
            {
                //#if DEBUG
                //                Log.Debug(nameof(GetFileToString), $"发起Get请求，目标:{URL}");
                //#endif
                int retries = 0;
                while (retries < maxRetries)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                    try
                    {

                        request.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                        request.ServicePoint.Expect100Continue = false;
                        request.UserAgent = Config.Core_RunConfig._HTTP_UA;
                        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                        if (!string.IsNullOrEmpty(referer)) request.Referer = referer;
                        if (IsCookie && RuntimeObject.Account.AccountInformation != null && RuntimeObject.Account.AccountInformation.State) request.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                        request.Method = "GET";
                        request.Timeout = 1000;

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                            {
                                string B = reader.ReadToEnd();
                                return B;
                            }
                        }

                    }
                    catch (WebException ex)
                    {
                        Log.Debug(nameof(DownloadFile), $"{ex.Status.ToString()}:{URL}");
                        retries++;
                        Thread.Sleep(300);
                    }
                    catch (Exception ex)
                    {
                        retries++;
                        Thread.Sleep(300);
                        Log.Error(nameof(GetFileToString), $"发生未知错误，详细堆栈:{ex.ToString()}", ex);
                    }
                    finally
                    {
                        if (request != null) request.Abort();
                    }
                }
                Log.Debug(nameof(GetFileToString), $"重试{maxRetries}次均失败:{URL}");
                return null;
            }
            /// <summary>
            /// 获取网络byte数据流
            /// </summary>
            /// <param name="URL">目标地址</param>
            /// <param name="IsCookie">是否携带cookie</param>
            /// <param name="referer">默认referer</param>
            /// <param name="maxRetries">重试次数</param>
            /// <returns></returns>
            public static long Abandon_GetFileToByte(FileStream fs, string URL, bool IsCookie = false, string referer = "", int maxRetries = 3)
            {
                //#if DEBUG
                //                Log.Debug(nameof(GetFileToByte), $"发起Get请求，目标:{URL}");
                //#endif
                const int bufferSize = 81920;
                int retries = 0;
                while (retries < maxRetries)
                {
                    HttpWebRequest request = null;
                    try
                    {


                        request = (HttpWebRequest)WebRequest.Create(URL);
                        request.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                        request.ServicePoint.Expect100Continue = false;
                        request.UserAgent = Config.Core_RunConfig._HTTP_UA;
                        request.ContentType = "application/x-www-form-urlencoded";
                        request.Headers.Add("Connection", "keep-alive");
                        request.Accept = "*/*";
                        if (!string.IsNullOrEmpty(referer)) request.Referer = referer;
                        if (IsCookie && RuntimeObject.Account.AccountInformation != null && RuntimeObject.Account.AccountInformation.State) request.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                        request.Method = "GET";
                        request.Timeout = 5000; // 10 seconds


                        int maxAttempts = 3;
                        for (int attempt = 0; attempt < maxAttempts; attempt++)
                        {
                            try
                            {
                                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                {
                                    using (Stream dataStream = response.GetResponseStream())
                                    {
                                        long old = fs.Length;
                                        dataStream.CopyTo(fs, bufferSize);
                                        long n = fs.Length;
                                        return n - old;
                                    }
                                }



                            }

                            catch (WebException ex)
                            {
                                if (attempt == maxAttempts - 1)
                                {
                                    Log.Error(nameof(GetFileToByte), $"获取网络流重试{maxAttempts}次均失败，详细堆栈:{ex.ToString()}", ex, true);
                                    retries++;
                                    break;
                                }
                                else
                                {
                                    Log.Warn(nameof(GetFileToByte), $"获取网络byte流出现错误，进行重试，详细堆栈:{ex.ToString()}", ex, false);
                                    Thread.Sleep(200);
                                }
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        //Log.Debug(nameof(DownloadFile), $"{ex.Status.ToString()}:{URL}");
                        Log.Info(nameof(DownloadFile), $"{ex.Status.ToString()}:{URL}");
                        retries++;
                        if (request != null) request.Abort();
                        Thread.Sleep(300);
                    }
                    catch (Exception ex)
                    {
                        retries++;
                        Thread.Sleep(300);
                        Log.Error(nameof(GetFileToByte), $"发生未知错误，详细堆栈:{ex.ToString()}", ex, false);
                    }
                }
                Log.Debug(nameof(GetFileToByte), $"重试{maxRetries}次均失败:{URL}");
                return 0;
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
                    // 设置默认请求超时时间（例如 3 秒）
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
            /// 同步获取网络byte数据流，并写入文件流（超时1秒，最多重试3次）。
            /// </summary>
            /// <param name="fs">目标文件流</param>
            /// <param name="URL">目标地址</param>
            /// <param name="IsCookie">是否携带cookie</param>
            /// <param name="referer">Referer</param>
            /// <param name="maxRetries">最大重试次数</param>
            /// <returns>写入的字节数</returns>
            public static long GetFileToByte(FileStream fs, string URL, bool IsCookie = false, string referer = "", int maxRetries = 3)
            {
                const int bufferSize = 81920;
                // 1. 将超时时间设置为1秒
                const int requestTimeoutSeconds = 1;

                for (int retries = 0; retries <= maxRetries; retries++)
                {
                    // 使用 CancellationTokenSource 来精确控制单次请求的超时
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(requestTimeoutSeconds)))
                    {
                        try
                        {
                            var requestMessage = new HttpRequestMessage(HttpMethod.Get, URL);

                            if (!string.IsNullOrEmpty(referer))
                                requestMessage.Headers.Referrer = new Uri(referer);

                            if (IsCookie && RuntimeObject.Account.AccountInformation != null && RuntimeObject.Account.AccountInformation.State)
                                requestMessage.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);

                            // 发送同步请求，并传入 cancellation token
                            using (HttpResponseMessage response = _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cts.Token).Result)
                            {
                                response.EnsureSuccessStatusCode(); // 如果HTTP状态码不是成功的，会抛出HttpRequestException

                                long oldPosition = fs.Position;
                                using (Stream dataStream = response.Content.ReadAsStreamAsync(cts.Token).Result)
                                {
                                    // CopyTo是同步方法，它会一直阻塞直到完成或cancellationToken被触发
                                    dataStream.CopyTo(fs, bufferSize);
                                }

                                long bytesWritten = fs.Position - oldPosition;
                                //Log.Debug(nameof(GetFileToByte), $"成功下载 {bytesWritten} 字节。URL: {URL}");
                                return bytesWritten;
                            }
                        }
                        catch (AggregateException ex)
                        {
                            // 解包 AggregateException，获取内部实际的异常
                            Exception innerEx = ex.InnerException ?? ex;

                            // 2. 统一处理可重试的异常
                            if (innerEx is TaskCanceledException)
                            {
                                // 超时或被取消
                                Log.Warn(nameof(GetFileToByte), $"下载超时 (Attempt {retries + 1}/{maxRetries})。URL: {URL}。错误: {innerEx.Message}");
                            }
                            else if (innerEx is HttpRequestException httpEx)
                            {
                                // HTTP错误，如404, 500等
                                Log.Warn(nameof(GetFileToByte), $"HTTP请求失败 (Attempt {retries + 1}/{maxRetries})。URL: {URL}。错误: {httpEx.Message}");
                            }
                            else
                            {
                                // 其他不可重试的严重错误，直接抛出
                                Log.Error(nameof(GetFileToByte), $"发生不可重试的错误。URL: {URL}", innerEx);
                                throw innerEx;
                            }

                            // 如果还没到最大重试次数，则等待后继续循环
                            if (retries < maxRetries)
                            {
                                // 3. 使用指数退避策略进行等待，避免因重试过于频繁给服务器造成压力
                                int delayMilliseconds = (int)Math.Pow(2, retries) * 500; // 等待时间：500ms, 1000ms, 2000ms...
                                Log.Debug(nameof(GetFileToByte), $"将在 {delayMilliseconds}ms 后进行第 {retries + 2} 次重试。");
                                Thread.Sleep(delayMilliseconds);
                            }
                        }
                        catch (Exception ex)
                        {
                            // 捕获非AggregateException类型的异常（虽然在同步调用中较少见，但为了健壮性）
                            Log.Error(nameof(GetFileToByte), $"发生未知错误。URL: {URL}", ex);
                            throw;
                        }
                    }
                }

                // 如果循环结束都没有成功，则抛出最终的失败异常
                string errorMessage = $"重试 {maxRetries} 次后均失败。URL: {URL}";
                Log.Error(nameof(GetFileToByte), errorMessage);
                throw new Exception(errorMessage);
            }
        }
    }
}
