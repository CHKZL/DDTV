using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static Auxiliary.Downloader;


namespace Auxiliary.Upload
{
    /// <summary>
    /// 上传任务
    /// </summary>
    public class UploadTask
    {
        /// <summary>
        /// 存放待上传文件相关信息
        /// </summary>
        public UploadInfo uploadInfo { set; get; }

        /// <summary>
        /// 委托函数，不同上传目标需自行编写
        /// </summary>
        /// <param name="uploadInfo">该目标指定的上传函数</param>
        public delegate void doUpload(UploadInfo uploadInfo);

        /// <summary>
        /// 初始化上传任务
        /// </summary>
        /// <param name="downIofo">下载信息</param>
        public UploadTask(DownIofoData downIofo)
        {
            try
            {
                uploadInfo = new UploadInfo(downIofo);//创建上传文件信息
                Uploader.UploadList.Add(uploadInfo);//加入上传列表，便于Web端获取上传详情
            }
            catch(CreateUploadTaskFailure)
            {
                uploadInfo = null;//无法获取上传信息时为空
            }
        }


        public void UploadVideo()
        {
            if (uploadInfo != null&&Uploader.enableUpload)//成功获取待上传文件的信息 且 开启上传
            {
                new Task(() =>
                {
                    foreach (var item in Uploader.UploadOrder)//遍历上传目标
                    {
                        uploadInfo.type = item.Value;//指定当前上传目标
                        switch (uploadInfo.type)//根据不同上传目标执行不同上传函数
                        {
                            case "OneDrive":
                                InfoLog.InfoPrintf("OneDrive上传任务已提交", InfoLog.InfoClass.下载系统信息);
                                Upload(new OneDriveUpload().doUpload);
                                break;
                            case "Cos":
                                InfoLog.InfoPrintf("Cos上传任务已提交", InfoLog.InfoClass.下载系统信息);
                                Upload(new CosUpload().doUpload);
                                break;
                            case "BaiduPan":
                                InfoLog.InfoPrintf("BaiduPan上传任务已提交", InfoLog.InfoClass.下载系统信息);
                                Upload(new BaiduPanUpload().doUpload);
                                break;
                            default:
                                break;
                        }
                    }
                    if (Uploader.deleteAfterUpload == "1" && uploadInfo.status[uploadInfo.type].statusCode == 0 && System.IO.File.Exists(uploadInfo.srcFile))
                    //设定上传后删除的参数 且 最后一个上传目标上传成功 且 当前文件存在，则删除文件
                    {
                        System.IO.File.Delete(uploadInfo.srcFile);
                    }
                }).Start();
            }
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="do">委托，传入当前上传目标的特定上传函数</param>
        private void Upload(doUpload @do)
        {
            uploadInfo.retries = 1;//第一次上传
            UploadStatus uploadStatus = new UploadStatus();
            uploadInfo.status.Add(uploadInfo.type, uploadStatus);//初始化并在该文件上传信息中添加新的上传状态

            InfoLog.InfoPrintf($"\r\n==============建立{uploadInfo.type}上传任务================\r\n" +
                          $"主播名:{uploadInfo.streamerName}" +
                          $"\r\n标题:{uploadInfo.streamTitle}" +
                          $"\r\n本地文件:{uploadInfo.srcFile}" +
                          $"\r\n上传路径:{uploadInfo.remotePath}" +
                          $"\r\n网盘类型:{uploadInfo.type}" +
                          $"\r\n开始时间：{MMPU.Unix转换为DateTime(uploadInfo.status[uploadInfo.type].startTime.ToString())}" +
                          $"\r\n===============建立{uploadInfo.type}上传任务===============\r\n", InfoLog.InfoClass.上传系统信息);
            uploadInfo.status[uploadInfo.type].comments = "建立上传任务";
            uploadInfo.status[uploadInfo.type].statusCode = 1;//第一次上传
            while (true)//失败后重试，达到最大次数后退出
            {
                try
                {
                    InfoLog.InfoPrintf($"{uploadInfo.type}:开始第{uploadInfo.retries}次上传", InfoLog.InfoClass.上传系统信息);
                    uploadInfo.status[uploadInfo.type].comments = $"开始第{uploadInfo.retries}次上传";
                    uploadInfo.status[uploadInfo.type].statusCode = uploadInfo.retries;//第n次上传
                    @do(uploadInfo);//执行指定目标的上传函数，失败则异常被捕获
                    InfoLog.InfoPrintf($"{uploadInfo.type}:上传完毕", InfoLog.InfoClass.上传系统信息);
                    uploadInfo.status[uploadInfo.type].endTime = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);//更新结束时间
                    InfoLog.InfoPrintf($"\r\n=============={uploadInfo.type}上传成功================\r\n" +
                                       $"主播名:{uploadInfo.streamerName}" +
                                       $"\r\n标题:{uploadInfo.streamTitle}" +
                                       $"\r\n本地文件:{uploadInfo.srcFile}" +
                                       $"\r\n上传路径:{uploadInfo.remotePath}" +
                                       $"\r\n网盘类型:{uploadInfo.type}" +
                                       $"\r\n开始时间：{MMPU.Unix转换为DateTime(uploadInfo.status[uploadInfo.type].startTime.ToString())}" +
                                       $"\r\n结束时间：{MMPU.Unix转换为DateTime(uploadInfo.status[uploadInfo.type].endTime.ToString())}" +
                                       $"\r\n==============={uploadInfo.type}上传成功===============\r\n", InfoLog.InfoClass.上传系统信息);
                    uploadInfo.status[uploadInfo.type].comments = $"上传成功";
                    uploadInfo.status[uploadInfo.type].statusCode = 0;//上传成功
                    break;//成功则退出
                }
                catch (UploadFailure)//此次上传失败
                {
                    if (uploadInfo.retries == Uploader.RETRY_MAX_TIMES)//最后一次上传
                    {
                        InfoLog.InfoPrintf($"{uploadInfo.type}:第{uploadInfo.retries}/{Uploader.RETRY_MAX_TIMES}次{uploadInfo.type}上传失败", InfoLog.InfoClass.上传系统信息);
                        uploadInfo.status[uploadInfo.type].endTime = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);

                        InfoLog.InfoPrintf($"\r\n=============={uploadInfo.type}上传失败================\r\n" +
                                       $"主播名:{uploadInfo.streamerName}" +
                                       $"\r\n标题:{uploadInfo.streamTitle}" +
                                       $"\r\n本地文件:{uploadInfo.srcFile}" +
                                       $"\r\n上传路径:{uploadInfo.remotePath}" +
                                       $"\r\n网盘类型:{uploadInfo.type}" +
                                       $"\r\n开始时间：{MMPU.Unix转换为DateTime(uploadInfo.status[uploadInfo.type].startTime.ToString())}" +
                                       $"\r\n结束时间：{MMPU.Unix转换为DateTime(uploadInfo.status[uploadInfo.type].endTime.ToString())}" +
                                       $"\r\n==============={uploadInfo.type}上传失败===============\r\n", InfoLog.InfoClass.上传系统信息);
                        uploadInfo.status[uploadInfo.type].comments = $"上传失败";
                        uploadInfo.status[uploadInfo.type].statusCode = -1;//上传失败
                        break;//达到最大重试次数，失败，退出
                    }
                    else//未达到最大重试次数，等待一定时间后重试
                    {
                        InfoLog.InfoPrintf($"{uploadInfo.type}:第{uploadInfo.retries}/{Uploader.RETRY_MAX_TIMES}次上传失败，{Uploader.RETRY_WAITING_TIME}s后重试", InfoLog.InfoClass.上传系统信息);
                        uploadInfo.status[uploadInfo.type].comments = $"第{uploadInfo.retries}次上传失败，重试等待中";
                        uploadInfo.retries++;//重试次数+1
                        Thread.Sleep(Uploader.RETRY_WAITING_TIME * 1000);//等待RETRY_WAITING_TIME秒
                        uploadInfo.status[uploadInfo.type].statusCode = uploadInfo.retries;//更新第n次重试
                    }
                }
            }
        }
        /// <summary>
        /// 每个文件的上传信息
        /// </summary>
        public class UploadInfo
        {
            /// <summary>
            /// 主播名
            /// </summary>
            public string streamerName { get; }
            /// <summary>
            /// 直播标题
            /// </summary>
            public string streamTitle { get; }
            /// <summary>
            /// 文件名
            /// </summary>
            public string fileName { get; }
            /// <summary>
            /// 文件大小
            /// </summary>
            public double fileSize { get; }

            /// <summary>
            /// 待上传文件路径
            /// </summary>
            public string srcFile { get; }
            /// <summary>
            /// 上传目标路径
            /// </summary>
            public string remotePath { get; }
            /// <summary>
            /// 上传目标
            /// </summary>
            public string type { set; get; }
            /// <summary>
            /// 已重试次数
            /// </summary>
            public int retries { set; get; }

            /// <summary>
            /// 存放每个上传任务的上传状态
            /// </summary>
            public Dictionary<string, UploadStatus> status { set; get; } = new Dictionary<string, UploadStatus>();

            /// <summary>
            /// 初始化上传信息
            /// </summary>
            /// <param name="downIofo">下载信息</param>
            public UploadInfo(DownIofoData downIofo)
            {
                try
                {
                    streamerName = downIofo.主播名称;
                    streamTitle = downIofo.标题;
                    srcFile = downIofo.文件保存路径;
                    fileName = System.IO.Path.GetFileName(srcFile);
                    fileSize = (double)new FileInfo(srcFile).Length;
                    remotePath = $"{downIofo.主播名称}_{downIofo.房间_频道号}/{MMPU.Unix转换为DateTime(downIofo.开始时间.ToString()).ToString("yyyyMMdd")}_{downIofo.标题}/";
                    type = null;
                    retries = 0;
                }
                catch
                {
                    InfoLog.InfoPrintf($"创建{srcFile}上传任务失败，无法获取文件信息", InfoLog.InfoClass.系统错误信息);
                    throw new CreateUploadTaskFailure("fail to ctreate upload task");//处理获取文件名、文件大小的错误
                }
            }
        }
        /// <summary>
        /// 每个任务的上传状态
        /// </summary>
        public class UploadStatus
        {
            /// <summary>
            /// 开始时间
            /// </summary>
            public int startTime { set; get; }
            /// <summary>
            /// 结束时间
            /// </summary>
            public int endTime { set; get; }
            /// <summary>
            /// 上传状态
            /// <para>-1：上传失败 0:上传成功 其他：上传次数</para>
            /// </summary>
            public int statusCode { set; get; }
            /// <summary>
            /// 其他信息
            /// <para>用于web端展示备注</para>
            /// </summary>
            public string comments { set; get; }
            /// <summary>
            /// 上传进度
            /// <para>范围为0~100之间的整数, -1时为进度读取失败</para>
            /// </summary>
            public int progress { set; get; }


            /// <summary>
            /// 初始化上传状态
            /// </summary>
            public UploadStatus()
            {
                startTime = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                endTime = -1;
                statusCode = 1;
                comments = "";
            }
        }

    }
}