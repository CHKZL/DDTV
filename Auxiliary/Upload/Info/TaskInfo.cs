using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace Auxiliary.Upload.Info
{
    public class TaskInfo
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string fileName { get; }
        /// <summary>
        /// 本地文件夹
        /// </summary>
        public string localPath { get; }
        /// <summary>
        /// 上传文件夹
        /// </summary>
        public string remotePath { get; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public int startTime { set; get; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public int endTime { set; get; }

        public FileType fileType;
        public TaskType taskType;

        //public Service.ServiceInterface @interface;

        /// <summary>
        /// 已重试次数
        /// </summary>
        public int retries { set; get; }
        /// <summary>
        /// 上传状态
        /// </summary>
        public Status statusCode { set; get; }
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

        public TaskInfo(string fileName, string localPath, string remotePath,FileType fileType, TaskType taskType)
        {
            startTime = -1;
            endTime = -1;
            statusCode = Status.OnHold;
            this.fileName = fileName;
            this.localPath = localPath;
            this.remotePath = remotePath;
            this.fileType = fileType;
            this.taskType = taskType;

            retries = -1;
            progress = -1;
            comments = ""; 
        }

        public void UploadTask()
        {
            startTime = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
            Type classType = Type.GetType("Auxiliary.Upload.Service." + Enum.GetName(typeof(TaskType), taskType));
            Service.ServiceInterface @interface = (Service.ServiceInterface)Activator.CreateInstance(classType);

            retries = 1;//第一次上传
            statusCode = Status.OnGoing;

            InfoLog.InfoPrintf($"\r\n==============建立{Enum.GetName(typeof(TaskType), taskType)}-{Enum.GetName(typeof(FileType), fileType)}上传任务================\r\n" +
                          $"\r\n本地路径:{localPath}" +
                          $"\r\n上传路径:{remotePath}" +
                          $"\r\n开始时间：{MMPU.Unix转换为DateTime(startTime.ToString())}" +
                          $"\r\n===============建立{Enum.GetName(typeof(TaskType), taskType)}-{Enum.GetName(typeof(FileType), fileType)}上传任务===============\r\n", InfoLog.InfoClass.上传系统信息);
            comments = "建立上传任务";

            while (true)//失败后重试，达到最大次数后退出
            {
                try
                {
                    InfoLog.InfoPrintf($"{Enum.GetName(typeof(TaskType), taskType)}:开始第{retries}次上传", InfoLog.InfoClass.上传系统信息);
                    comments = $"开始第{retries}次上传";
                    statusCode = Status.OnGoing;
                    @interface.doUpload(this);

                    InfoLog.InfoPrintf($"{Enum.GetName(typeof(TaskType), taskType)}:上传完毕", InfoLog.InfoClass.上传系统信息);
                    endTime = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);//更新结束时间
                    
                    InfoLog.InfoPrintf($"\r\n=============={Enum.GetName(typeof(TaskType), taskType)}-{Enum.GetName(typeof(FileType), fileType)}上传成功================\r\n" +
                                       $"\r\n本地路径:{localPath}" +
                                       $"\r\n上传路径:{remotePath}" +
                                       $"\r\n开始时间：{MMPU.Unix转换为DateTime(startTime.ToString())}" +
                                       $"\r\n结束时间：{MMPU.Unix转换为DateTime(endTime.ToString())}" +
                                       $"\r\n==============={Enum.GetName(typeof(TaskType), taskType)}-{Enum.GetName(typeof(FileType), fileType)}上传成功===============\r\n", InfoLog.InfoClass.上传系统信息);
                    comments = $"上传成功";
                    statusCode = Status.Success;//上传成功
                    break;//成功则退出
                }
                catch (UploadFailure ex)//此次上传失败
                {
                    InfoLog.InfoPrintf($"{Enum.GetName(typeof(TaskType), taskType)}: {ex.Message}", InfoLog.InfoClass.系统错误信息);
                    statusCode = Status.OnHold;
                    if (retries == Configer.RETRY_MAX_TIMES)//最后一次上传
                    {
                        InfoLog.InfoPrintf($"{Enum.GetName(typeof(TaskType), taskType)}:第{retries}/{Configer.RETRY_MAX_TIMES}次{Enum.GetName(typeof(TaskType), taskType)}上传失败", InfoLog.InfoClass.上传系统信息);
                        endTime = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);

                        InfoLog.InfoPrintf($"\r\n=============={Enum.GetName(typeof(TaskType), taskType)}-{Enum.GetName(typeof(FileType), fileType)}上传失败================\r\n" +
                                           $"\r\n本地路径:{localPath}" +
                                           $"\r\n上传路径:{remotePath}" +
                                           $"\r\n开始时间：{MMPU.Unix转换为DateTime(startTime.ToString())}" +
                                           $"\r\n结束时间：{MMPU.Unix转换为DateTime(endTime.ToString())}" +
                                           $"\r\n==============={Enum.GetName(typeof(TaskType), taskType)}-{Enum.GetName(typeof(FileType), fileType)}上传失败===============\r\n", InfoLog.InfoClass.上传系统信息);
                        comments = $"上传失败";
                        statusCode = Status.Fail;//上传失败
                        break;//达到最大重试次数，失败，退出
                    }
                    else//未达到最大重试次数，等待一定时间后重试
                    {
                        InfoLog.InfoPrintf($"{Enum.GetName(typeof(TaskType), taskType)}:第{retries}/{Configer.RETRY_MAX_TIMES}次上传失败，{Configer.RETRY_WAITING_TIME}s后重试", InfoLog.InfoClass.上传系统信息);
                        comments = $"第{retries}次上传失败，重试等待中";
                        retries++;//重试次数+1
                        Thread.Sleep(Configer.RETRY_WAITING_TIME * 1000);//等待RETRY_WAITING_TIME秒
                    }
                }
            }

        }

    }
}
