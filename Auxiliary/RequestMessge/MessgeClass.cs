using System;
using System.Collections.Generic;
using System.Text;

namespace Auxiliary.RequestMessge
{
    public class MessgeClass
    {
        public class Messge<T>
        {
            public int code { set; get; }//状态码
            public string messge { set; get; }
            public int queue { set; get; }//Package的长度
            public List<T> Package { set; get; }
        }
        public enum ServerSendMessgeCode
        {
            请求错误 = -1,
            请求成功 = 1001,
            鉴权失败 = 1002,
            请求成功但出现了错误 = 1003,
        }
        public enum ClientSendMessgeCode
        {
            请求WebSocketWToken=2001,
            获取系统运行情况=2002,
            查看当前配置文件=2003,
            检查更新=2004,
            获取系统运行日志=2005,
            获取当前录制中的队列简报 = 2006,
            获取所有下载任务的队列简报 = 2007,
            根据录制任务GUID获取任务详情 = 2008,
            根据录制任务GUID取消相应任务 = 2009,
            增加配置文件中监听的房间 = 2010,
            删除配置文件中监听的房间 = 2011,
            修改房间的自动录制开关配置 = 2012,
            获取当前房间配置列表总览 = 2013,
            获取当前录制文件夹中的所有文件的列表 = 2014,
            删除某个录制完成的文件 = 2015,
            根据房间号获得相关录制文件 = 2016,
            获取上传任务信息列表 = 2017,
            获取上传中的任务信息列表 = 2018,
            修改配置_自动转码设置=2201,
        }
    }
    public class File
    {
        public class FileDeleteInfo
        {
            public bool result { set; get; }
            public string messge { set; get; } = null;
        }
        public class FileRangeInfo
        {
            public long Size { set; get; }
            public string Name { set; get; }
            public string Directory { set; get; }
            public string Path { set; get; }
            public DateTime ModifiedTime { set; get; }
        }
        public class FileListInfo
        {
            public long Size { set; get; }
            public string Name { set; get; }
            public string Directory { set; get; }
            public string Path { set; get; }
            public DateTime ModifiedTime { set; get; }
        }
    }
    public class Rec
    {
        public class RecAllList
        {
            /// <summary>
            /// 房间号
            /// </summary>
            public string RoomId { set; get; }
            /// <summary>
            /// 已下载大小(bit)
            /// </summary>
            public double Downloaded_bit { set; get; }
            /// <summary>
            /// 已下载大小(换算后的大小:字符串格式)
            /// </summary>
            public string Downloaded_str { set; get; }
            /// <summary>
            /// 主播名称
            /// </summary>
            public string Name { set; get; }
            /// <summary>
            /// 开始下载时间
            /// </summary>
            public int StartTime { set; get; }
            /// <summary>
            /// 结束下载时间
            /// </summary>
            public int EndTime { set; get; }
            /// <summary>
            /// 下载任务唯一标识符
            /// </summary>
            public string GUID { set; get; }
        }
        public class RecLProcessinist
        {
            /// <summary>
            /// 房间号
            /// </summary>
            public string RoomId { set; get; }
            /// <summary>
            /// 已下载大小(bit)
            /// </summary>
            public double Downloaded_bit { set; get; }
            /// <summary>
            /// 已下载大小(换算后的大小:字符串格式)
            /// </summary>
            public string Downloaded_str { set; get; }
            /// <summary>
            /// 主播名称
            /// </summary>
            public string Name { set; get; }
            /// <summary>
            /// 开始下载时间
            /// </summary>
            public int StartTime { set; get; }
            /// <summary>
            /// 结束下载时间
            /// </summary>
            public int EndTime { set; get; }
            /// <summary>
            /// 下载任务唯一标识符
            /// </summary>
            public string GUID { set; get; }
        }
    }
    public class Room
    {
        public class RoomDeleteInfo
        {
            public bool result { set; get; }
            public string messge { set; get; }
        }
        public class RoomStatusInfo
        {
            public bool result { set; get; }
            public string messge { set; get; }
        }
        public class RoomAddInfo
        {
            public bool result { set; get; }
            public string messge { set; get; } = null;
        }
    }
    public class System_Core
    {
        public class systemConfig
        {
            public string Key { set; get; }
            public string Value { set; get; }
        }
        public class SystemInfo
        {
            /// <summary>
            /// 当前DDTV版本号
            /// </summary>
            public string DDTVCore_Ver { get; set; }
            /// <summary>
            /// 监控房间数量
            /// </summary>
            public int Room_Quantity { get; set; }
            /// <summary>
            /// 操作系统相关信息
            /// </summary>
            public OS_Info oS_Info { get; set; }
            /// <summary>
            /// 下载任务基础信息
            /// </summary>
            public Download_Info download_Info { get; set; }
            /// <summary>
            /// DDTV更新信息
            /// </summary>
            public Ver_Info ver_Info { get; set; }
            public class OS_Info
            {
                /// <summary>
                /// 系统版本
                /// </summary>
                public string OS_Ver { get; set; }
                /// <summary>
                /// 系统类型
                /// </summary>
                public string OS_Tpye { get; set; }
                /// <summary>
                /// 使用内存量，单位bit
                /// </summary>
                public long Memory_Usage { get; set; }
                /// <summary>
                /// 运行时版本
                /// </summary>
                public string Runtime_Ver { get; set; }
                /// <summary>
                /// 是否在交互模式下
                /// </summary>
                public bool UserInteractive { get; set; }
                /// <summary>
                /// 关联的用户
                /// </summary>
                public string Associated_Users { get; set; }
                /// <summary>
                /// 工作目录
                /// </summary>
                public string Current_Directory { get; set; }
                /// <summary>
                /// Core程序核心框架版本
                /// </summary>
                public string AppCore_Ver { set; get; }
                /// <summary>
                /// Web程序核心框架版本
                /// </summary>
                public string WebCore_Ver { set; get; }
            }
            public class Download_Info
            {
                /// <summary>
                /// 下载中的任务数
                /// </summary>
                public int Downloading { get; set; }
                /// <summary>
                /// 下载结束的任务数
                /// </summary>
                public int Completed_Downloads { get; set; }
            }
            public class Ver_Info
            {
                /// <summary>
                /// 是否有新版本
                /// </summary>
                public bool IsNewVer { set; get; }
                /// <summary>
                /// 新版本号
                /// </summary>
                public string NewVer { get; set; }
                /// <summary>
                /// 更新日志
                /// </summary>
                public string Update_Log { get; set; }
            }
        }
        public class SystemUpdateInfo
        {
            /// <summary>
            /// 是否有新版本
            /// </summary>
            public bool IsNewVer { set; get; }
            /// <summary>
            /// 新版本号
            /// </summary>
            public string NewVer { get; set; }
            /// <summary>
            /// 更新日志
            /// </summary>
            public string Update_Log { get; set; }

        }
    }
    public class ServerClass
    {
        public class Login
        {
            /// <summary>
            /// 消息
            /// </summary>
            public string messge { set; get; }
            /// <summary>
            /// 是否成功
            /// </summary>
            public bool result { set; get; }
            /// <summary>
            /// 如果成功返回webtoken
            /// </summary>
            public string WebToken { set; get; } = null;
        }
    }

    public class Config
    {
        public class AutoTranscoding
        { 
            public bool result { set; get; }
            public string messge { set; get; }
        }

    }
}
