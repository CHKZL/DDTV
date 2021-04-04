using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using static Auxiliary.Downloader;


namespace Auxiliary.Upload
{
    public class UploadTask
    {
        public UploadInfo uploadInfo { set; get; }

        public delegate void doUpload(UploadInfo uploadInfo);

        public UploadTask(DownIofoData downIofo)
        {
            uploadInfo = new UploadInfo(downIofo);
            Uploader.UploadList.Add(uploadInfo);
        }


        public void UploadVideo()
        {
            new Task(() =>
            {
                if (!Uploader.enableUpload) return;
                bool lastStatus = false;
                int cnt = 0;
                foreach (var item in Uploader.UploadOrder)
                {
                    cnt++;
                    try
                    {
                        uploadInfo.type = item.Value;
                        switch (uploadInfo.type)
                        {
                            case "OneDrive":
                                Upload(new OneDriveUpload().doUpload);
                                break;
                            case "Cos":
                                Upload(new CosUpload().doUpload);
                                break;
                            default:
                                break;
                        }
                        if (cnt == Uploader.UploadOrder.Count)
                            lastStatus = true;
                    }
                    catch(UploadFailure)
                    {
                        if (cnt == Uploader.UploadOrder.Count)
                            lastStatus = false;
                    }
                }
                if (Uploader.deleteAfterUpload == "1" && lastStatus && System.IO.File.Exists(uploadInfo.srcFile))
                {
                    System.IO.File.Delete(uploadInfo.srcFile);
                }
            }).Start();
        }

        private void Upload(doUpload @do)
        {
            uploadInfo.retries = 1;
            UploadStatus uploadStatus = new UploadStatus();
            uploadInfo.status.Add(uploadInfo.type, uploadStatus);
            
            InfoLog.InfoPrintf($"\r\n==============建立{uploadInfo.type}上传任务================\r\n" +
                          $"主播名:{uploadInfo.streamerName}" +
                          $"\r\n标题:{uploadInfo.streamTitle}" +
                          $"\r\n本地文件:{uploadInfo.srcFile}" +
                          $"\r\n上传路径:{uploadInfo.remotePath}" +
                          $"\r\n网盘类型:{uploadInfo.type}" +
                          $"\r\n开始时间：{MMPU.Unix转换为DateTime(uploadInfo.status[uploadInfo.type].startTime.ToString())}" +
                          $"\r\n===============建立{uploadInfo.type}上传任务===============\r\n", InfoLog.InfoClass.上传必要提示);
            uploadInfo.status[uploadInfo.type].comments = "建立上传任务";
            uploadInfo.status[uploadInfo.type].statusCode = 1;
            while (true)
            {
                try
                {
                    InfoLog.InfoPrintf($"{uploadInfo.type}:开始第{uploadInfo.retries}次上传", InfoLog.InfoClass.上传必要提示);
                    uploadInfo.status[uploadInfo.type].comments = $"开始第{uploadInfo.retries}次上传";
                    uploadInfo.status[uploadInfo.type].statusCode = uploadInfo.retries;
                    @do(uploadInfo);
                    InfoLog.InfoPrintf($"{uploadInfo.type}:上传完毕", InfoLog.InfoClass.上传必要提示);
                    uploadInfo.status[uploadInfo.type].endTime = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                    InfoLog.InfoPrintf($"\r\n=============={uploadInfo.type}上传成功================\r\n" +
                                       $"主播名:{uploadInfo.streamerName}" +
                                       $"\r\n标题:{uploadInfo.streamTitle}" +
                                       $"\r\n本地文件:{uploadInfo.srcFile}" +
                                       $"\r\n上传路径:{uploadInfo.remotePath}" +
                                       $"\r\n网盘类型:{uploadInfo.type}" +
                                       $"\r\n开始时间：{MMPU.Unix转换为DateTime(uploadInfo.status[uploadInfo.type].startTime.ToString())}" +
                                       $"\r\n结束时间：{MMPU.Unix转换为DateTime(uploadInfo.status[uploadInfo.type].endTime.ToString())}" +
                                       $"\r\n==============={uploadInfo.type}上传成功===============\r\n", InfoLog.InfoClass.上传必要提示);
                    uploadInfo.status[uploadInfo.type].comments = $"上传成功";
                    uploadInfo.status[uploadInfo.type].statusCode = 0;
                    break;
                }
                catch (Exception)
                {
                    if (uploadInfo.retries == Uploader.RETRY_MAX_TIMES)
                    {
                        InfoLog.InfoPrintf($"{uploadInfo.type}:第{uploadInfo.retries}/{Uploader.RETRY_MAX_TIMES}次{uploadInfo.type}上传失败", InfoLog.InfoClass.上传必要提示);
                        uploadInfo.status[uploadInfo.type].endTime = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);

                        InfoLog.InfoPrintf($"\r\n=============={uploadInfo.type}上传失败================\r\n" +
                                       $"主播名:{uploadInfo.streamerName}" +
                                       $"\r\n标题:{uploadInfo.streamTitle}" +
                                       $"\r\n本地文件:{uploadInfo.srcFile}" +
                                       $"\r\n上传路径:{uploadInfo.remotePath}" +
                                       $"\r\n网盘类型:{uploadInfo.type}" +
                                       $"\r\n开始时间：{MMPU.Unix转换为DateTime(uploadInfo.status[uploadInfo.type].startTime.ToString())}" +
                                       $"\r\n结束时间：{MMPU.Unix转换为DateTime(uploadInfo.status[uploadInfo.type].endTime.ToString())}" +
                                       $"\r\n==============={uploadInfo.type}上传失败===============\r\n", InfoLog.InfoClass.上传必要提示);
                        uploadInfo.status[uploadInfo.type].comments = $"上传失败";
                        uploadInfo.status[uploadInfo.type].statusCode = -1;
                        throw new UploadFailure($"{uploadInfo.type} upload fail");
                        //break;
                    }
                    else
                    {
                        InfoLog.InfoPrintf($"{uploadInfo.type}:第{uploadInfo.retries}/{Uploader.RETRY_MAX_TIMES}次上传失败，{Uploader.RETRY_WAITING_TIME}s后重试", InfoLog.InfoClass.上传必要提示);
                        uploadInfo.status[uploadInfo.type].comments = $"第{uploadInfo.retries}次上传失败，重试等待中";
                        uploadInfo.retries++;
                        Thread.Sleep(Uploader.RETRY_WAITING_TIME * 1000);
                        uploadInfo.status[uploadInfo.type].statusCode = uploadInfo.retries;
                    }
                }
            }
        }

        public class UploadInfo
        {
            public string streamerName { get; }
            public string streamTitle { get; }
            public string fileName { get; }

            public string srcFile { get; }
            public string remotePath { get; }
            public string type { set; get; }
            public int retries { set; get; }

            public Dictionary<string, UploadStatus> status { set; get; } = new Dictionary<string, UploadStatus>();

            public UploadInfo(DownIofoData downIofo)
            {
                streamerName = downIofo.主播名称;
                streamTitle = downIofo.标题;
                srcFile = downIofo.文件保存路径;
                fileName = System.IO.Path.GetFileName(srcFile);
                remotePath = $"{downIofo.主播名称}_{downIofo.房间_频道号}/{MMPU.Unix转换为DateTime(downIofo.开始时间.ToString()).ToString("yyyyMMdd")}_{downIofo.标题}/";
                type = null;
                retries = 0;
            }
        }

        public class UploadStatus
        {
            public int startTime { set; get; }
            public int endTime { set; get; }
            public int statusCode { set; get; }//-1：上传失败 0:上传成功 其他：上传次数
            public string comments { set; get; }

            public UploadStatus ()
            {
                startTime = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                endTime = -1;
                statusCode = 1;
                comments = "";
            }
        }

    }
}