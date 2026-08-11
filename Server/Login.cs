using Server.WebAppServices.Api;
using Core.Account;
using Core.Account.Linq;
using Core.LogModule;
using Masuit.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Account.Kernel.ByQRCode;

namespace Server
{
    public class Login
    {
        /// <summary>
        /// 登陆流程并发锁
        /// </summary>
        private static readonly object _loginLock = new();
        /// <summary>
        /// 是否有登陆流程正在进行中
        /// </summary>
        private static bool _loginInProgress = false;
        /// <summary>
        /// 最近一次二维码生成/刷新的时间（二维码180秒有效期到期后Core会自动刷新续期）
        /// </summary>
        private static DateTime _lastQrGeneratedAt = DateTime.MinValue;

        /// <summary>
        /// 事件只订阅一次，避免每次触发登陆都重复挂载处理器导致事件连发
        /// </summary>
        static Login()
        {
            ByQRCode.QrCodeRefresh += ByQRCode_QrCodeRefresh;
            ByQRCode.QrCodeStatus_Changed += ByQRCode_QrCodeStatus_Changed;
        }

        /// <summary>
        /// 触发扫码登陆流程
        /// </summary>
        /// <returns>true=新登陆流程已触发；false=已有登陆流程在进行中，复用当前二维码</returns>
        public static async Task<bool> QR()
        {
            lock (_loginLock)
            {
                //登陆流程进行中且二维码仍在自动刷新周期内时，忽略重复触发，
                //防止并发流程互相覆盖登录态（空账号写文件、二维码被刷新掉等）
                if (_loginInProgress && (DateTime.Now - _lastQrGeneratedAt) < TimeSpan.FromSeconds(200))
                {
                    Log.Info(nameof(Login), "登陆流程已在进行中，忽略重复的触发请求");
                    return false;
                }
                _loginInProgress = true;
                _lastQrGeneratedAt = DateTime.Now;
            }
            string Message = "触发登陆流程";
            OperationQueue.Add(Opcode.Account.TriggerLoginAgain, Message);
            Log.Info(nameof(Login), Message);
            AccountInformation tmp_a = Core.RuntimeObject.Account.AccountInformation;
            tmp_a.State = false;
            Core.RuntimeObject.Account.AccountInformation = tmp_a;
            await Task.Run(() =>
            {

                QR_Object QR = ByQRCode.LoginByQrCode("#FF000000", "#FFFFFFFF", true);
                using (var stream = File.OpenWrite($"./{Core.Config.Core_RunConfig._QrFileNmae}"))
                {
                    Log.Info(nameof(Login), $"保存登陆二维码为本地QR文件为：[{Core.Config.Core_RunConfig._QrFileNmae}]");
                    QR.SKData.SaveTo(stream);
                }
                using (var stream = File.OpenWrite($"./{Core.Config.Core_RunConfig._QrUrl}"))
                {
                    Log.Info(nameof(Login), $"保存登陆二维码的原始Url字符串文件为:[{Core.Config.Core_RunConfig._QrUrl}]");
                    stream.WriteAllText(QR.OriginalString, Encoding.UTF8);
                }
                Core.Tools.QRConsole.Output(QR.OriginalString);
            });
            return true;
        }
        private static void ByQRCode_QrCodeStatus_Changed(ByQRCode.QrCodeStatus status, AccountInformation account)
        {
            if (status == ByQRCode.QrCodeStatus.Success)
            {
                _loginInProgress = false;
                account.State = true;
                Core.RuntimeObject.Account.AccountInformation = account;
                Console.WriteLine($"登陆成功");
                Console.WriteLine($"Uid:{account.Uid}");
                Console.WriteLine($"Buvid:{account.Buvid}");
                Console.WriteLine($"Expires_Cookies:{account.Expires_Cookies}");
                Console.WriteLine($"CsrfToken:{account.CsrfToken}");
                Core.Tools.FileOperations.Delete(Core.Config.Core_RunConfig._QrFileNmae,"登陆完成，删除登陆用临时文件");
                Core.Tools.FileOperations.Delete(Core.Config.Core_RunConfig._QrUrl,"登陆完成，删除登陆用临时文件");

                string Message = "登陆成功";
                OperationQueue.Add(Opcode.Account.LoginSuccessful, Message);
                Log.Info(nameof(Login), Message);
                Core.Config.Core_RunConfig._ValidAccount = account.Uid;
            }
        }

        private static void ByQRCode_QrCodeRefresh(Core.Account.Kernel.ByQRCode.QR_Object newQrCode)
        {
            //二维码过期后Core自动刷新，续期节流窗口
            _lastQrGeneratedAt = DateTime.Now;
            using (var stream = File.OpenWrite($"./{Core.Config.Core_RunConfig._QrFileNmae}"))
            {
                newQrCode.SKData.SaveTo(stream);
            }
            using (var stream = File.OpenWrite($"./{Core.Config.Core_RunConfig._QrUrl}"))
            {
                stream.WriteAllText(newQrCode.OriginalString, Encoding.UTF8);
            }
        }
    }
}
