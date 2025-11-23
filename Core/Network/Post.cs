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
                        builder.AppendFormat("{0}", item.Key);
                        i++;
                    }
                    else
                    {
                        if (i > 0)
                            builder.Append("&");
                        builder.AppendFormat("{0}={1}", item.Key, item.Value);
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
                // 为了尽可能接近原有每次请求都新建请求对象的行为，这里为每次尝试创建新的 HttpClient（在 handler 中根据 cookieContainer 设置 Cookie 支持）
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                    AllowAutoRedirect = true
                };

                if (cookieContainer != null)
                {
                    handler.UseCookies = true;
                    handler.CookieContainer = cookieContainer;
                }

                using (var client = new HttpClient(handler, disposeHandler: true))
                {
                    // 我们使用 CancellationTokenSource 来精确控制单次请求超时，而不是依赖 HttpClient.Timeout（和原实现行为一致）
                    using (var cts = new CancellationTokenSource(singleRequestTimeoutMs))
                    {
                        try
                        {
                            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                            {
                                // ContentType 的原代码逻辑有一个看起来是反向的判断：
                                // if (string.IsNullOrEmpty(contenttype)) req.ContentType = contenttype;
                                // 为了保证行为不变，我们保留相同的条件（即仅当 contenttype 为空时才设置 ContentType）
                                var content = new ByteArrayContent(contentBytes ?? Array.Empty<byte>());

                                if (string.IsNullOrEmpty(contenttype))
                                {
                                    // 与原实现保持一致（只有在 contenttype 为空时设置）
                                    // 这里可能会抛出格式异常，如果传入非法 contenttype 且为空条件成立，保持原有潜在问题以保证兼容性
                                    try
                                    {
                                        content.Headers.ContentType = MediaTypeHeaderValue.Parse(contenttype ?? string.Empty);
                                    }
                                    catch
                                    {
                                        // 保持原实现的宽松性，忽略解析错误（原实现也不会校验）
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
                } // client

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
