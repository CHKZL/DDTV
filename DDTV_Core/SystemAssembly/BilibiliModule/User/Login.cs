using BiliAccount;
using BiliAccount.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DDTV_Core.SystemAssembly.BilibiliModule.User
{
    public class login
    {
        public class VerifyLogin
        {
            public static void Loing(InitDDTV_Core.SatrtType satrtType)
            {
               QR.QRInit();
                do
                {
                    if (!string.IsNullOrEmpty(BilibiliUser.account.csrf)||!string.IsNullOrEmpty(BilibiliUser.account.uid)||!string.IsNullOrEmpty(BilibiliUser.account.cookie)||BilibiliUser.account.ExTime>DateTime.UtcNow)
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
                                Log.Log.AddLog(nameof(login), Log.LogClass.LogType.Info, "DDTV正在等待扫码登陆...");
                                break;
                            default:
                                break;
                        }    
                    Thread.Sleep(6000);
                    }
                } while (string.IsNullOrEmpty(BilibiliUser.account.cookie));
            }
        }
        public class QR
        {
            public static void QRInit()
            {
                ByQRCode.QrCodeStatus_Changed+=ByQRCode_QrCodeStatus_Changed;
                ByQRCode.QrCodeRefresh +=ByQRCode_QrCodeRefresh; ;
                ByQRCode.LoginByQrCode("#FF000000", "#FFFFFFFF", true).Save("./BiliQR.png", System.Drawing.Imaging.ImageFormat.Png);
            }

            private static void ByQRCode_QrCodeRefresh(System.Drawing.Bitmap newQrCode)
            {
                newQrCode.Save("./BiliQR.png", System.Drawing.Imaging.ImageFormat.Png);
            }

            private static void ByQRCode_QrCodeStatus_Changed(ByQRCode.QrCodeStatus status, Account account)
            {
                if (status == ByQRCode.QrCodeStatus.Success)
                {
                    BilibiliUser.AccClass=account;
                    Log.Log.AddLog(nameof(login), Log.LogClass.LogType.Info, "QR扫码登陆bilibili成功");
                    BilibiliUser.account.uid= account.Uid;
                    foreach (var item in account.Cookies)
                    {
                        BilibiliUser.account.cookie = BilibiliUser.account.cookie + item + ";";
                    }
                    BilibiliUser.account.ExTime =account.Expires_Cookies;
                    BilibiliUser.account.csrf=account.CsrfToken;

                    BilibiliUser.WritUserFile();
                }
            }
        } 
       
    }
}
