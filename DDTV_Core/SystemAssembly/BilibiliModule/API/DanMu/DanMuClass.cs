using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.DanMu
{
    public class DanMuClass
    {
        public class DanMuWssInfo
        {
            public long uid { set; get; }
            public string token { set; get; }
            public List<Host> host_list { set; get; } = new List<Host>();
        }
        public class Host
        {
            public string host { set; get; }
            public int port { set; get; }
            public int wss_port { set; get; }
            public int ws_port { set; get; }
        }
        public class DanmuMessage
        {
            public string FileName { set; get; }
            public Stopwatch TimeStopwatch { set; get; }
            /// <summary>
            /// 弹幕信息
            /// </summary>
            public List<DanmuInfo> Danmu { set; get; } = new();
            /// <summary>
            /// SC信息
            /// </summary>
            public List<SuperChatInfo> SuperChat { set; get; } = new();
            /// <summary>
            /// 礼物信息
            /// </summary>
            public List<GiftInfo> Gift { set; get; } = new();
            /// <summary>
            /// 舰队信息
            /// </summary>
            public List<GuardBuyInfo> GuardBuy { set; get; } = new();
        }
        public class DanmuInfo
        {
            /// <summary>
            /// 弹幕在视频里的时间
            /// </summary>
            public double time { set; get; }
            /// <summary>
            /// 弹幕类型
            /// </summary>
            public int type { set; get; }
            /// <summary>
            /// 弹幕大小
            /// </summary>
            public int size { set;get; }
            /// <summary>
            /// 弹幕颜色
            /// </summary>
            public int color { set; get; }
            /// <summary>
            /// 时间戳
            /// </summary>
            public long timestamp { set; get; }
            /// <summary>
            /// 弹幕池
            /// </summary>
            public int pool { set; get; }
            /// <summary>
            /// 发送者UID
            /// </summary>
            public long uid { set; get; }
            /// <summary>
            /// 弹幕信息
            /// </summary>
            public string Message { set; get; }
            /// <summary>
            /// 发送人昵称
            /// </summary>
            public string Nickname { set; get;}
        }
        public class SuperChatInfo
        {
            /// <summary>
            /// 送礼的时候在视频里的时间
            /// </summary>
            public double Time { set; get; }
            /// <summary>
            /// 时间戳
            /// </summary>
            public long Timestamp { set; get; }
            /// <summary>
            /// 打赏人UID
            /// </summary>
            public long UserId { set; get; }
            /// <summary>
            /// 打赏人昵称
            /// </summary>
            public string UserName { set; get; }
            /// <summary>
            /// SC金额
            /// </summary>
            public double Price { set; get; }
            /// <summary>
            /// SC消息内容
            /// </summary>
            public string Message { set; get; }
            /// <summary>
            /// SC消息内容_翻译后
            /// </summary>
            public string MessageTrans { set; get; }
        }
        public class GiftInfo
        {
            /// <summary>
            /// 送礼的时候在视频里的时间
            /// </summary>
            public double Time { set; get; }
            /// <summary>
            /// 时间戳
            /// </summary>
            public long Timestamp { set; get; }
            /// <summary>
            /// 送礼人UID
            /// </summary>
            public long UserId { set; get; }
            /// <summary>
            /// 送礼人昵称
            /// </summary>
            public string UserName { set; get; }
            /// <summary>
            /// 礼物数量
            /// </summary>
            public int Amount { set; get; }
            /// <summary>
            /// 花费：单位金瓜子
            /// </summary>
            public float Price { set; get; }
            /// <summary>
            /// 礼物名称
            /// </summary>
            public string GiftName { set; get; }
        }
        public class GuardBuyInfo
        {
            /// <summary>
            /// 送礼的时候在视频里的时间
            /// </summary>
            public double Time { set; get; }
            /// <summary>
            /// 时间戳
            /// </summary>
            public long Timestamp { set; get; }
            /// <summary>
            /// 上舰人UID
            /// </summary>
            public long UserId { set; get; }
            /// <summary>
            /// 上舰人昵称
            /// </summary>
            public string UserName { set; get; }
            /// <summary>
            /// 开通了几个月
            /// </summary>
            public int Number { set; get; }
            /// <summary>
            /// 开通的舰队名称
            /// </summary>
            public string GuradName { set; get; }
            /// <summary>
            /// 舰队等级：1-总督 2-提督 3-舰长
            /// </summary>
            public int GuardLevel { set; get; }
            /// <summary>
            /// 花费：单位金瓜子
            /// </summary>
            public int Price { set; get; }
        }
    }
}
