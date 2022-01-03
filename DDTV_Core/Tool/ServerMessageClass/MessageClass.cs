using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.Tool.ServerMessageClass
{
    public class MessageClass
    {
        public class VerClass
        {
            public string Ver { get; set; }
        }
        public class UpdateLogClass
        {
            public string Log { get; set; }
        }
        public class NoticeVer
        {
            public string Ver { set; get; }
        }
        public class NoticeText
        {
            public string Text { set; get; }
        }
    }
}
