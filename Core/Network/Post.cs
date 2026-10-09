using AngleSharp.Io;
using Core.Account;
using Core.LogModule;
using Core.RuntimeObject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Core.Network
{
    public class Post
    {
        /// <summary>
        /// 常规POST请求的共享client（不带独立Cookie容器时复用，避免每次请求重建TCP/TLS连接并堆积TIME_WAIT；与Get.cs的单例模式一致）
        /// </summary>
        private static readonly Lazy<HttpClient> _sharedClient = new Lazy<HttpClient>(() =>
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                AllowAutoRedirect = true
            };
            return new HttpClient(handler, disposeHandler: false);
        });

        /// <summary>
        /// POST方法
        /// （已现代化为 HttpClient 实现，保持原签名与行为以保证兼容）
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="dic">POST要发送的键值对</param>
        /// <param name="IsCookie">是否使用cookis</param>
        /// <param name="jsondate">POST要发送的文本信息</param>
        /// <param name="contenttype">数据类型</param>
        /// <param name="referer">Referer</param>
        /// <param name="specialheaders">除前面之外的Headers</param>
        /// <returns>请求返回体</returns>
        public static string PostBody(string url, Dictionary<string, string> dic, bool IsCookie = false, string jsondate = "", string contenttype = "application/x-www-form-urlencoded;charset=utf-8", string referer = "", WebHeaderCollection specialheaders = null, int maxAttempts = 3, CookieContainer cookieContainer = null, AccountInformation account = null)
        {
//# if DEBUG
//            Log.Debug(nameof(PostBody), $"发起Post请求，目标:{url}");
//# endif
            string result = "";
            // 保持与原实现近似的超时：8000ms
            const int singleRequestTimeoutMs = 8000;
            // 原逻辑上如果 dic != null 最终发送的是 dic 拼接的内容（原实现先写 jsondate 再写 dic，最终以 dic 为准）
            // 因此我们实现的逻辑：当 dic != null 时发送 dic 内容，否则发送 jsondate 内容

            // 构建要发送的请求内容（优先 dic）
            byte[] contentBytes = null;
            string contentString = null;
            if (dic != null)
            {
                StringBuilder builder = new StringBuilder();
                int i = 0;
                foreach (var item in dic)
                {
                    if (item.Key.Length > 20)
                    {
                        if (i > 0)
                            builder.Append("&");
                        builder.AppendFormat("{0}", System.Web.HttpUtility.UrlEncode(item.Key, Encoding.UTF8));
                        i++;
                    }
                    else
                    {
                        if (i > 0)
                            builder.Append("&");
                        //表单键值必须URL编码，值含&、=、+或非ASCII字符会破坏表单结构
                        builder.AppendFormat("{0}={1}", System.Web.HttpUtility.UrlEncode(item.Key, Encoding.UTF8), System.Web.HttpUtility.UrlEncode(item.Value ?? string.Empty, Encoding.UTF8));
                        i++;
                    }
                }
                contentString = builder.ToString();
                contentBytes = Encoding.UTF8.GetBytes(contentString);
            }
            else
            {
                contentString = jsondate ?? string.Empty;
                contentBytes = Encoding.UTF8.GetBytes(contentString);
            }

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                //带独立Cookie容器的少数路径（登录换票等）每次新建client；常规请求复用静态共享实例
                HttpClient client;
                HttpClient perCallClient = null;
                if (cookieContainer != null)
                {
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                        AllowAutoRedirect = true,
                        UseCookies = true,
                        CookieContainer = cookieContainer
                    };
                    perCallClient = new HttpClient(handler, disposeHandler: true);
                    client = perCallClient;
                }
                else
                {
                    client = _sharedClient.Value;
                }
                try
                {
                    // 我们使用 CancellationTokenSource 来精确控制单次请求超时，而不是依赖 HttpClient.Timeout（和原实现行为一致）
                    using (var cts = new CancellationTokenSource(singleRequestTimeoutMs))
                    {
                        try
                        {
                            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                            {
                                var content = new ByteArrayContent(contentBytes ?? Array.Empty<byte>());

                                //contenttype非空时设置Content-Type（历史版本条件写反导致永远不设置，已修正）
                                if (!string.IsNullOrEmpty(contenttype))
                                {
                                    try
                                    {
                                        content.Headers.ContentType = MediaTypeHeaderValue.Parse(contenttype);
                                    }
                                    catch
                                    {
                                        // 忽略非法contenttype，保持宽松
                                    }
                                }

                                request.Content = content;

                                // referer
                                if (!string.IsNullOrEmpty(referer))
                                {
                                    try
                                    {
                                        request.Headers.Referrer = new Uri(referer);
                                    }
                                    catch { /* 保持兼容：忽略无效 referer */ }
                                }

                                // Cookie 优先级：account -> IsCookie + RuntimeObject.Account.AccountInformation
                                if (account != null)
                                {
                                    if (!string.IsNullOrEmpty(account.strCookies))
                                        request.Headers.TryAddWithoutValidation("Cookie", account.strCookies);
                                }
                                else if (IsCookie && RuntimeObject.Account.AccountInformation != null && RuntimeObject.Account.AccountInformation.State)
                                {
                                    if (!string.IsNullOrEmpty(RuntimeObject.Account.AccountInformation.strCookies))
                                        request.Headers.TryAddWithoutValidation("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                                }

                                // specialheaders：尝试添加到 request headers，如失败则添加到 content headers
                                if (specialheaders != null)
                                {
                                    foreach (string key in specialheaders.AllKeys)
                                    {
                                        string value = specialheaders[key];
                                        if (!request.Headers.TryAddWithoutValidation(key, value))
                                        {
                                            // 某些 header 需要放在 content header
                                            if (!request.Content.Headers.TryAddWithoutValidation(key, value))
                                            {
                                                // 如果还是失败则忽略，保持兼容性
                                            }
                                        }
                                    }
                                }

                                // 发起请求（使用 ResponseHeadersRead 模式以便尽快开始读取流）
                                using (HttpResponseMessage response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token).GetAwaiter().GetResult())
                                {
                                    response.EnsureSuccessStatusCode();

                                    // 读取响应体（同步风格以保持原方法为同步）
                                    var respString = response.Content.ReadAsStringAsync(cts.Token).GetAwaiter().GetResult();
                                    result = respString;

                                    if (!string.IsNullOrEmpty(result))
                                    {
                                        // 成功获取响应，退出重试循环
                                        break;
                                    }
                                    // 如果 result 为空，会进入下一次尝试（与原实现一致）
                                }
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            // 超时或取消
                            Log.Warn(nameof(PostBody), $"{ex.Message}:{url}", null, false);
                        }
                        catch (HttpRequestException ex)
                        {
                            // 网络或 HTTP 层面错误
                            Log.Warn(nameof(PostBody), $"{ex.Message}:{url}", null, false);
                        }
                        catch (Exception ex)
                        {
                            // 保留与原实现一致的错误记录
                            Thread.Sleep(300);
                            Log.Error(nameof(PostBody), $"发起Post请求，目标:【{url}】未知错误，详细堆栈:{ex}", ex, false);
                            // 与原逻辑一样继续重试（如果尚未到达 maxAttempts）
                        }
                        finally
                        {
                            // 客户端在 using 中自动 disposed，handler 也会随之 disposed
                        }

                        // 如果没有达到最大尝试次数，则等待 300ms（与原实现保持一致）
                        if (attempt < maxAttempts - 1 && string.IsNullOrEmpty(result))
                        {
                            Thread.Sleep(300);
                            continue;
                        }
                    } // cts
                }
                finally
                {
                    //仅每次新建的client需要释放；共享client常驻复用
                    perCallClient?.Dispose();
                }

                // 如果已经取得非空结果则跳出外层循环
                if (!string.IsNullOrEmpty(result))
                    break;

                // 如果到达最后一次仍未成功，设置为空字符串并记录（与原实现一致）
                if (attempt == maxAttempts - 1 && string.IsNullOrEmpty(result))
                {
                    Log.Warn(nameof(PostBody), $"重试{maxAttempts}次均失败:{url}");
                    result = string.Empty;
                }
            } // attempts

            return result;
        }
    }
}
