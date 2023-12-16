using Core.LogModule;
using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static FastExpressionCompiler.ImTools.FHashMap;

namespace Core.Network
{
    internal class Download
    {
        public class File
        {
            public static bool DownloadFile(string URL, string SavePath, bool IsCookie = false, string referer = "", int maxRetries = 3)
            {
                int retries = 0;
                while (retries < maxRetries)
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                        request.ServicePoint.Expect100Continue = false;
                        request.UserAgent = Config.Core._HTTP_UA;
                        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                        if (!string.IsNullOrEmpty(referer)) request.Referer = referer;
                        if (IsCookie && RuntimeObject.Account.AccountInformation.State) request.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                        request.Method = "GET";
                        request.Timeout = 10000; // 10 seconds

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
                        switch (ex.Status)
                        {
                            case WebExceptionStatus.Timeout:
                                Log.Warn(nameof(DownloadFile), $"Timeout:{URL}", null, false);
                                break;
                            default:
                                Log.Warn(nameof(DownloadFile), $"{ex.Status.ToString()}:{URL}", null, false);
                                break;
                        }
                        retries++;
                        Thread.Sleep(300);
                    }
                    catch (Exception ex)
                    {
                        retries++;
                        Thread.Sleep(300);
                        Log.Error(nameof(DownloadFile), $"发生未知错误，详细堆栈:{ex.ToString()}", ex, false);
                    }
                }
                Log.Warn(nameof(DownloadFile), $"重试{maxRetries}次均失败:{URL}");
                return false;
            }



            public static HttpWebRequest DownloadFileAsync(string URL, string SavePath, bool IsCookie = false, string referer = "", int maxRetries = 3)
            {
                int retries = 0;
                while (retries < maxRetries)
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                        request.ServicePoint.Expect100Continue = false;
                        request.UserAgent = Config.Core._HTTP_UA;
                        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                        if (!string.IsNullOrEmpty(referer)) request.Referer = referer;
                        if (IsCookie && RuntimeObject.Account.AccountInformation.State) request.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                        request.Method = "GET";
                        request.Timeout = 10000; // 10 seconds

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
                        switch (ex.Status)
                        {
                            case WebExceptionStatus.Timeout:
                                Log.Warn(nameof(DownloadFileAsync), $"Timeout:{URL}", null, false);
                                break;
                            default:
                                Log.Warn(nameof(DownloadFileAsync), $"{ex.Status.ToString()}:{URL}", null, false);
                                break;
                        }
                        retries++;
                        Thread.Sleep(300);
                    }
                    catch (Exception ex)
                    {
                        retries++;
                        Thread.Sleep(300);
                        Log.Error(nameof(DownloadFileAsync), $"发生未知错误，详细堆栈:{ex.ToString()}", ex, false);
                    }
                }
                Log.Warn(nameof(DownloadFileAsync), $"重试{maxRetries}次均失败:{URL}");
                return default;
            }



            public static string GetFileToString(string URL, bool IsCookie = false, string referer = "", int maxRetries = 3)
            {
                int retries = 0;
                while (retries < maxRetries)
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                        request.ServicePoint.Expect100Continue = false;
                        request.UserAgent = Config.Core._HTTP_UA;
                        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                        if (!string.IsNullOrEmpty(referer)) request.Referer = referer;
                        if (IsCookie && RuntimeObject.Account.AccountInformation.State) request.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                        request.Method = "GET";
                        request.Timeout = 10000;

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
                        switch (ex.Status)
                        {
                            case WebExceptionStatus.Timeout:
                                Log.Warn(nameof(GetFileToString), $"Timeout:{URL}");
                                break;
                            default:
                                Log.Warn(nameof(GetFileToString), $"{ex.Status.ToString()}:{URL}");
                                break;
                        }
                        retries++;
                        Thread.Sleep(300);
                    }
                    catch (Exception ex)
                    {
                        retries++;
                        Thread.Sleep(300);
                        Log.Error(nameof(GetFileToString), $"发生未知错误，详细堆栈:{ex.ToString()}", ex);
                    }
                }
                Log.Warn(nameof(GetFileToString), $"重试{maxRetries}次均失败:{URL}");
                return default;
            }
            public static byte[] GetFileToByte(string URL, bool IsCookie = false, string referer = "", int maxRetries = 3)
            {
                int retries = 0;
                while (retries < maxRetries)
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                        request.ServicePoint.Expect100Continue = false;
                        request.UserAgent = Config.Core._HTTP_UA;
                        request.ContentType = "application/x-www-form-urlencoded";
                        request.Accept = "*/*";
                        if (!string.IsNullOrEmpty(referer)) request.Referer = referer;
                        if (IsCookie && RuntimeObject.Account.AccountInformation.State) request.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                        request.Method = "GET";
                        request.Timeout = 10000; // 10 seconds

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            using (Stream dataStream = response.GetResponseStream())
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    dataStream.CopyTo(ms);
                                    return ms.ToArray();
                                }
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        switch (ex.Status)
                        {
                            case WebExceptionStatus.Timeout:
                                Log.Warn(nameof(GetFileToByte), $"Timeout:{URL}", null, false);
                                break;
                            default:
                                Log.Warn(nameof(GetFileToByte), $"{ex.Status.ToString()}:{URL}", null, false);
                                break;
                        }
                        retries++;
                        Thread.Sleep(300);
                    }
                    catch (Exception ex)
                    {
                        retries++;
                        Thread.Sleep(300);
                        Log.Error(nameof(GetFileToByte), $"发生未知错误，详细堆栈:{ex.ToString()}", ex, false);
                    }
                }
                Log.Warn(nameof(GetFileToByte), $"重试{maxRetries}次均失败:{URL}");
                return default;
            }


        }
    }
}
