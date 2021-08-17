using System;
using System.Collections.Generic;
using System.IO;

namespace Auxiliary.Upload.Info
{
    /// <summary>
    /// 每个任务的上传状态
    /// </summary>
    public class FileInfo
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
        /// 文件类型
        /// </summary>
        public FileType fileType { get; }


        /// <summary>
        /// 当前上传任务
        /// </summary>
        public TaskType currentTask { get; }
        /// <summary>
        /// 上传任务列表
        /// </summary>
        public Dictionary<TaskType, Info.TaskInfo> tasks = new Dictionary<TaskType, Info.TaskInfo>();


        /// <summary>
        /// 开始时间
        /// </summary>
        public int startTime { set; get; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public int endTime { set; get; }

        public double fileSize { get; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public Status statusCode { set; get; }

        /// <summary>
        /// 初始化上传状态
        /// </summary>
        public FileInfo(string fileName, string localPath, string remotePath, FileType fileType)
        {
            startTime = -1;
            endTime = -1;
            statusCode = Status.OnHold;
            this.fileName = fileName;
            this.localPath = localPath;
            this.remotePath = remotePath;
            this.fileType = fileType;
            this.fileSize = (double)new System.IO.FileInfo(this.localPath + this.fileName).Length;
            foreach (var item in Configer.UploadOrder) //遍历上传目标
            {
                TaskInfo task = new TaskInfo(fileName, localPath, remotePath, fileType, (TaskType)Enum.Parse(typeof(TaskType), item.Value));
                tasks.Add((TaskType)Enum.Parse(typeof(TaskType), item.Value), task);
            }
        }

        public void uploadFile()
        {
            startTime = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
            statusCode = Status.OnGoing;

            bool flag = true;

            foreach (var item in tasks)
            {
                item.Value.UploadTask();

                if (item.Value.statusCode == Status.Fail)
                    flag = false;
            }

            endTime = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
            if (flag)
                statusCode = Status.Success;
            else
                statusCode = Status.Fail;
        }
    }

}
