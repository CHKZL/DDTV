using BiliAccount;
using BiliAccount.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Auxiliary;
using System.IO;
using System.Drawing.Imaging;

namespace DDTV_New.window
{
    /// <summary>
    /// BiliLoginWindowQR.xaml 的交互逻辑
    /// </summary>
    public partial class BiliLoginWindowQR : Window
    {
        public BiliLoginWindowQR()
        {
            InitializeComponent();
            ByQRCode.QrCodeStatus_Changed += ByQRCode_QrCodeStatus_Changed;
            ByQRCode.QrCodeRefresh += ByQRCode_QrCodeRefresh;

            LoginQR.Source = ChangeBitmapToImageSource(ByQRCode.LoginByQrCode());
        }
        private void ByQRCode_QrCodeRefresh(Bitmap newQrCode)
        {
            LoginQR.Source = ChangeBitmapToImageSource(newQrCode);
        }

        private void ByQRCode_QrCodeStatus_Changed(ByQRCode.QrCodeStatus status, Account account = null)
        {
            if (account != null)
            {
                Plugin.BilibiliAccount.account = account;
                Console.WriteLine(status);
                //MessageBox.Show("UID:"+account.Uid+",登陆成功");
                MMPU.UID = account.Uid;
                MMPU.写ini配置文件("User", "UID", MMPU.UID, MMPU.BiliUserFile);
                foreach (var item in account.Cookies)
                {
                    MMPU.Cookie = MMPU.Cookie + item.ToString() + ";";
                }
                MMPU.CookieEX = account.Expires_Cookies;
                MMPU.csrf = account.CsrfToken;
                MMPU.写ini配置文件("User", "csrf", MMPU.csrf, MMPU.BiliUserFile);
                MMPU.写ini配置文件("User", "Cookie", MMPU.Cookie, MMPU.BiliUserFile);
                MMPU.写ini配置文件("User", "CookieEX", MMPU.CookieEX.ToString(), MMPU.BiliUserFile);
                Dispatcher.Invoke(new Action(delegate
                {
                    Close();
                }));
            }
         
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static ImageSource ChangeBitmapToImageSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new System.ComponentModel.Win32Exception();
            }
            return wpfBitmap;
        }
    }
}
