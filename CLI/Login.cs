using Core.Account;
using Core.Account.Linq;
using Masuit.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Account.Kernel.ByQRCode;

namespace CLI
{
    internal class Login
    {
        public static async Task QR()
        {
            await Task.Run(() =>
            {
                ByQRCode.QrCodeRefresh += ByQRCode_QrCodeRefresh;
                QR_Object QR = ByQRCode.LoginByQrCode("#FF000000", "#FFFFFFFF", true);
                ByQRCode.QrCodeStatus_Changed += ByQRCode_QrCodeStatus_Changed;
                using (var stream = File.OpenWrite($"./{Core.Config.Core._QrFileNmae}"))
                {
                    QR.SKData.SaveTo(stream);
                }
                using (var stream = File.OpenWrite($"./{Core.Config.Core._QrUrl}"))
                {
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
                Core.Tools.FileOperations.Delete(Core.Config.Core._QrFileNmae);
                Core.Tools.FileOperations.Delete(Core.Config.Core._QrUrl);
            }
        }

        private static void ByQRCode_QrCodeRefresh(Core.Account.Kernel.ByQRCode.QR_Object newQrCode)
        {
            using (var stream = File.OpenWrite($"./{Core.Config.Core._QrFileNmae}"))
            {
                newQrCode.SKData.SaveTo(stream);
            }
            using (var stream = File.OpenWrite($"./{Core.Config.Core._QrUrl}"))
            {
                stream.WriteAllText(newQrCode.OriginalString, Encoding.UTF8);
            }
        }
    }
}
