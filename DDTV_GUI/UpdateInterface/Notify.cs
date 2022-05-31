using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_GUI.UpdateInterface
{
    public class Notify
    {
        
        public static event EventHandler<EventArgs> NotifyUpdate;
        /// <summary>
        /// 增加Notify提示
        /// </summary>
        /// <param name="A">持续时间(毫秒)</param>
        /// <param name="B">标题</param>
        /// <param name="C">内容</param>
        public static void Add(string title, string content)
        {
            InfoC infoC = new InfoC()
            {
                Title = title,
                Content = content
            };
             NotifyUpdate?.Invoke(infoC, EventArgs.Empty);
        }
        public class InfoC
        {
            /// <summary>
            /// Notify消息标题
            /// </summary>
            public string Title { set; get; } = "";
            /// <summary>
            /// Notify消息内容
            /// </summary>
            public string Content { set; get; } = "";
        }
    }
}
