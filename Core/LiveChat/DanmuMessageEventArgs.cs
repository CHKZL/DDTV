using System.Text.Json;
using System.Text.Json.Nodes;

namespace Core.LiveChat
{
    public class DanmuMessageEventArgs:MessageEventArgs
    {
        /// <summary>
        /// 弹幕在视频里的时间
        /// </summary>
        public double Time { set; get; }
        /// <summary>
        /// 弹幕类型
        /// </summary>
        public int MessageType { get; set; }
        /// <summary>
        /// 字体大小
        /// </summary>
        public int Size { set; get; } = 25;
        /// <summary>
        /// 弹幕颜色
        /// </summary>
        public int MessageColor { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// 弹幕池
        /// </summary>
        public int pool { set; get; } = 0;
        /// <summary>
        /// 发送人userId
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 发送人昵称
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 弹幕内容
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 发送人舰队等级
        /// </summary>
        public int GuardLV { get; set; }
        
       
        /// <summary>
        /// 弹幕字号
        /// </summary>
        public int MessageFontSize { get; set; }
       

        public string UserTitile { get; set; }


        internal DanmuMessageEventArgs(JsonObject obj) : base(obj)
        {
            
            UserId = (long)obj["info"][2][0];
            UserName = (string)obj["info"][2][1];
            Message = (string)obj["info"][1];
            GuardLV = (int)obj["info"][7];
            UserTitile = (string)obj["info"][5][1];
            MessageType = (int)obj["info"][0][1];
            MessageFontSize = (int)obj["info"][0][2];
            MessageColor = (int)obj["info"][0][3];
            Timestamp = (long)obj["info"][0][4];
        }
    }
}
