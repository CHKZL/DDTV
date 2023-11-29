using Core;
using Core.Account;
using Core.Account.Linq;
using Core.LogModule;
using Core.Network;
using Core.Network.Methods;
using Microsoft.Extensions.Configuration.Ini;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using SkiaSharp;
using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text.Json;
using static Core.Account.Kernel.ByQRCode;
using static Core.Network.Methods.User;
using Masuit.Tools.Reflection;

namespace TestProject
{
    internal class Program
    {
        static void Main(string[] args)
        {



            Core.Init.Start();//初始化必须执行的
            Testing_LoadingLoginStatus();//如果已经有登录态，必须执行的
                                         //test_QR();//如果没有登录态，需要执行扫码

           


            Console.ReadKey();
        }

        #region 获取房间信息
        private static void Testing_GetRoomInfo()
        {
            //var A = Core.RuntimeObject.Room.GetRoomInfo(122459);
            
        }
        #endregion

        #region 获取标题
        private static void Testing_GetTitle()
        {
            var A = Core.RuntimeObject.RoomList.GetTitle(122459);
            var B = Core.RuntimeObject.RoomList.GetTitle(122459);
        }
        #endregion

        #region 获取房间号
        private static void Testing_GetRoomId() 
        {
            var A = Core.RuntimeObject.RoomList.GetRoomId(122459);
            var B = Core.RuntimeObject.RoomList.GetRoomId(122459);
        }
        #endregion

        #region 获取房间列表信息

        public static void testing_roomlist()
        {
           //var A = Room.GetRoomList(new List<long> { 508963009 });
        }

        #endregion

        #region 获取账号信息测试
        public static void Testing_userinfo()
        {
            //var A =  Core.Network.Methods.User.GetUserInfo(122459);
            //var B = typeof(UserInfo);

        }
        #endregion

        #region 登录态nav状态检测测试

        public static void Testing_nav()
        {
            var B = Core.Network.Methods.Nav.GetNav();
        }

        #endregion

        #region 已登陆登录态加载测试
        private static void Testing_LoadingLoginStatus()
        {
            
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
                Core.RuntimeObject.Account.AccountInformation = account;
                string accountString = JsonSerializer.Serialize(Core.RuntimeObject.Account.AccountInformation);
                Core.Tools.Encryption.EncryptFile(accountString, $"{Config.Core._ConfigDirectory}{account.Uid}{Config.Core._UserInfoCoinfFileExtension}");
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