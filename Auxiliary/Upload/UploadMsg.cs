using System.Collections.Generic;

namespace Auxiliary.Upload
{
    public class ProjectMsg
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
        /// 开始时间
        /// </summary>
        public long startTime { set; get; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public long endTime { set; get; }

        /// <summary>
        /// 当前上传文件类型
        /// </summary>
        public FileType currentFileType { set; get; }
        /// <summary>
        /// 当前上传任务网盘类型
        /// </summary>
        public TaskType currentTaskType { set; get; }

        /// <summary>
        /// 该项目上传文件列表
        /// </summary>
        public List<FileType> fileList = new List<FileType>();
        /// <summary>
        /// 该项目已完成文件列表
        /// </summary>
        public List<FileType> fileDone = new List<FileType>();

        /// <summary>
        /// 当前project状态
        /// </summary>
        public Status statusCode { set; get; }

        /// <summary>
        /// 当前task备注
        /// </summary>
        public string comments { set; get; }
        /// <summary>
        /// 当前task进度
        /// </summary>
        public int progress { set; get; }

        /// <summary>
        /// 正在上传的File详情
        /// </summary>
        public FileMsg fileInfo;
        /// <summary>
        /// 正在上传的Task详情
        /// </summary>
        public TaskMsg taskInfo;

        public ProjectMsg(Info.ProjectInfo project)
        {
            this.streamerName = project.streamerName;
            this.streamTitle = project.streamTitle;
            this.startTime = project.startTime;
            this.endTime = project.endTime;
            this.fileList = project.fileList;
            this.fileDone = project.fileDone;

            this.currentFileType = project.currentFileType;
            this.currentTaskType = project.currentFileInfo.currentTaskType;

            this.comments = project.currentFileInfo.currentTaskInfo.comments;
            this.progress = project.currentFileInfo.currentTaskInfo.progress;

            this.statusCode = project.statusCode;

            this.fileInfo = new FileMsg(project.currentFileInfo);
            this.taskInfo = new TaskMsg(project.currentFileInfo.currentTaskInfo);
        }

    }

    public class FileMsg
    {
        /// <summary>
        /// File文件名
        /// </summary>
        public string fileName { get; }
        /// <summary>
        /// 此文件类型
        /// </summary>
        public FileType fileType { get; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public double fileSize { get; }


        /// <summary>
        /// 当前文件上传任务列表
        /// </summary>
        public List<TaskType> taskList = new List<TaskType>();
        /// <summary>
        /// 当前文件已完成任务列表
        /// </summary>
        public List<TaskType> taskDone = new List<TaskType>();


        /// <summary>
        /// 开始时间
        /// </summary>
        public long startTime { set; get; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public long endTime { set; get; }

        /// <summary>
        /// 当前文件状态
        /// </summary>
        public Status statusCode { set; get; }

        public FileMsg(Info.FileInfo file)
        {
            this.fileName = file.fileName;
            this.fileSize = file.fileSize;
            this.fileType = file.fileType;
            this.taskList = file.taskList;
            this.taskDone = file.taskDone;
            this.startTime = file.startTime;
            this.endTime = file.endTime;
            this.statusCode = file.statusCode;
        }

    }

    public class TaskMsg
    {
        /// <summary>
        /// 本地文件完整路径
        /// </summary>
        public string localFullPath { get; }
        /// <summary>
        /// 网盘文件完整路径
        /// </summary>
        public string remoteFullPath { get; }

        public TaskType taskType { get; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public long startTime { set; get; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public long endTime { set; get; }

        /// <summary>
        /// 已重试次数
        /// </summary>
        public int retries { set; get; }

        /// <summary>
        /// 当前任务状态
        /// </summary>
        public Status statusCode { set; get; }
        public TaskMsg(Info.TaskInfo task)
        {
            this.localFullPath = task.localPath + task.fileName;
            this.remoteFullPath = task.remotePath + task.fileName;
            this.taskType = task.taskType;

            this.startTime = task.startTime;
            this.endTime = task.endTime;

            this.retries = task.retries;

            this.statusCode = task.statusCode;
        }
    }
}
