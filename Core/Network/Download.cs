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
            public static bool DownloadFile(string URL, string SavePath, bool IsCookie = false, string referer = "", int maxRetries = 10)
            {
                int retries = 0;
                while (retries < maxRetries)
                {
                    try
                    {
                        WebClient client = new WebClient();
                        client.Headers.Add("User-Agent", Config.Core._HTTP_UA);
                        client.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                        if (!string.IsNullOrEmpty(referer)) client.Headers.Add("Referer", referer);
                        if (IsCookie && RuntimeObject.Account.AccountInformation.State) client.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                        client.Encoding = Encoding.UTF8;
                        client.DownloadFile(new Uri(URL), SavePath);
                        return true;
                    }
                    catch (WebException ex)
                    {
                        switch (ex.Status)
                        {
                            case WebExceptionStatus.Timeout:
                                Log.Warn(nameof(DownloadFile), $"Timeout:{URL}");
                                break;
                            default:
                                Log.Warn(nameof(DownloadFile), $"{ex.Status.ToString()}:{URL}");
                                break;
                        }
                        retries++;
                        Thread.Sleep(100);
                        throw;
                    }
                }
                return false;
            }


            public static WebClient DownloadFileAsync(string URL, string SavePath, bool IsCookie = false, string referer = "", int maxRetries = 10)
            {
                int retries = 0;
                while (retries < maxRetries)
                {
                    try
                    {
                        WebClient client = new WebClient();
                        client.Headers.Add("UserAgent", Config.Core._HTTP_UA);
                        client.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                        if (!string.IsNullOrEmpty(referer)) client.Headers.Add("Referer", referer);
                        if (IsCookie && RuntimeObject.Account.AccountInformation.State) client.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                        client.Encoding = Encoding.UTF8;
                        client.DownloadFileAsync(new Uri(URL), SavePath);
                        return client;
                    }
                    catch (WebException ex)
                    {
                        switch (ex.Status)
                        {
                            case WebExceptionStatus.Timeout:
                                Log.Warn(nameof(DownloadFileAsync), $"Timeout:{URL}");
                                break;
                            default:
                                Log.Warn(nameof(DownloadFileAsync), $"{ex.Status.ToString()}:{URL}");
                                break;
                        }
                        retries++;
                        Thread.Sleep(100);
                        throw;
                    }
                }
                return default;
            }


            public static string GetFileToString(string URL, bool IsCookie = false, string referer = "", int maxRetries = 10)
            {
                int retries = 0;
                while (retries < maxRetries)
                {
                    try
                    {
                        WebClient client = new WebClient();
                        client.Headers.Add("UserAgent", Config.Core._HTTP_UA);
                        client.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                        if (!string.IsNullOrEmpty(referer)) client.Headers.Add("Referer", referer);
                        if (IsCookie && RuntimeObject.Account.AccountInformation.State) client.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                        client.Encoding = Encoding.UTF8;
                        client.DownloadData(new Uri(URL));
                        byte[] data = client.DownloadData(new Uri(URL));
                        string B = Encoding.UTF8.GetString(data);
                        return B;
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
                        Thread.Sleep(100);
                        throw;
                    }
                }
                return default;
            }
            public static byte[] GetFileToByte(string URL, bool IsCookie = false, string referer = "", int maxRetries = 10)
            {
                int retries = 0;
                while (retries < maxRetries)
                {
                    try
                    {
                        WebClient client = new WebClient();
                        client.Headers.Add("UserAgent", Config.Core._HTTP_UA);
                        client.Headers.Add("ContentType: application/x-www-form-urlencoded");
                        client.Headers.Add("Accept: */*");
                        if (!string.IsNullOrEmpty(referer)) client.Headers.Add("Referer", referer);
                        if (IsCookie && RuntimeObject.Account.AccountInformation.State) client.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                        client.Encoding = Encoding.UTF8;
                        byte[] data = client.DownloadData(new Uri(URL));
                        return data;
                    }
                    catch (WebException ex)
                    {
                        switch (ex.Status)
                        {
                            case WebExceptionStatus.Timeout:
                                Log.Warn(nameof(GetFileToByte), $"Timeout:{URL}");
                                break;
                            default:
                                Log.Warn(nameof(GetFileToByte), $"{ex.Status.ToString()}:{URL}");
                                break;
                        }
                        retries++;
                        Thread.Sleep(100);
                        throw;
                    }
                }
                return default;
            }

        }
    }
}
