using Core.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.RuntimeObject
{
    public class Account
    {
        private static AccountInformation accountInformation = null;
        public static AccountInformation AccountInformation
        {
            get
            {
                if (accountInformation == null)
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
                }
                return accountInformation;
            }
            set { accountInformation = value; }
        }
    }
}
