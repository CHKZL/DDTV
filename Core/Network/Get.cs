using AngleSharp.Dom;
using Core.Account;
using Core.LogModule;
using Core.RuntimeObject;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Core.Network
{
    public class Get
    {
        // 单例 HttpClient，复用连接池并忽略证书（保留原行为）
        private static readonly Lazy<HttpClient> _httpClientLazy = new Lazy<HttpClient>(() =>
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                AllowAutoRedirect = true
            };

            var client = new HttpClient(handler, disposeHandler: true)
            {
                // 默认 Timeout 保持一个宽容值；实际每次请求使用 CancellationTokenSource 精确控制 8000ms
                Timeout = TimeSpan.FromSeconds(30)
            };

            return client;
        });

        private static HttpClient _httpClient => _httpClientLazy.Value;

        /// <summary>
        /// Get方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="IsCookie">cookies集合实例</param>
        /// <param name="referer">Referer</param>
        /// <param name="specialheaders">除前面之外的Headers</param>
        /// <param name="IsRid">是否使用w_rid签名</param>
        /// <returns>请求返回体</returns>
        public static string GetBody(string url, bool IsCookie = false, string referer = "", WebHeaderCollection specialheaders = null, string ContentType = "application/x-www-form-urlencoded", int maxAttempts = 3, AccountInformation account = null, bool IsRid = false)
        {
//#if DEBUG
//            Log.Debug(nameof(GetBody), $"发起Get请求，目标:{url}");
//#endif
            string result = "";

            try
            {
                if (url.Contains("getDanmuInfo"))
                {
                    ;
                }

                if (IsRid)
                {
                    url = Core.Network.Methods.User.GetRidURL(url);
                }

                // 每次请求单次超时
                const int singleRequestTimeoutMs = 3000;

                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(singleRequestTimeoutMs)))
                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        try
                        {
                            // 如果 caller 提供了 specialheaders，最终应以 specialheaders 为准（原实现会把 req.Headers = specialheaders 覆盖之前的默认头）
                            if (specialheaders == null)
                            {
                                // 设置默认头（与原代码设置顺序一致，只有在 specialheaders 为空时生效）
                                request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
                                if (!string.IsNullOrEmpty(Config.Core_RunConfig._HTTP_UA))
                                    request.Headers.TryAddWithoutValidation("User-Agent", Config.Core_RunConfig._HTTP_UA);
                                request.Headers.TryAddWithoutValidation("Cache-Control", "max-age=0");

                                if (!string.IsNullOrEmpty(ContentType))
                                {
                                    // GET 上设置 Content-Type 仅为保持兼容（原代码也会设置）
                                    request.Headers.TryAddWithoutValidation("Content-Type", ContentType);
                                }
                            }
                            else
                            {
                                // 将 specialheaders 覆盖到请求头（模拟 req.Headers = specialheaders）
                                foreach (string key in specialheaders.AllKeys)
                                {
                                    string value = specialheaders[key];
                                    // 优先放到 request.Headers，若失败则尝试放到 content headers（尽量兼容各种 header 名）
                                    if (!request.Headers.TryAddWithoutValidation(key, value))
                                    {
                                        if (request.Content == null)
                                            request.Content = new StringContent(string.Empty);
                                        request.Content.Headers.TryAddWithoutValidation(key, value);
                                    }
                                }
                            }

                            // Referer
                            if (!string.IsNullOrEmpty(referer))
                            {
                                try
                                {
                                    request.Headers.Referrer = new Uri(referer);
                                }
                                catch
                                {
                                    // 忽略无效 referer（与原实现行为一致）
                                }
                            }

                            // Cookie 优先级：account -> IsCookie + RuntimeObject.Account.AccountInformation
                            if (account != null)
                            {
                                if (!string.IsNullOrEmpty(account.strCookies))
                                    request.Headers.TryAddWithoutValidation("Cookie", account.strCookies);
                            }
                            else if (IsCookie && RuntimeObject.Account.AccountInformation != null && RuntimeObject.Account.AccountInformation.State)
                            {
                                string str = $"{RuntimeObject.Account.AccountInformation.strCookies}; buvid3={Core.Account.AccountBuvid.CreateUniqueIdentifier()}";
                                request.Headers.TryAddWithoutValidation("Cookie", str);
                            }

                            // 发送请求（同步风格以保证与原方法为同步）
                            using (HttpResponseMessage response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token).GetAwaiter().GetResult())
                            {
                                response.EnsureSuccessStatusCode();

                                // 读取响应体（同步风格）
                                result = response.Content.ReadAsStringAsync(cts.Token).GetAwaiter().GetResult();

                                if (!string.IsNullOrEmpty(result))
                                {
                                    break; // 成功，跳出重试循环
                                }
                                // 如果为空，继续重试（与原实现一致）
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            // 超时或被取消
                            if (IsCookie)
                                Log.Warn(nameof(GetBody), $"{ex.Message}:{url}", null, false);

                            if (attempt == maxAttempts - 1)
                            {
                                if (IsCookie)
                                    Log.Warn(nameof(GetBody), $"重试{maxAttempts}次均失败:{url}");
                                result = string.Empty;
                            }
                            else
                            {
                                Thread.Sleep(300);
                                continue;
                            }
                        }
                        catch (HttpRequestException ex)
                        {
                            // 网络或 HTTP 错误
                            if (IsCookie)
                                Log.Warn(nameof(GetBody), $"{ex.Message}:{url}", null, false);

                            if (attempt == maxAttempts - 1)
                            {
                                if (IsCookie)
                                    Log.Warn(nameof(GetBody), $"重试{maxAttempts}次均失败:{url}");
                                result = string.Empty;
                            }
                            else
                            {
                                Thread.Sleep(300);
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(300);
                            if (IsCookie)
                                Log.Error(nameof(GetBody), $"发起Get请求，目标:【{url}】发生未知错误，详细堆栈:{ex.ToString()}", ex, false);

                            if (attempt == maxAttempts - 1)
                            {
                                result = string.Empty;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    } // using cts/request

                } // attempts
            }
            catch (Exception exOuter)
            {
                // 全局保护，保持原代码的容错风格
                Log.Error(nameof(GetBody), $"GetBody 全局捕获异常: {exOuter}", exOuter, false);
                result = string.Empty;
            }

            return result;
        }
    }
}
