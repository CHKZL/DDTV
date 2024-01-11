using Core.Account;
using Core.Account.Linq;
using Core.LogModule;
using Core.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Core.Account.Kernel.ByQRCode;

namespace Core.RuntimeObject
{
    public class Account
    {
        public static event EventHandler<EventArgs> LoginFailureEvent;//登陆失效事件
        private static AccountInformation accountInformation = new();
        public static AccountInformation AccountInformation
        {
            get
            {
                if (accountInformation == null || !accountInformation.State)
                {
                    string[] files = Directory.GetFiles(Config.Core._ConfigDirectory, $"*{Config.Core._UserInfoCoinfFileExtension}");
                    if (files != null && files.Length > 0)
                    {
                        Tools.Encryption.DecryptFile(files[0], out string accountString);
                        AccountInformation accountInformation = JsonSerializer.Deserialize<AccountInformation>(accountString);
                        if (accountInformation != null && accountInformation.State)
                        {
                            Account.accountInformation = accountInformation;
                            if (!Core.Config.Core._LoginStatus)
                                Core.Config.Core._LoginStatus = true;
                        }
                    }
                    if (accountInformation == null || !accountInformation.State)
                    {
                        LoginFailureEvent?.Invoke(null, new EventArgs());
                    }
                }
                return accountInformation;
            }
            set
            {
                accountInformation = value;
                string Cookies = "";
                foreach (var item in accountInformation.Cookies)
                {
                    Cookies += item + ";";
                }
                accountInformation.strCookies = Cookies;
                accountInformation.State = true;
                Encryption.EncryptFile(JsonSerializer.Serialize(AccountInformation), $"{Config.Core._ConfigDirectory}{accountInformation.Uid}{Config.Core._UserInfoCoinfFileExtension}");
            }
        }

        private static bool _AccountCheckRunningStatus = false;
        private static bool _AccountStatus = false;
        internal static void CheckLoginStatus()
        {
            if (!_AccountCheckRunningStatus!)
            {
                _AccountCheckRunningStatus = true;
                Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            if (GetNavState())
                            {
                                _AccountStatus = true;
                                if (!Core.Config.Core._LoginStatus)
                                    Core.Config.Core._LoginStatus = true;

                            }
                            else
                            {
                                _AccountStatus = false;
                                if (Core.Config.Core._LoginStatus)
                                    Core.Config.Core._LoginStatus = false;
                            }
                            if (accountInformation == null || !accountInformation.State)
                            {
                                LoginFailureEvent?.Invoke(null, new EventArgs());
                            }
                        }
                        catch (Exception)
                        {
                            Log.Warn(nameof(CheckLoginStatus), $"登陆状态过期,请重新登陆");
                        }
                        if (!_AccountStatus)
                            Thread.Sleep(1000 * 60 * 10);
                        else
                            Thread.Sleep(1000 * 60);
                    }
                });
            }
        }


        /// <summary>
        /// 当前阿B登陆是否有效
        /// </summary>
        /// <returns></returns>
        public static bool GetNavState()
        {
            return Network.Methods.Nav.GetNav() == null;
        }
    }
}
