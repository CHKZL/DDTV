using System;
using System.Collections.Generic;
using System.IO;

namespace Auxiliary.Upload.Info
{
    /// <summary>
    /// Project: 录制的一个项目
    /// </summary>
    public class ProjectInfo
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
        /// 当前上传文件类型
        /// </summary>
        public FileType currentFile { set; get; }
        /// <summary>
        /// 上传文件列表
        /// </summary>
        public Dictionary<FileType, Info.FileInfo> files = new Dictionary<FileType, Info.FileInfo>();

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
        /// </summary>
        public Status statusCode { set; get; }

        /// <summary>
        /// 初始化上传信息
        /// </summary>
        /// <param name="downIofo">下载信息</param>
        public ProjectInfo(Downloader.DownIofoData downIofo)
        {
            startTime = -1;
            endTime = -1;
            statusCode = Status.OnHold;
            try
            {
                streamerName = downIofo.主播名称;
                streamTitle = downIofo.标题;
                string localPath = (Path.GetDirectoryName(downIofo.文件保存路径) + "/").Replace("\\", "/");
                string flvFileName = Path.GetFileName(downIofo.文件保存路径);
                string remotePath = $"{downIofo.主播名称}_{downIofo.房间_频道号}/{MMPU.Unix转换为DateTime(downIofo.开始时间.ToString()).ToString("yyyyMMdd")}_{downIofo.标题}/";
                files.Add(FileType.flv, new FileInfo(flvFileName, localPath, remotePath, FileType.flv));
                if (MMPU.录制弹幕)
                {
                    try
                    {
                        files.Add(FileType.gift, new FileInfo(flvFileName + ".txt", localPath, remotePath, FileType.gift));
                    }
                    catch
                    {
                        //throw new CreateUploadTaskFailure("礼物文件上传任务创建失败");
                    }
                    try
                    {
                        files.Add(FileType.danmu, new FileInfo(flvFileName.Replace(".flv", (MMPU.弹幕录制种类 == 1 ? ".ass" : ".xml")), localPath, remotePath, FileType.danmu));
                    }
                    catch
                    {
                        //throw new CreateUploadTaskFailure("弹幕文件上传任务创建失败");
                    }
                    
                }
                if (MMPU.转码功能使能)
                {
                    try
                    {
                        files.Add(FileType.mp4, new FileInfo(flvFileName.Replace(".flv", ".mp4"), localPath, remotePath, FileType.mp4));
                            if (MMPU.转码后自动删除文件)
                                files.Remove(FileType.flv);
                    }
                    catch
                    {
                        //throw new CreateUploadTaskFailure("转码文件上传任务创建失败");
                    }
                    
                }
            }
            catch
            {
                //throw new CreateUploadTaskFailure("创建{streamerName}-{streamTitle}上传任务失败，无法获取文件信息");//处理获取文件名、文件大小的错误
            }
        }

        public void UploadProject()
        {
            startTime = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
            statusCode = Status.OnGoing;

            bool flag = true;

            foreach (var item in files)
            {
                item.Value.uploadFile();

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
