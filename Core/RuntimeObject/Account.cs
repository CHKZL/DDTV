using Core.Account;
using Core.Account.Linq;
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
