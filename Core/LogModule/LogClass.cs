using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Config;

namespace Core.LogModule
{
    
    public class LogClass
    {


        public enum LogType
        {
            /// <summary>
            /// 会造成整个DDTV无法运行的严重错误
            /// </summary>
            Error = 10,
            /// <summary>
            /// 虽然现在还没发生问题，但是不管这个问题之后肯定会导致严重错误
            /// </summary>
            Error_IsAboutToHappen = 11,
            /// <summary>
            /// 会造成错误，但是不影响运行的警告
            /// </summary>
            Warn = 20,         
            /// <summary>
            /// 房间巡逻系统错误日志
            /// </summary>
            Warn_RoomPatrol = 23,
            /// <summary>
            /// 系统一般消息
            /// </summary>
            Info = 30,
            /// <summary>
            /// 转码消息
            /// </summary>
            Info_Transcod=31,
            /// <summary>
            /// API消息
            /// </summary>
            Info_API = 32,
            /// <summary>
            /// IP协议版本消息
            /// </summary>
            Info_IP_Ver = 33,
            /// <summary>
            /// 地址报文
            /// </summary>
            Info_IP_Address = 34,
            /// <summary>
            /// 删除文件消息
            /// </summary>
            Info_DelFile=35,
            /// <summary>
            /// 调试信息
            /// </summary>
            Debug = 40,
            /// <summary>
            /// 调试信息
            /// </summary>
            Debug_Request = 41,
            /// <summary>
            /// DDcenter请求
            /// </summary>
            Debug_DDcenter = 42,
            /// <summary>
            /// 调试信息
            /// </summary>
            Debug_Request_Error = 43,
             /// <summary>
            /// GetFile_Bytes错误信息
            /// </summary>
            Debug_GetFile_Bytes = 44,
            /// <summary>
            /// 一些追踪数据
            /// </summary>
            Trace = 50,
            Trace_Web=51,
            TmpInfo=99,
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
        /// <summary>
        /// 是否需要写入txt记录的错误
        /// </summary>
        public bool IsError { set; get; }
        /// <summary>
        /// IsError为真时有效，记录错误详细信息
        /// </summary>
        public Exception exception { set; get; }
        /// <summary>
        /// 是否应该打印到终端
        /// </summary>
        public bool IsDisplay { set; get; } =false;
    }
}
