using Core.LogModule;
using Core.Network;
using SkiaSharp;
using SkiaSharp.QrCode;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using Color = System.Drawing.Color;


namespace Core.Account.Kernel
{
    /// <summary>
    /// 通过二维码登录
    /// </summary>
    public class ByQRCode
    {
        #region Private Fields

        /// <summary>
        /// 状态监视器
        /// </summary>
        private static Timer Monitor;

        /// <summary>
        /// 状态监视器的调用计数
        /// </summary>
        private static int MonitorCallCount = 0;

        /// <summary>
        /// 刷新监视器
        /// </summary>
        private static Timer Refresher;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// 取消登录
        /// </summary>
        public static void CancelLogin()
        {
            if (Monitor != null)
            {
                try
                {
                    Monitor.Dispose();
                }
                catch (Exception)
                { }
            }
            if (Refresher != null)
            {
                try
                {
                    Refresher.Dispose();
                }
                catch (Exception)
                { }
            }


        }

        /// <summary>
        /// 获取二维码要包含的登录url
        /// </summary>
        /// <returns>二维码要包含的登录url</returns>
        public static string GetQrcodeUrl()
        {
            return Core.Network.Get.GetBody("https://passport.bilibili.com/x/passport-login/web/qrcode/generate", false, "https://passport.bilibili.com");
        }

        /// <summary>
        /// 获取二维码(只返回登录url）
        /// </summary>
        /// <returns>登录url</returns>
        public static string GetQrcode()
        {
        re:
            string str = GetQrcodeUrl();
            while (string.IsNullOrEmpty(str))
            {
                str = GetQrcodeUrl();
                Thread.Sleep(50);
            }
            GetQrcode_DataTemplete obj = JsonSerializer.Deserialize<GetQrcode_DataTemplete>(str);
            if (obj.code == 0)
            {
                CancelLogin();
                MonitorCallCount = 0;
                Monitor = new Timer(MonitorCallback, obj.data.qrcode_key, 1000, 1000);
                Refresher = new Timer(RefresherCallback, null, 180000, Timeout.Infinite);
            }
            return str;
        }

        /// <summary>
        /// 获取二维码
        /// </summary>
        /// <param name="Foreground">前景颜色</param>
        /// <param name="Background">背景颜色</param>
        /// <param name="IsBorderVisable">是否使用边框</param>
        /// <returns>二维码位图</returns>
        public static QR_Object GetQrcode(Color Foreground, Color Background, bool IsBorderVisable)
        {
            Log.Info(nameof(GetQrcode), $"开始获取Qrcode");
            QR_Object qrCodeImage = new QR_Object();
        re:
            string str = GetQrcodeUrl();
            if (!string.IsNullOrEmpty(str))
            {
                GetQrcode_DataTemplete obj = JsonSerializer.Deserialize<GetQrcode_DataTemplete>(str);
                if (obj.code == 0)
                {
                    // 生成二维码的内容
                    string content = obj.data.url;
                    qrCodeImage.OriginalString = content;
                    //qrCodeImage.OriginalString = content;
                    using (var generator = new SkiaSharp.QrCode.QRCodeGenerator())
                    {
                        // 设置错误校正能力（ECC）级别
                        var qr = generator.CreateQrCode(content, ECCLevel.H);

                        // 创建一个Canvas
                        var info = new SKImageInfo(512, 512);
                        using (var surface = SKSurface.Create(info))
                        {
                            var canvas = surface.Canvas;

                            // 渲染二维码到Canvas
                            canvas.Render(qr, info.Width, info.Height);

                            // 输出到文件

                            using (var image = surface.Snapshot())
                            {
                                var data = image.Encode(SKEncodedImageFormat.Png, 100);


                                //using (var stream = File.OpenWrite(@"QRCode.png"))
                                //{
                                //    data.SaveTo(stream);
                                //}
                                qrCodeImage.SKData = data;

                                //string NM = @"TMPQR" + new Random().Next(1000, 9999);
                                //using (var stream = File.OpenWrite(NM))
                                //{
                                //    data.SaveTo(stream);
                                //    Bitmap bitmap = new Bitmap(NM);
                                //    return bitmap;
                                //}
                            }





                        }
                    }


                    //QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    //QRCodeData qrCodeData = qrGenerator.CreateQrCode(strCode, QRCodeGenerator.ECCLevel.Q);
                    //QRCode qrcode = new QRCode(qrCodeData);
                    ////生成二维码位图
                    //qrCodeImage = qrcode.GetGraphic(5, Foreground, Background, null, 0, 6, IsBorderVisable);

                    //qrCodeImage.MakeTransparent(Background);

                    //if (Background.A != 0)
                    //{
                    //    for (int x = 0; x < qrCodeImage.Width; x++)
                    //    {
                    //        for (int y = 0; y < qrCodeImage.Height; y++)
                    //        {
                    //            if (qrCodeImage.GetPixel(x, y).ToArgb() == 0)
                    //            {
                    //                qrCodeImage.SetPixel(x, y, Background);
                    //            }
                    //        }
                    //    }
                    //}
                    CancelLogin();
                    MonitorCallCount = 0;
                    Monitor = new Timer(MonitorCallback, obj.data.qrcode_key, 1000, 1000);
                    Refresher = new Timer(RefresherCallback, new List<object> { Foreground, Background, IsBorderVisable }, 180000, Timeout.Infinite);
                }
            }
            else goto re;

            return qrCodeImage;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// 状态监视器回调
        /// </summary>
        /// <param name="o">oauthKey</param>
        private static void MonitorCallback(object o)
        {
            MonitorCallCount++;
            if (MonitorCallCount > 30 && MonitorCallCount < 60)
            {
                Monitor.Change(1000, 3000);
                MonitorCallCount = 60;
            }
            else if (MonitorCallCount > 90 && MonitorCallCount < 120)
            {
                Monitor.Change(1000, 5000);
                MonitorCallCount = 120;
            }
            string qrcode_key = o.ToString();
            string str = Get.GetBody($"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={qrcode_key}");
            if (!string.IsNullOrEmpty(str))
            {
                MonitorCallBack_Templete obj = JsonSerializer.Deserialize<MonitorCallBack_Templete>(str);

                switch (obj.data.code)
                {
                    //确认登陆
                    case 0:
                        //关闭监视器
                        Monitor.Dispose();
                        Refresher.Dispose();
                        AccountInformation account = new AccountInformation();
                        CookieBack_Templete cookies = new()
                        {
                            refresh_token = obj.data.refresh_token,
                            timestamp = obj.data.timestamp,
                            url = obj.data.url
                        };
                        string Querystring = cookies.url.Split('?')[1];
                        string[] KeyValuePair = Regex.Split(Querystring, "&");
                        account.Cookies = new CookieCollection();
                        for (int i = 0; i < KeyValuePair.Length - 1; i++)
                        {
                            string[] tmp = Regex.Split(KeyValuePair[i], "=");
                            switch (tmp[0])
                            {
                                case "bili_jct":
                                    account.CsrfToken = tmp[1];
                                    account.strCookies += KeyValuePair[i] + "; ";
                                    account.Cookies.Add(new Cookie(tmp[0], tmp[1]) { Domain = ".bilibili.com" });
                                    break;
                                case "DedeUserID":
                                    account.Uid = tmp[1];
                                    account.strCookies += KeyValuePair[i] + "; ";
                                    account.Cookies.Add(new Cookie(tmp[0], tmp[1]) { Domain = ".bilibili.com" });
                                    break;
                                case "Expires":
                                    account.Expires_Cookies = DateTime.Parse("1970-01-01 08:00:00").AddSeconds(double.Parse(tmp[1]));
                                    break;
                                case "gourl":
                                    break;

                                default:
                                    account.strCookies += KeyValuePair[i] + "; ";
                                    account.Cookies.Add(new Cookie(tmp[0], tmp[1]) { Domain = ".bilibili.com" });
                                    break;
                            }
                        }
                        account.strCookies = account.strCookies.Substring(0, account.strCookies.Length - 2);
                        account.LoginStatus = AccountInformation.LoginStatusEnum.ByQrCode;
                        Linq.ByQRCode.RaiseQrCodeStatus_Changed(Linq.ByQRCode.QrCodeStatus.Success, account);
                        OperationQueue.Add(Opcode.Account.ScanCodeConfirmation, "扫码登陆确认");
                        return;
                    //已扫描
                    case 86090:
                        Linq.ByQRCode.RaiseQrCodeStatus_Changed(Linq.ByQRCode.QrCodeStatus.Scaned);
                        OperationQueue.Add(Opcode.Account.ScannedCodeWaitingForConfirmation, "已扫码等待确认登陆");
                        break;
                    //未扫描
                    case 86101:
                        Linq.ByQRCode.RaiseQrCodeStatus_Changed(Linq.ByQRCode.QrCodeStatus.Wating);
                        OperationQueue.Add(Opcode.Account.QrCodeWaitingForScann, "二维码等待扫码");
                        break;
                    //二维码过期
                    case 86038:
                        Linq.ByQRCode.RaiseQrCodeStatus_Changed(Linq.ByQRCode.QrCodeStatus.Overdue);
                        OperationQueue.Add(Opcode.Account.QrCodeExpir, "二维码已过期");
                        break;
                }
            }
        }

        /// <summary>
        /// 刷新监视器回调
        /// </summary>
        /// <param name="state"></param>
        public static void RefresherCallback(object state)
        {
            if (state == null)
                Linq.ByQRCode.RaiseQrCodeRefresh(GetQrcode());
            else
                Linq.ByQRCode.RaiseQrCodeRefresh(GetQrcode((Color)((List<object>)state)[0], (Color)((List<object>)state)[1], (bool)((List<object>)state)[2]));
        }

        #endregion Private Methods

        public class QR_Object
        {
            /// <summary>
            /// 二维码结构体
            /// </summary>
            public SKData SKData { get; set; }
            /// <summary>
            /// 二维码Bitmap对象
            /// </summary>
            public SKBitmap SKBitmap { get; set; }
            /// <summary>
            /// 用于生成二维码的url
            /// </summary>
            public string OriginalString { get; set; }
        }

        #region Private Classes
        /// <summary>
        /// 获取二维码的数据模板
        /// </summary>
        private class GetQrcode_DataTemplete
        {
            public int code { set; get; } = -1;
            public Data_Templete data { set; get; }
            public class Data_Templete
            {
                public string qrcode_key { set; get; }
                public string url { set; get; }
            }
        }

        /// <summary>
        /// 状态监视器回调数据模板
        /// </summary>
        private class MonitorCallBack_Templete
        {
            #region Public Fields

            public Data data { set; get; }
            public int code { set; get; }

            public class Data
            {
                public string url { set; get; }
                public string refresh_token { set; get; }
                public long timestamp { set; get; }
                public int code { set; get; }
                public string message { set; get; }
            }
            #endregion Public Fields
        }

        /// <summary>
        /// 登陆完成返回cookie回调数据模版
        /// </summary>
        private class CookieBack_Templete
        {
            public string url { set; get; }
            public string refresh_token { set; get; }
            public long timestamp { set; get; }
        }

        #endregion Private Classes
    }
}