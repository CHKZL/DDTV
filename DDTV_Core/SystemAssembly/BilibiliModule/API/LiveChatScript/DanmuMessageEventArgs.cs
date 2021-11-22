using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using Newtonsoft.Json.Linq;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.LiveChatScript
{
    public class DanmuMessageEventArgs:MessageEventArgs
    {
        /// <summary>
        /// 发送人userId
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 发送人昵称
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 弹幕内容
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 发送人等级
        /// </summary>
        public int GuardLV { get; set; }
        /// <summary>
        /// 弹幕颜色
        /// </summary>
        public int MessageColor { get; set; }
        /// <summary>
        /// 弹幕类型
        /// </summary>
        public int MessageType { get; set; }
        /// <summary>
        /// 弹幕字号
        /// </summary>
        public int MessageFontSize { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; }

        public string UserTitile { get; set; }


        internal DanmuMessageEventArgs(JObject obj) : base(obj)
        {
            UserId = (int)obj["info"][2][0];
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
