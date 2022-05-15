using SkiaSharp;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using static BiliAccount.Core.ByQRCode;

#if NETFRAMEWORK

using System.Windows;

#endif

#if !NETSTANDARD2_0 && !NETCORE3_0

using System.Windows.Media;
using System.Windows.Media.Imaging;


#endif

namespace BiliAccount.Linq
{
    /// <summary>
    /// 二维码登录
    /// </summary>
    public class ByQRCode
    {
        #region Public Delegates

        /// <summary>
        /// 二维码刷新处理程序
        /// </summary>
        /// <param name="newQrCode">新二维码</param>
        public delegate void QrCodeRefresh_Handle(QR_Object newQrCode);

        /// <summary>
        /// 二维码刷新处理程序(仅登录url）
        /// </summary>
        /// <param name="newQrCodeUrl">新二维码</param>
        public delegate void QrCodeUrlRefresh_Handle(string newQrCodeUrl);

        /// <summary>
        /// 二维码登录状态变更处理程序
        /// </summary>
        /// <param name="status">二维码状态</param>
        /// <param name="account">登录成功时有值，账号信息实例</param>
        public delegate void QrCodeStatus_Changed_Handle(QrCodeStatus status, Account account = null);

        #endregion Public Delegates

        #region Public Events

        /// <summary>
        /// 二维码刷新事件
        /// </summary>
        public static event QrCodeRefresh_Handle QrCodeRefresh;

        /// <summary>
        /// 登录Url刷新事件
        /// </summary>
        public static event QrCodeUrlRefresh_Handle QrCodeUrlRefresh;

        /// <summary>
        /// 二维码登录状态变更事件
        /// </summary>
        public static event QrCodeStatus_Changed_Handle QrCodeStatus_Changed;

        #endregion Public Events

        #region Public Enums

        /// <summary>
        /// 二维码登录状态枚举
        /// </summary>
        public enum QrCodeStatus
        {
            #region Public Fields

            /// <summary>
            /// 等待扫描
            /// </summary>
            Wating,

            /// <summary>
            /// 等待确认
            /// </summary>
            Scaned,

            /// <summary>
            /// 登录成功
            /// </summary>
            Success

            #endregion Public Fields
        }

        #endregion Public Enums

        #region Public Methods

        /// <summary>
        /// 取消登录
        /// </summary>
        public static void CancelLogin()
        {
            Core.ByQRCode.CancelLogin();
        }

#if !NETSTANDARD2_0 && !NETCORE3_0

        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="hObject">对象指针</param>
        /// <returns></returns>
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// 获取WPF显示用的ImageSource
        /// </summary>
        /// <param name="qrCodeImage">二维码图片Bitmap</param>
        /// <returns>ImageSource</returns>
        public static ImageSource GetQrCodeImageSource(Bitmap qrCodeImage)
        {
            IntPtr myImagePtr = qrCodeImage.GetHbitmap();     //创建GDI对象，返回指针

            BitmapSource imgsource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(myImagePtr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());  //创建imgSource

            DeleteObject(myImagePtr);

            return imgsource;
        }

#endif

        /// <summary>
        /// 获取登录Url
        /// </summary>
        /// <returns>登录Url</returns>
        /// <exception cref="Exceptions.InvalidColorValue">传入了错误的颜色值</exception>
        public static string LoginByQrCodeUrl()
        {
            return Core.ByQRCode.GetQrcode();
        }

        /// <summary>
        /// 获取二维码
        /// </summary>
        /// <param name="strForeground">前景颜色</param>
        /// <param name="strBackground">背景颜色</param>
        /// <param name="IsBorderVisable">是否使用边框</param>
        /// <returns>二维码位图</returns>
        /// <exception cref="Exceptions.InvalidColorValue">传入了错误的颜色值</exception>
        public static QR_Object LoginByQrCode(string strForeground = "#FF000000", string strBackground = "#FFFFFFFF", bool IsBorderVisable = false)
        {
            Regex reg = new Regex("#[0-9A-Fa-f]{6,8}");
            if (reg.IsMatch(strForeground) && reg.IsMatch(strBackground))
            {
                return LoginByQrCode(ColorTranslator.FromHtml(strForeground), ColorTranslator.FromHtml(strBackground), IsBorderVisable);
            }
               
            else if (!reg.IsMatch(strForeground))
                throw new Exceptions.InvalidColorValue("strForeground");
            else
                throw new Exceptions.InvalidColorValue("strBackground");
        }

        /// <summary>
        /// 获取二维码
        /// </summary>
        /// <param name="Foreground">前景颜色</param>
        /// <param name="Background">背景颜色</param>
        /// <param name="IsBorderVisable">是否使用边框</param>
        /// <returns>二维码位图</returns>
        /// <exception cref="Exceptions.InvalidColorValue">传入了错误的颜色值</exception>
        public static QR_Object LoginByQrCode(System.Drawing.Color Foreground, System.Drawing.Color Background, bool IsBorderVisable = false)
        {
            if (Foreground != Background)
            {
                return GetQrcode(Foreground, Background, IsBorderVisable);
            }
            else
                throw new Exceptions.InvalidColorValue("strForeground and strBackground can not be same!");
        }

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        /// 调起二维码刷新
        /// </summary>
        /// <param name="newQrCode">新二维码</param>
        internal static void RaiseQrCodeRefresh(QR_Object newQrCode)
        {
            QrCodeRefresh?.Invoke(newQrCode);
        }

        /// <summary>
        /// 调起二维码刷新
        /// </summary>
        /// <param name="newQrCodeUrl">新二维码</param>
        internal static void RaiseQrCodeRefresh(string newQrCodeUrl)
        {
            QrCodeUrlRefresh?.Invoke(newQrCodeUrl);
        }

        /// <summary>
        /// 调起二维码登录状态变更
        /// </summary>
        /// <param name="status">二维码状态</param>
        /// <param name="account">登录成功时有值，账号信息实例</param>
        internal static void RaiseQrCodeStatus_Changed(QrCodeStatus status, Account account = null)
        {
            QrCodeStatus_Changed?.Invoke(status, account);
        }

        #endregion Internal Methods
    }
}