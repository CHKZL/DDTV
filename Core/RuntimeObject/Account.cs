using Core.Account;
using Core.Account.Linq;
using Core.LogModule;
using Core.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Core.Account.Kernel.ByQRCode;
using static Core.Network.Methods.Nav;

namespace Core.RuntimeObject
{
    public class Account
    {
        public static event EventHandler<EventArgs> LoginFailureEvent;//登陆失效事件
        private static AccountInformation _accountInformation = new();
        private static readonly object _accountLock = new();//账号信息读写锁，防止多线程下赋值与写文件之间被其他线程篡改
        public static Nav_Class.Data nav_info = new Nav_Class.Data();
        public static AccountInformation AccountInformation
        {
            get
            {
                if (_accountInformation == null || string.IsNullOrEmpty(_accountInformation.strCookies) )
                {
                    string[] files = Directory.GetFiles(Config.Core_RunConfig._ConfigDirectory, $"*{Config.Core_RunConfig._UserInfoCoinfFileExtension}");
                    if (files.Length > 0)
                    {
                        //注意：_ValidAccount存的是裸UID，匹配时必须只取文件名部分（去掉目录前缀），否则永远匹配不上
                        if(Config.Core_RunConfig._ValidAccount=="-1")
                        {
                            Config.Core_RunConfig._ValidAccount = Path.GetFileName(files[0]).Replace($"{Config.Core_RunConfig._UserInfoCoinfFileExtension}", "");
                        }
                        string ACC = files[0];
                        foreach (var item in files)
                        {
                            if (Path.GetFileName(item).Replace($"{Config.Core_RunConfig._UserInfoCoinfFileExtension}", "") == Config.Core_RunConfig._ValidAccount)
                            {
                                ACC = item;
                                break;
                            }
                        }
                        Log.Info(nameof(AccountInformation), $"读取User配置文件{ACC}");
                        string accountString = string.Empty;
                        try
                        {
                            Tools.Encryption.DecryptFile(ACC, out accountString);
                        }
                        catch (Exception)
                        {
                            _accountInformation = new();
                            LoginFailureEvent?.Invoke(null, new EventArgs());
                            return _accountInformation;
                        }
                        var tempAccountInfo = JsonSerializer.Deserialize<AccountInformation>(accountString);
                        if (tempAccountInfo?.State == true)
                        {
                            _accountInformation = tempAccountInfo;
                            //Core.Config.Core._LoginStatus = true;
                        }
                        else
                        {
                            LoginFailureEvent?.Invoke(null, new EventArgs());
                        }
                    }
                    else
                    {
                        _accountInformation = new();
                        LoginFailureEvent?.Invoke(null, new EventArgs());    
                    }
                }
                //if (_accountInformation.State && !Core.Config.Core._LoginStatus)
                //{
                //    Core.Config.Core._LoginStatus = true;
                //}
                return _accountInformation;
            }
            set
            {
                string Message = $"更新登录态缓存:[{MethodBase.GetCurrentMethod().Name}]]";
                OperationQueue.Add(Opcode.Account.UpdateLoginStateCache, Message);
                Log.Info(nameof(AccountInformation), Message);
                lock (_accountLock)
                {
                    _accountInformation = value;
                    //注意：判断、序列化内容、文件名都必须使用传入的value，不能用静态字段_accountInformation，
                    //否则在赋值与写文件之间其他线程（如并发触发的重新登陆流程）可能将_accountInformation替换为空账号，
                    //导致写出文件名为空UID的".Duser"文件
                    if (!string.IsNullOrEmpty(value.Uid) && value.State)
                    {
                        Encryption.EncryptFile(JsonSerializer.Serialize(value), $"{Config.Core_RunConfig._ConfigDirectory}{value.Uid}{Config.Core_RunConfig._UserInfoCoinfFileExtension}");
                    }
                }
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
                LoginFailureEvent += Account_LoginFailureEvent;
                Task.Run(() =>
                {
                    Thread.Sleep(1000 * 10);
                    while (true)
                    {
                        try
                        {
                            //三重判断
                            if (GetNavState() || GetNavState() || GetNavState())
                            {
                                AccountInformation.State = true;
                                _AccountStatus = true;
                            }
                            else
                            {
                                AccountInformation.State = false;
                                _AccountStatus = false;
                            }
                            if (_accountInformation == null || !_accountInformation.State)
                            {
                                LoginFailureEvent?.Invoke(null, new EventArgs());                                
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error(nameof(CheckLoginStatus), $"登陆状态过期,请重新登陆", e);
                        }
                        if (!_AccountStatus)
                            Thread.Sleep(1000 * 60 * 10);
                        else
                            Thread.Sleep(1000 * 60);
                    }
                });
            }
        }

        private static void Account_LoginFailureEvent(object? sender, EventArgs e)
        {
            string Message = $"触发登陆失效事件";
            OperationQueue.Add(Opcode.Account.InvalidLoginStatus, Message);
            Log.Info(nameof(CheckLoginStatus), $"触发登陆失效事件");
            SMTP.TriggerEvent(null, SMTP.SMTP_EventType.LoginFailureReminder);
        }


        /// <summary>
        /// 验证当前登录态是否有效
        /// </summary>
        /// <returns></returns>
        public static bool GetNavState()
        {
            Thread.Sleep(new Random().Next(500, 2000));
            var LoginStatus = Network.Methods.Nav.GetNav();
            if (LoginStatus != null && LoginStatus.code == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
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
