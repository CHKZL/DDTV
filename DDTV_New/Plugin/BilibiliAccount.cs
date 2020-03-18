using BiliAccount;
using BiliAccount.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_New.Plugin
{
    class BilibiliAccount
    {
        public static Account account = new Account();
        public static void BiliLogin()
        {
            window.BiliLoginWindowQR BLW = new window.BiliLoginWindowQR();
            BLW.ShowDialog();
        }
    }
}
