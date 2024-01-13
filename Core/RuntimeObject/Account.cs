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
        private static AccountInformation _accountInformation = new();
        public static AccountInformation AccountInformation
        {
            get
            {
                if (_accountInformation == null || !_accountInformation.State)
                {
                    string[] files = Directory.GetFiles(Config.Core._ConfigDirectory, $"*{Config.Core._UserInfoCoinfFileExtension}");
                    if (files.Length > 0)
                    {
                        Tools.Encryption.DecryptFile(files[0], out string accountString);
                        var tempAccountInfo = JsonSerializer.Deserialize<AccountInformation>(accountString);
                        if (tempAccountInfo?.State == true)
                        {
                            _accountInformation = tempAccountInfo;
                            Core.Config.Core._LoginStatus = true;
                        }
                        else
                        {
                            LoginFailureEvent?.Invoke(null, new EventArgs());
                        }
                    }
                }
                if (_accountInformation.State && !Core.Config.Core._LoginStatus)
                {
                    Core.Config.Core._LoginStatus = true;
                }
                return _accountInformation;
            }
            set
            {
                _accountInformation = value;
                Core.Config.Core._LoginStatus = value.State;
                Encryption.EncryptFile(JsonSerializer.Serialize(AccountInformation), $"{Config.Core._ConfigDirectory}{_accountInformation.Uid}{Config.Core._UserInfoCoinfFileExtension}");
            }
        }

        private static bool _AccountCheckRunningStatus = false;
        private static bool _AccountStatus = false;
        /// <summary>
        /// 检查登陆信息有效性
        /// </summary>
        internal static void CheckLoginStatus()
        {
            if (!_AccountCheckRunningStatus!)
            {
                _AccountCheckRunningStatus = true;
                Task.Run(() =>
                {
                    Thread.Sleep(1000 * 10);
                    while (true)
                    {
                        try
                        {
                            if (GetNavState())
                            {
                                AccountInformation.State = true;
                                _AccountStatus = true;
                                if (!Core.Config.Core._LoginStatus)
                                    Core.Config.Core._LoginStatus = true;
                            }
                            else
                            {
                                AccountInformation.State = false;
                                _AccountStatus = false;
                                if (Core.Config.Core._LoginStatus)
                                    Core.Config.Core._LoginStatus = false;
                            }
                            if (_accountInformation == null || !_accountInformation.State)
                            {
                                LoginFailureEvent?.Invoke(null, new EventArgs());
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Warn(nameof(CheckLoginStatus), $"登陆状态过期,请重新登陆",e);
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
        /// 验证当前阿B登陆是否有效
        /// </summary>
        /// <returns></returns>
        public static bool GetNavState()
        {
            return Network.Methods.Nav.GetNav() == null;
        }

        /// <summary>
        /// 获取本地登录态AccountInformation的有效状态
        /// </summary>
        /// <returns></returns>
        public static bool GetLoginStatus()
        {
            return AccountInformation.State;
        }
    }
}
