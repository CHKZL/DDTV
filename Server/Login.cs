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
    internal class Login
    {
        public static async Task QR()
        {
            string Message = "触发登陆流程";
            OperationQueue.Add(Opcode.Account.TriggerLoginAgain, Message);
            Log.Info(nameof(Login), Message);
            AccountInformation tmp_a = Core.RuntimeObject.Account.AccountInformation;
            tmp_a.State = false;
            Core.RuntimeObject.Account.AccountInformation = tmp_a;
            await Task.Run(() =>
            {
               
                ByQRCode.QrCodeRefresh += ByQRCode_QrCodeRefresh;
                QR_Object QR = ByQRCode.LoginByQrCode("#FF000000", "#FFFFFFFF", true);
                ByQRCode.QrCodeStatus_Changed += ByQRCode_QrCodeStatus_Changed;
                using (var stream = File.OpenWrite($"./{Core.Config.Core_RunConfig._QrFileNmae}"))
                {
                    Log.Info(nameof(Login), $"保存登陆二维码为本地QR文件为：[{Core.Config.Core_RunConfig._QrFileNmae}]");
                    QR.SKData.SaveTo(stream);
                }
                using (var stream = File.OpenWrite($"./{Core.Config.Core_RunConfig._QrUrl}"))
                {
                    Log.Info(nameof(Login), $"保存登陆二维码的原始Url字符串文件为:[{Core.Config.Core_RunConfig._QrFileNmae}]");
                    stream.WriteAllText(QR.OriginalString, Encoding.UTF8);
                }
                Core.Tools.QRConsole.Output(QR.OriginalString);
            });
        }
        private static void ByQRCode_QrCodeStatus_Changed(ByQRCode.QrCodeStatus status, AccountInformation account)
        {
            if (status == ByQRCode.QrCodeStatus.Success)
            {
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
