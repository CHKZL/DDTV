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
            public static bool DownloadFile(string URL, string SavePath, bool IsCookie = false, string referer = "")
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
                catch (Exception)
                {
                    return false;
                }
            }

            public static WebClient DownloadFileAsync(string URL, string SavePath, bool IsCookie = false, string referer = "")
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

            public static string GetFileToString(string URL, bool IsCookie = false, string referer = "")
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
            public static byte[] GetFileToByte(string URL, bool IsCookie = false, string referer = "")
            {
                WebClient client = new WebClient();
                client.Headers.Add("UserAgent", Config.Core._HTTP_UA);
                client.Headers.Add("ContentType: application/x-www-form-urlencoded");
                client.Headers.Add("Accept: */*");
                if (!string.IsNullOrEmpty(referer)) client.Headers.Add("Referer", referer);
                if (IsCookie && RuntimeObject.Account.AccountInformation.State) client.Headers.Add("Cookie", RuntimeObject.Account.AccountInformation.strCookies);
                client.Encoding = Encoding.UTF8;
                client.DownloadData(new Uri(URL));
                byte[] data = client.DownloadData(new Uri(URL));
                return data;
            }
        }
    }
}
