using AngleSharp.Dom;
using AngleSharp.Io;
using Core.LogModule;
using Microsoft.AspNetCore.Components.RenderTree;
using SharpCompress.IO;
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
                        if (IsCookie && RuntimeObject.Account.AccountInformation!=null && RuntimeObject.Account.AccountInformation.State) request.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
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
                        if (IsCookie && RuntimeObject.Account.AccountInformation!=null && RuntimeObject.Account.AccountInformation.State) request.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
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
                        if (IsCookie && RuntimeObject.Account.AccountInformation!=null && RuntimeObject.Account.AccountInformation.State) request.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
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
            public static long GetFileToByte(FileStream fs, string URL, bool IsCookie = false, string referer = "", int maxRetries = 3)
            {
//#if DEBUG
//                Log.Debug(nameof(GetFileToByte), $"发起Get请求，目标:{URL}");
//#endif
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
                        request.Accept = "*/*";
                        if (!string.IsNullOrEmpty(referer)) request.Referer = referer;
                        if (IsCookie && RuntimeObject.Account.AccountInformation!=null && RuntimeObject.Account.AccountInformation.State) request.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                        request.Method = "GET";
                        request.Timeout = 10000; // 10 seconds

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
                                        dataStream.CopyTo(fs);
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
                        Log.Debug(nameof(DownloadFile), $"{ex.Status.ToString()}:{URL}");
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


        }
    }
}
