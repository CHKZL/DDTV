using Core;
using Core.Account;
using Core.Account.Linq;
using Core.LogModule;
using SkiaSharp;
using System;
using System.Net;
using System.Security.Principal;
using System.Text.Json;
using static Core.Account.Kernel.ByQRCode;

namespace TestProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Core.Init.Start();
            Testing_LoadingLoginStatus();
            //test_QR();
            //Log_test();
            Console.ReadKey();
        }
        #region 已登陆登录态加载测试
        private static void Testing_LoadingLoginStatus()
        {
            string[] files = Directory.GetFiles(Config.ConfigDirectory, $"*{Config.UserInfoCoinfFileExtension}");
            if (files != null && files.Length > 0)
            {
                Core.Tools.Encryption.DecryptFile(files[0], out string accountString);
                AccountInformation accountInformation = JsonSerializer.Deserialize<AccountInformation>(accountString);
                if (accountInformation != null && accountInformation.State)
                {
                    Core.RuntimeObject.User.AccountInformation = accountInformation;
                }
            }
        }
        #endregion

        #region 扫码登陆测试

        private static void test_QR()
        {
            ByQRCode.QrCodeRefresh += ByQRCode_QrCodeRefresh;
            QR_Object QR = ByQRCode.LoginByQrCode("#FF000000", "#FFFFFFFF", true);
            ByQRCode.QrCodeStatus_Changed += ByQRCode_QrCodeStatus_Changed;
            using (var stream = File.OpenWrite("./BiliQR.png"))
            {
                QR.SKData.SaveTo(stream);
            }
            Core.Tools.QRConsole.Output(QR.OriginalString);
        }

        private static void ByQRCode_QrCodeStatus_Changed(ByQRCode.QrCodeStatus status, AccountInformation account)
        {
            if (status == ByQRCode.QrCodeStatus.Success)
            {
                account.State = true;
                Console.WriteLine($"Uid:{account.Uid}");
                Console.WriteLine($"Buvid:{account.Buvid}");
                string Cookies = "";
                foreach (var item in account.Cookies)
                {
                    Cookies += item + ";";
                }
                Console.WriteLine($"Expires_Cookies:{account.Expires_Cookies}");
                Console.WriteLine($"CsrfToken:{account.CsrfToken}");
                Core.RuntimeObject.User.AccountInformation = account;
                string accountString = JsonSerializer.Serialize(Core.RuntimeObject.User.AccountInformation);
                Core.Tools.Encryption.EncryptFile(accountString, $"{Config.ConfigDirectory}{account.Uid}{Config.UserInfoCoinfFileExtension}");
            }
        }

        private static void ByQRCode_QrCodeRefresh(Core.Account.Kernel.ByQRCode.QR_Object newQrCode)
        {
            using (var stream = File.OpenWrite("./BiliQR.png"))
            {
                newQrCode.SKData.SaveTo(stream);
            }
        }

        #endregion

        #region Log系统测试

        public static void Log_test()
        {
            Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    Log.Error("test1", "test");
                }
            });
            Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    Log.Error("test2", "test2");
                }
            });
            Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    Log.Error("test3", "test3");
                }
            });
        }

        #endregion
    }
}