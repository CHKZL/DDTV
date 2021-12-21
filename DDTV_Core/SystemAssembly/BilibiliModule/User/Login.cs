using BiliAccount;
using BiliAccount.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DDTV_Core.SystemAssembly.ConfigModule;

namespace DDTV_Core.SystemAssembly.BilibiliModule.User
{
    public class login
    {
        public class VerifyLogin
        {
            public static bool Loing(InitDDTV_Core.SatrtType satrtType)
            {
               QR.QRInit(satrtType);
                do
                {
                    if (string.IsNullOrEmpty(BilibiliUserConfig.account.csrf)||string.IsNullOrEmpty(BilibiliUserConfig.account.uid)||string.IsNullOrEmpty(BilibiliUserConfig.account.cookie)||BilibiliUserConfig.account.ExTime<DateTime.UtcNow)
                    {
                        switch (satrtType)
                        {
                            case InitDDTV_Core.SatrtType.DDTV_Core:
                                Log.Log.AddLog(nameof(login), Log.LogClass.LogType.Info, "等待登陆中，请用bilibili手机客户端扫描[./BiliQR.png]进行登录");
                                break;
                            case InitDDTV_Core.SatrtType.DDTV_CLI:
                                Log.Log.AddLog(nameof(login), Log.LogClass.LogType.Info, "等待登陆中，访问"+"https://本设备IP或域名:端口/loginqr"+"，查看二维码，或打开DDTV根目录中生成的[./BiliQR.png]文件，并使用bilibili手机客户端扫描进行登录");
                                break;
                            case InitDDTV_Core.SatrtType.DDTV_GUI:
                                return false;
                            break;
                            case InitDDTV_Core.SatrtType.DDTV_WEB:
                                Log.Log.AddLog(nameof(login), Log.LogClass.LogType.Info, "等待登陆中，访问" + "[http://本设备IP地址:11419/api/loginqr]查看二维码，或打开DDTV根目录中生成的[./BiliQR.png]文件，并使用bilibili手机客户端扫描进行登录");
                                break;
                            default:
                                break;
                        }    
                    Thread.Sleep(5000);
                    }
                } while (string.IsNullOrEmpty(BilibiliUserConfig.account.cookie));
                return true;
            }
        }
        public class QR
        {
            public static void QRInit(InitDDTV_Core.SatrtType satrtType)
            {
                switch(satrtType)
                {
                    case InitDDTV_Core.SatrtType.DDTV_GUI:
                        ByQRCode.QrCodeRefresh += ByQRCode_QrCodeRefresh; ;
                        ByQRCode.LoginByQrCode("#FF000000", "#FFFFFFFF", true).Save("./BiliQR.png", System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    default:
                        ByQRCode.QrCodeStatus_Changed += ByQRCode_QrCodeStatus_Changed;
                        ByQRCode.QrCodeRefresh += ByQRCode_QrCodeRefresh; ;
                        ByQRCode.LoginByQrCode("#FF000000", "#FFFFFFFF", true).Save("./BiliQR.png", System.Drawing.Imaging.ImageFormat.Png);
                        break;
                }
               
            }

            private static void ByQRCode_QrCodeRefresh(System.Drawing.Bitmap newQrCode)
            {
                newQrCode.Save("./BiliQR.png", System.Drawing.Imaging.ImageFormat.Png);
            }

            private static void ByQRCode_QrCodeStatus_Changed(ByQRCode.QrCodeStatus status, Account account)
            {
                if (status == ByQRCode.QrCodeStatus.Success)
                {
                    BilibiliUserConfig.AccClass=account;
                    Log.Log.AddLog(nameof(login), Log.LogClass.LogType.Info, "QR扫码登陆bilibili成功");
                    BilibiliUserConfig.account.uid= account.Uid;
                    foreach (var item in account.Cookies)
                    {
                        BilibiliUserConfig.account.cookie = BilibiliUserConfig.account.cookie + item + ";";
                    }
                    BilibiliUserConfig.account.ExTime =account.Expires_Cookies;
                    BilibiliUserConfig.account.csrf=account.CsrfToken;

                    BilibiliUserConfig.WritUserFile();
                }
            }
        } 
       
    }
}
