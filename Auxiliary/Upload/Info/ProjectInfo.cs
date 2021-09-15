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
        /// 开始时间
        /// </summary>
        public long startTime { set; get; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public long endTime { set; get; }

        public string comments;

        public int progress;

        /// <summary>
        /// 当前上传文件类型
        /// </summary>
        public FileType currentFileType { set; get; }
        
        /// <summary>
        /// 当前上传文件详情
        /// </summary>
        public FileInfo currentFileInfo = null;

        /// <summary>
        /// 上传文件列表
        /// </summary>
        public List<FileType> fileList = new List<FileType>();
        /// <summary>
        /// 已完成文件列表
        /// </summary>
        public List<FileType> fileDone = new List<FileType>();

        /// <summary>
        /// 上传状态
        /// </summary>
        public Status statusCode { set; get; }

        /// <summary>
        /// 上传文件列表
        /// </summary>
        public List<Info.FileInfo> files = new List<Info.FileInfo>();

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
                
                if (MMPU.转码功能使能) //开启转码
                {
                    if (MMPU.转码后自动删除文件) //开启转码后删除
                    {
                        try
                        {
                            addFile(flvFileName.Replace(".flv", ".mp4"), localPath, remotePath, FileType.mp4);
                            InfoLog.InfoPrintf($"转码mp4文件已添加上传队列", InfoLog.InfoClass.Debug);
                        }
                        catch (FileException ex) //未找到mp4文件
                        {
                            InfoLog.InfoPrintf(ex.Message, InfoLog.InfoClass.Debug);
                            InfoLog.InfoPrintf("未找到转码mp4文件", InfoLog.InfoClass.上传系统信息);
                            try //mp4文件不存在 添加flv文件
                            {
                                addFile(flvFileName, localPath, remotePath, FileType.flv);
                                InfoLog.InfoPrintf($"flv文件已添加上传队列", InfoLog.InfoClass.Debug);
                            }
                            catch (FileException ex2) //MP4 flv文件均不存在
                            {
                                InfoLog.InfoPrintf(ex2.Message, InfoLog.InfoClass.Debug);
                                InfoLog.InfoPrintf("未找到flv文件", InfoLog.InfoClass.Debug);
                                InfoLog.InfoPrintf("未找到视频文件", InfoLog.InfoClass.系统错误信息);
                            }
                        }
                    }
                    else //未开启转码后删除
                    {
                        try //添加mp4文件
                        {
                            addFile(flvFileName.Replace(".flv", ".mp4"), localPath, remotePath, FileType.mp4);
                            InfoLog.InfoPrintf($"转码mp4文件已添加上传队列", InfoLog.InfoClass.Debug);
                        }
                        catch (FileException ex)
                        {
                            InfoLog.InfoPrintf(ex.Message, InfoLog.InfoClass.Debug);
                            InfoLog.InfoPrintf("未找到转码mp4文件", InfoLog.InfoClass.上传系统信息);
                        }
                        try //添加flv文件
                        {
                            addFile(flvFileName, localPath, remotePath, FileType.flv);
                            InfoLog.InfoPrintf($"flv文件已添加上传队列", InfoLog.InfoClass.Debug);
                        }
                        catch (FileException ex)
                        {
                            InfoLog.InfoPrintf(ex.Message, InfoLog.InfoClass.Debug);
                            InfoLog.InfoPrintf("未找到flv文件", InfoLog.InfoClass.上传系统信息);
                        }
                    }
                }
                else //未开启转码
                {
                    try //添加flv文件
                    {
                        addFile(flvFileName, localPath, remotePath, FileType.flv);
                        InfoLog.InfoPrintf($"flv文件已添加上传队列", InfoLog.InfoClass.Debug);
                    }
                    catch (FileException ex)
                    {
                        InfoLog.InfoPrintf(ex.Message, InfoLog.InfoClass.Debug);
                        InfoLog.InfoPrintf("未找到flv文件", InfoLog.InfoClass.上传系统信息);
                    }
                }

                if (MMPU.录制弹幕)
                {
                    try
                    {
                        addFile(flvFileName + ".txt", localPath, remotePath, FileType.gift);
                        InfoLog.InfoPrintf($"礼物文件已添加上传队列", InfoLog.InfoClass.Debug);
                    }
                    catch (FileException ex)
                    {
                        InfoLog.InfoPrintf(ex.Message, InfoLog.InfoClass.Debug);
                        InfoLog.InfoPrintf("未找到礼物文件", InfoLog.InfoClass.上传系统信息);
                    }
                    try
                    {
                        addFile(flvFileName.Replace(".flv", (MMPU.弹幕录制种类 == 1 ? ".ass" : ".xml")), localPath, remotePath, FileType.danmu);
                        InfoLog.InfoPrintf($"弹幕文件已添加上传队列", InfoLog.InfoClass.Debug);
                    }
                    catch (FileException ex)
                    {
                        InfoLog.InfoPrintf(ex.Message, InfoLog.InfoClass.Debug);
                        InfoLog.InfoPrintf("未找到弹幕文件", InfoLog.InfoClass.上传系统信息);
                    }

                }

                if (files.Count == 0)
                    throw new ProjectException($"创建{streamerName}-{streamTitle}上传任务失败，未获取到相关文件");
            }
            catch (Exception e)
            {
                throw new ProjectException($"创建{streamerName}-{streamTitle}上传任务失败，无法获取项目信息\n{e}");
            }
        }

        private void addFile(string fileName, string localPath, string remotePath,FileType fileType)
        {
            files.Add(new FileInfo(fileName, localPath, remotePath, fileType));
            fileList.Add(fileType);
        }

        public void UploadProject()
        {
            startTime = MMPU.获取时间戳();

            statusCode = Status.OnGoing;

            bool flag = true;

            foreach (var item in files)
            {
                currentFileInfo = item;
                currentFileType = item.fileType;
                item.uploadFile();
                fileDone.Add(item.fileType);

                if (item.statusCode == Status.Fail)
                    flag = false;
                currentFileInfo = null;
            }

            endTime = MMPU.获取时间戳();
            if (flag)
                statusCode = Status.Success;
            else
                statusCode = Status.Fail;
        }
    }
}
