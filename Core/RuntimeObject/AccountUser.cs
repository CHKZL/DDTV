using Core.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.RuntimeObject
{
    public class AccountUser
    {
        private static AccountInformation accountInformation = new AccountInformation();
        public static AccountInformation AccountInformation { get { return accountInformation; } set { accountInformation = value; } }
    }
}
