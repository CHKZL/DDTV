using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Auxiliary.Upload
{
    public static class Uploader
    {
        //上传配置属性
        public static bool enableUpload { set; get; } = false;//开启上传
        private static bool CheckEnableUpload { set; get; } = false;//检测配置文件中是否成功配置上传
        public static int RETRY_MAX_TIMES { get; } = 5;//重试次数
        public static int RETRY_WAITING_TIME { get; } = 60;//重试等待时间
        //OneDrive
        public static string enableOneDrive { set; get; } = "";
        public static string oneDriveConfig { set; get; } = "";
        public static string oneDrivePath = "";
        //Cos
        public static string enableCos { set; get; } = "";
        public static string cosSecretId { set; get; } = "";
        public static string cosSecretKey { set; get; } = "";
        public static string cosRegion { set; get; } = "";
        public static string cosBucket { set; get; } = "";
        public static string cosPath = "";

        public static Dictionary<int, string> UploadOrderTemp { set; get; } = new Dictionary<int, string>(); //未排序上传顺序
        public static Dictionary<int, string> UploadOrder { set; get; } //上传顺序

        public static List<UploadTask.UploadInfo> UploadList { set; get; } = new List<UploadTask.UploadInfo>(); //当前上传任务

        //删除文件
        public static string deleteAfterUpload { set; get; } = "";

        /// <summary>
        /// 上传配置初始化
        /// </summary>
        public static void InitUpload()
        {
            try
            {
                enableUpload = MMPU.读取exe默认配置文件("EnableUpload", "0") == "1" ? true : false;
                InfoLog.InfoPrintf($"配置文件初始化任务[EnableUpload]:{enableUpload}", InfoLog.InfoClass.Debug);
            }
            catch
            {
                enableUpload = false;
                deleteAfterUpload = "0";
                InfoLog.InfoPrintf($"上传模块初始化错误，已关闭自动上传", InfoLog.InfoClass.系统错误信息);
                return;
            }
            if (enableUpload)
            {
                try
                {
                    deleteAfterUpload = MMPU.读取exe默认配置文件("DeleteAfterUpload", "0");
                    InfoLog.InfoPrintf($"配置文件初始化任务[DeleteAfterUpload]:{deleteAfterUpload}", InfoLog.InfoClass.Debug);

                    InitOneDrive();
                    InitCos();

                    enableUpload &= CheckEnableUpload; //配置文件中EnableUpload开启 且 至少成功配置一个上传目标
                    UploadOrder = UploadOrderTemp.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
                }
                catch (System.ArgumentException)
                {
                    InfoLog.InfoPrintf($"上传顺序出现重复，已自动关闭上传", InfoLog.InfoClass.系统错误信息);
                    enableUpload = false;
                    return;
                }
            }

        }

        /// <summary>
        /// 初始化OneDrive
        /// </summary>
        private static void InitOneDrive()
        {
            enableOneDrive = MMPU.读取exe默认配置文件("EnableOneDrive", "0");
            InfoLog.InfoPrintf($"配置文件初始化任务[EnableOneDrive]:{enableOneDrive}", InfoLog.InfoClass.Debug);
            if (enableOneDrive != "0")
            {
                UploadOrderTemp.Add(int.Parse(enableOneDrive), "OneDrive");
                CheckEnableUpload = true;
                InfoLog.InfoPrintf($"已检测到OneDrive上传任务，上传顺序为{enableOneDrive}", InfoLog.InfoClass.上传必要提示);

                oneDriveConfig = MMPU.读取exe默认配置文件("OneDriveConfig", "");
                InfoLog.InfoPrintf($"配置文件初始化任务[oneDriveConfig]:{oneDriveConfig}", InfoLog.InfoClass.Debug);
                oneDrivePath = MMPU.读取exe默认配置文件("OneDrivePath", "/");
                MMPU.CheckPath(ref oneDrivePath);
                InfoLog.InfoPrintf($"配置文件初始化任务[oneDrivePath]:{oneDrivePath}", InfoLog.InfoClass.Debug);
            }
        }

        /// <summary>
        /// 初始化Cos
        /// </summary>
        private static void InitCos()
        {
            enableCos = MMPU.读取exe默认配置文件("EnableCos", "0");
            InfoLog.InfoPrintf($"配置文件初始化任务[EnableCos]:{enableCos}", InfoLog.InfoClass.Debug);
            if (enableCos != "0")
            {
                UploadOrderTemp.Add(int.Parse(enableCos), "Cos");
                CheckEnableUpload = true;
                InfoLog.InfoPrintf($"已检测到Cos上传任务，上传顺序为{enableCos}", InfoLog.InfoClass.上传必要提示);

                cosSecretId = MMPU.读取exe默认配置文件("CosSecretId", "");
                InfoLog.InfoPrintf($"配置文件初始化任务[CosSecretId]:敏感信息，隐藏内容，信息长度:{cosSecretId.Length}", InfoLog.InfoClass.Debug);

                cosSecretKey = MMPU.读取exe默认配置文件("CosSecretKey", "");
                InfoLog.InfoPrintf($"配置文件初始化任务[CosSecretKey]:敏感信息，隐藏内容，信息长度:{cosSecretKey.Length}", InfoLog.InfoClass.Debug);

                cosRegion = MMPU.读取exe默认配置文件("CosRegion", "");
                InfoLog.InfoPrintf($"配置文件初始化任务[CosRegion]:{cosRegion}", InfoLog.InfoClass.Debug);

                cosBucket = MMPU.读取exe默认配置文件("CosBucket", "");
                InfoLog.InfoPrintf($"配置文件初始化任务[CosBucket]:{cosBucket}", InfoLog.InfoClass.Debug);

                cosPath = MMPU.读取exe默认配置文件("CosPath", "/");
                MMPU.CheckPath(ref cosPath);
                InfoLog.InfoPrintf($"配置文件初始化任务[CosPath]:{cosPath}", InfoLog.InfoClass.Debug);
            }
        }
    }
}
