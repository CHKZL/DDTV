using BiliAccount.Linq;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.BilibiliModule.User;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.Log;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDTV_GUI.WPFControl
{
    /// <summary>
    /// LoginQRDialog.xaml 的交互逻辑
    /// </summary>
    public partial class LoginQRDialog
    {
        EventHandler<EventArgs> LoginEndEvent;
        public LoginQRDialog(EventHandler<EventArgs> _,string __)
        {
            InitializeComponent();
            LoginEndEvent = _;
            Text.Content = __;
            ByQRCode.QrCodeStatus_Changed += ByQRCode_QrCodeStatus_Changed;
            ByQRCode.QrCodeRefresh += ByQRCode_QrCodeRefresh; 
            //ByQRCode.LoginByQrCode("#FF000000", "#FFFFFFFF", true).Save("./BiliQR.png", System.Drawing.Imaging.ImageFormat.Png);

            LoginQR.Source = ChangeBitmapToImageSource(ByQRCode.LoginByQrCode());
            
        }

        private void ByQRCode_QrCodeRefresh(Bitmap newQrCode)
        {
            LoginQR.Source = ChangeBitmapToImageSource(newQrCode);
        }

        private void ByQRCode_QrCodeStatus_Changed(ByQRCode.QrCodeStatus status, BiliAccount.Account account = null)
        {
            if (status == ByQRCode.QrCodeStatus.Success)
            {
                BilibiliUserConfig.AccClass = account;
                Log.AddLog(nameof(login), LogClass.LogType.Info, "QR扫码登陆bilibili成功");
                BilibiliUserConfig.account.uid = account.Uid;
                foreach (var item in account.Cookies)
                {
                    BilibiliUserConfig.account.cookie = BilibiliUserConfig.account.cookie + item + ";";
                }
                BilibiliUserConfig.account.ExTime = account.Expires_Cookies;
                BilibiliUserConfig.account.csrf = account.CsrfToken;

                BilibiliUserConfig.WritUserFile();
                //开始房间巡逻
                LoginEndEvent.Invoke(this, EventArgs.Empty);
                DDTV_Core.InitDDTV_Core.ClientAID = CoreConfig.GetValue(CoreConfigClass.Key.ClientAID, Guid.NewGuid().ToString(), CoreConfigClass.Group.Core) + "-" + BilibiliUserConfig.account.uid;
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
