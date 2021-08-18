using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace Auxiliary.Upload
{
    /// <summary>
    /// 上传任务
    /// </summary>
    public class Upload
    {
        /// <summary>
        /// 存放待上传文件相关信息
        /// </summary>
        public Info.ProjectInfo project { set; get; }

        /// <summary>
        /// 委托函数，不同上传目标需自行编写
        /// </summary>
        /// <param name="task">子任务信息</param>
        public delegate void doUpload(FileInfo task);

        /// <summary>
        /// 初始化上传任务
        /// </summary>
        /// <param name="downIofo">下载信息</param>
        public Upload(Downloader.DownIofoData downIofo)
        {
            try
            {
                if (Configer.enableUpload)
                {
                    project = new Info.ProjectInfo(downIofo);//创建上传文件信息
                    Configer.UploadList.Add(project);//加入上传列表，便于Web端获取上传详情
                }
                else project = null;
            }
            catch(TaskException ex)
            {
                project = null;//无法获取上传信息时为空
                InfoLog.InfoPrintf(ex.Message, InfoLog.InfoClass.系统错误信息);
            }
        }

        public void upload()
        {
            if (project != null)//成功获取待上传文件的信息
            {
                new Task(() =>
                {
                    try
                    {
                        project.UploadProject();
                    }
                    catch (Exception ex)
                    {
                        InfoLog.InfoPrintf($"未知上传错误, {project.streamerName}-{project.streamTitle}上传失败\n{ex.Message}", InfoLog.InfoClass.系统错误信息);
                    }
                }).Start();
            }
        }
    }
}