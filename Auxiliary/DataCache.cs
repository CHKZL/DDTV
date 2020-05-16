using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    public class DataCache
    {
        public static int BilibiliApiCount = 0;

        public static Dictionary<string, string> 通过UID获取房间号键值对=new Dictionary<string, string>();

        public static Dictionary<string, string> 获取真实房间号键值对=new Dictionary<string, string>();

        public static Dictionary<string, string> 获取标题键值对 = new Dictionary<string, string>();
        public static Dictionary<string, DateTime> 获取标题有效期 = new Dictionary<string, DateTime>();


    }
}