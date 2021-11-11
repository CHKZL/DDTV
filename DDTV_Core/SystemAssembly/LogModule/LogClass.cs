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
            Error = 10,//会造成整个系统无法运行的严重错误
            Warn = 20,//会造成错误，但是不影响运行的警告
            Info = 30,//系统一般消息
            Debug = 40,//调试信息
            Trace = 50,//一些追踪数据
            All = int.MaxValue,//打开所有日志
        }
        public string? Message { set; get; }//日志内容
        public LogType Type { set; get; }//日志类型
        public DateTime Time { set; get; }//系统时间
        public int RunningTime { set; get; }//软件的运行时间
        public string? Source { set; get; }//来源
    }
}