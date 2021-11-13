using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.Log
{
    public class LogClass
    {
        public enum LogType
        {
            /// <summary>
            /// 会造成整个系统无法运行的严重错误
            /// </summary>
            Error = 10,
            /// <summary>
            /// 会造成错误，但是不影响运行的警告
            /// </summary>
            Warn = 20,
            /// <summary>
            /// 系统一般消息
            /// </summary>
            Info = 30,
            /// <summary>
            /// 调试信息
            /// </summary>
            Debug = 40,
            /// <summary>
            /// 一些追踪数据
            /// </summary>
            Trace = 50,
            /// <summary>
            /// 打开所有日志
            /// </summary>
            All = int.MaxValue,
        }
        /// <summary>
        /// 日志内容
        /// </summary>
        public string? Message { set; get; }
        /// <summary>
        /// 日志类型
        /// </summary>
        public LogType Type { set; get; }
        /// <summary>
        /// 系统时间
        /// </summary>
        public DateTime Time { set; get; }
        /// <summary>
        /// 软件的运行时间
        /// </summary>
        public long RunningTime { set; get; }
        /// <summary>
        /// 来源
        /// </summary>
        public string? Source { set; get; }
    }
}