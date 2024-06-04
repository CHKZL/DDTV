using BiliAccount;
using BiliAccount.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DDTV_Core.SystemAssembly.ConfigModule;
using System.Drawing;
using SkiaSharp;
using System.IO;
using static BiliAccount.Core.ByQRCode;
using DDTV_Core.Tool;
using System.Runtime.InteropServices;
using DDTV_Core.SystemAssembly.Log;
using DDTV_Core.SystemAssembly.RoomPatrolModule;
using System.Net.Http;

namespace DDTV_Core.SystemAssembly.BilibiliModule.User
{
    public class login
    {
        public class VerifyLogin
        {
            public static bool QRLogin(InitDDTV_Core.SatrtType satrtType)
            {
                BilibiliUserConfig.account.cookie = String.Empty;
                Log.Log.AddLog(nameof(login), Log.LogClass.LogType.Info, "开始生成登陆QR码");
                QR.QRInit(satrtType);
                bool F = true;
                do
                {
                    if (string.IsNullOrEmpty(BilibiliUserConfig.account.csrf)||string.IsNullOrEmpty(BilibiliUserConfig.account.uid)||string.IsNullOrEmpty(BilibiliUserConfig.account.cookie)||BilibiliUserConfig.account.ExTime<DateTime.UtcNow)
                    {
                        if (F)
                        {
                            F = !F;
                            Log.Log.AddLog(nameof(login), Log.LogClass.LogType.Info, $"启动登陆流程，等待登陆，登陆类型为:{satrtType}");
                            switch (satrtType)
                            {
                                case InitDDTV_Core.SatrtType.DDTV_Core:
                                    Console.WriteLine("等待登陆中，请用bilibili手机客户端扫描控制台显示的二维码\r\n如果二维码排版错误，请打开文件[./BiliQR.png]进行登录");
                                    break;
                                case InitDDTV_Core.SatrtType.DDTV_CLI:
                                    Console.WriteLine("等待登陆中，请用bilibili手机客户端扫描控制台显示的二维码\r\n如果二维码排版错误，请打开DDTV根目录中生成的[./BiliQR.png]文件扫描进行登录");
                                    break;
                               case InitDDTV_Core.SatrtType.DDTV_CLI_Docker:
                                    Console.WriteLine("等待登陆中，请用bilibili手机客户端扫描控制台显示的二维码\r\n如果二维码排版错误，请打开DDTV根目录中生成的[./BiliQR.png]文件扫描进行登录");
                                    break;
                               
                                case InitDDTV_Core.SatrtType.DDTV_WEB_Docker:
                                    Console.WriteLine("等待登陆中，访问" + "[http://本设备IP地址:11419/api/loginqr]查看二维码\r\n或者使用bilibili手机客户端扫描控制台显示的二维码\r\n如果二维码排版错误打开DDTV根目录中生成的[./BiliQR.png]文件扫描进行登录");
                                    break;
                                case InitDDTV_Core.SatrtType.DDTV_WEB:
                                    Console.WriteLine("等待登陆中，访问" + "[http://本设备IP地址:11419/api/loginqr]查看二维码\r\n或者使用bilibili手机客户端扫描控制台显示的二维码\r\n如果二维码排版错误打开DDTV根目录中生成的[./BiliQR.png]文件扫描进行登录");
                                    break;
                                     case InitDDTV_Core.SatrtType.DDTV_GUI:
                                    return false;
                                default:
                                    break;
                            }
                        }
                    Thread.Sleep(5000);
                    }
                } while (string.IsNullOrEmpty(BilibiliUserConfig.account.cookie));
                RoomPatrol.IsOn = true;
                Log.Log.AddLog(nameof(login), Log.LogClass.LogType.Info, "登陆流程完成");
                return true;
            }
        }
        public class QR
        {
            public static void QRInit(InitDDTV_Core.SatrtType satrtType)
            {
                BilibiliUserConfig.account.loginStatus = BilibiliUserConfig.LoginStatus.LoggingIn;
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);
                }
                QR_Object sKData;
                switch (satrtType)
                {
                    case InitDDTV_Core.SatrtType.DDTV_GUI:
                        {
                            ByQRCode.QrCodeRefresh += ByQRCode_QrCodeRefresh;
                            QR_Object GUI = ByQRCode.LoginByQrCode("#FF000000", "#FFFFFFFF", true);
                            sKData = GUI;
                            break;
                        }
                    default:
                        {

                            Log.Log.AddLog(nameof(login), LogClass.LogType.Warn, "\r\n-------" +
                                "\r\n按任意键开始触发登陆流程" +
                                ",触发后请在5分钟内登陆" +
                                ",如果超时请重启程序再试" +
                                "\r\n-------\r\n");
                            try
                            {
                                Console.ReadKey();
                                Log.Log.AddLog(nameof(login), LogClass.LogType.Warn, "\r\n-------\r\n开始登陆流程\r\n-------\r\n");
                            }
                            catch (Exception)
                            {
                                Log.Log.AddLog(nameof(login), LogClass.LogType.Warn, "\r\n-------\r\n检测到当前并非控制台环境，跳过按键检测直接显示二维码等待扫码\r\n但请注意，长期触发该提示可能会导致风控\r\n-------\r\n");
                            }
                            
                            ByQRCode.QrCodeStatus_Changed += ByQRCode_QrCodeStatus_Changed;
                            ByQRCode.QrCodeRefresh += ByQRCode_QrCodeRefresh;
                            QR_Object DEF = ByQRCode.LoginByQrCode("#FF000000", "#FFFFFFFF", true);
                            sKData=DEF;
                            break;
                        }
                }
                QRConsole.Output(sKData.OriginalString);
                using (var stream = File.OpenWrite(@"./BiliQR.png"))
                {
                    sKData.SKData.SaveTo(stream);
                }
            }

            private static void ByQRCode_QrCodeRefresh(QR_Object newQrCode)
            {
                using (var stream = File.OpenWrite("./BiliQR.png"))
                {
                    newQrCode.SKData.SaveTo(stream);
                }
            }

            private static void ByQRCode_QrCodeStatus_Changed(ByQRCode.QrCodeStatus status, Account account)
            {
                if (status == ByQRCode.QrCodeStatus.Success)
                {
                    BilibiliUserConfig.AccClass=account;
                    RoomPatrol.IsOn = true;
                    Log.Log.AddLog(nameof(login), Log.LogClass.LogType.Info, "QR扫码登陆bilibili成功");
                    BilibiliUserConfig.account.uid= account.Uid;
                    //BilibiliUserConfig.account.buvid = account.Buvid;
                    BilibiliUserConfig.account.loginStatus = BilibiliUserConfig.LoginStatus.LoggedIn;
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

        public class ReLogin
        {
            public static void Login()
            {
                BilibiliUserConfig.account.loginStatus= BilibiliUserConfig.LoginStatus.LoggingIn;
                ByQRCode.QrCodeStatus_Changed += ByQRCode_QrCodeStatus_Changed;
                ByQRCode.QrCodeRefresh += ByQRCode_QrCodeRefresh;
                QR_Object GUI = ByQRCode.LoginByQrCode("#FF000000", "#FFFFFFFF", true);
                using (var stream = File.OpenWrite("./BiliQR.png"))
                {
                    GUI.SKData.SaveTo(stream);
                }
            }

            private static void ByQRCode_QrCodeRefresh(QR_Object newQrCode)
            {
                using (var stream = File.OpenWrite("./BiliQR.png"))
                {
                    newQrCode.SKData.SaveTo(stream);
                }
            }

            private static void ByQRCode_QrCodeStatus_Changed(ByQRCode.QrCodeStatus status, BiliAccount.Account account = null)
            {
                if (status == ByQRCode.QrCodeStatus.Success)
                {
                    BilibiliUserConfig.AccClass = account;
                    BilibiliUserConfig.account.loginStatus = BilibiliUserConfig.LoginStatus.LoggedIn;
                    Log.Log.AddLog(nameof(login), LogClass.LogType.Info, "QR扫码登陆bilibili成功");
                    BilibiliUserConfig.account.uid = account.Uid;
                    //BilibiliUserConfig.account.buvid = account.Buvid;
                    foreach (var item in account.Cookies)
                    {
                        BilibiliUserConfig.account.cookie = BilibiliUserConfig.account.cookie + item + ";";
                    }
                    BilibiliUserConfig.account.ExTime = account.Expires_Cookies;
                    BilibiliUserConfig.account.csrf = account.CsrfToken;

                    BilibiliUserConfig.WritUserFile();
                    DDTV_Core.InitDDTV_Core.ClientAID = CoreConfig.GetValue(CoreConfigClass.Key.ClientAID, Guid.NewGuid().ToString(), CoreConfigClass.Group.Core) + "-" + BilibiliUserConfig.account.uid;
                }

            }
        }
    }
}
