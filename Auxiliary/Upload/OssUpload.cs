using Aliyun.OSS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Auxiliary.Upload
{
    class OssUpload
    {
        private string endpoint;
        private string accessKeyId ;
        private string accessKeySecret;
        private string bucketName;
        private string objectName;
        //private string localFilename;

        DateTime startTime;
        /// <summary>
        /// 保存PartETag的列表
        /// </summary>
        private List<PartETag> partETags = new List<PartETag>();
        private OssClient client;
        private string uploadId = "";

        /// <summary>
        /// 参数获取
        /// </summary>
        public OssUpload()
        {
            client = new OssClient(endpoint, accessKeyId, accessKeySecret);
            endpoint = Uploader.ossEndpoint;
            accessKeyId = Uploader.ossAccessKeyId;
            accessKeySecret = Uploader.ossAccessKeySecret;
            bucketName = Uploader.ossBucketName;
        }

        /// <summary>
        /// 初始化分片上传
        /// </summary>
        private void InitMultiUpload()
        {
            try
            {
                // 定义上传的文件及所属存储空间的名称。您可以在InitiateMultipartUploadRequest中设置ObjectMeta，但不必指定其中的ContentLength。
                var request = new InitiateMultipartUploadRequest(bucketName, objectName);
                var result = client.InitiateMultipartUpload(request);
                uploadId = result.UploadId;
                // 打印UploadId。
                InfoLog.InfoPrintf($"Oss: Init multi part upload succeeded. Upload Id:{result.UploadId}", InfoLog.InfoClass.上传系统信息);
            }
            catch (Exception ex)
            {
                throw new UploadFailure($"Init multi part upload failed, {ex.Message}");
            }
        }

        /// <summary>
        /// 上传一个分片
        /// </summary>
        /// <param name="partNumber">当前上传分片 从0开始</param>
        /// <param name="partSize">块大小</param>
        /// <param name="srcPath">上传文件</param>
        private void UploadPart(int partNumber, long partSize, string srcPath)
        {
            var fs = File.Open(srcPath, FileMode.Open);
            var skipBytes = (long)partSize * partNumber;
            // 定位到本次上传的起始位置。
            fs.Seek(skipBytes, 0);
            // 计算本次上传的分片大小，最后一片为剩余的数据大小。
            var size = (partSize < fs.Length - skipBytes) ? partSize : (fs.Length - skipBytes);
            var request = new UploadPartRequest(bucketName, objectName, uploadId)
            {
                InputStream = fs,
                PartSize = size,
                PartNumber = partNumber + 1
            };
            // 调用UploadPart接口执行上传功能，返回结果中包含了这个数据片的ETag值。
            var result = client.UploadPart(request);
            partETags.Add(result.PartETag);
        }

        /// <summary>
        /// 完成分片上传任务
        /// </summary>
        private void CompleteMultiUpload()
        {
            try
            {
                var completeMultipartUploadRequest = new CompleteMultipartUploadRequest(bucketName, objectName, uploadId);
                foreach (var partETag in partETags)
                {
                    completeMultipartUploadRequest.PartETags.Add(partETag);
                }
                var result = client.CompleteMultipartUpload(completeMultipartUploadRequest);
                InfoLog.InfoPrintf($"Oss: Complete multi part succeeded.", InfoLog.InfoClass.上传系统信息);
            }
            catch (Exception ex)
            {
                throw new UploadFailure($"Complete multi part failed, {ex.Message}");
            }
        }
        /// <summary>
        /// 上传文件
        /// </summary>
        public void doUpload(UploadTask.UploadInfo uploadInfo)
        {
            try
            {
                this.objectName = Uploader.cosPath + uploadInfo.remotePath + uploadInfo.fileName;
                InitMultiUpload();
                FileInfo fileInfo = null;
                long partSize = 50 * 1048576;//初始块大小 50M
                long fileSize = 0;
                int partCount = 0;
                try
                {
                    long tmp;
                    fileInfo = new FileInfo(uploadInfo.srcFile);
                    fileSize = fileInfo.Length;
                    partSize /= 2;
                    do
                    {
                        partSize *= 2;
                        tmp = (fileSize - 1) / partSize + 1;
                    }
                    while (tmp > 500);//超过500块时自动增加块大小
                    partCount = (int)tmp;
                }
                catch (FileNotFoundException)
                {
                    throw new UploadFailure($"该文件{uploadInfo.srcFile}不存在");
                }
                startTime = DateTime.Now;
                for (var i = 0; i < partCount; i++)
                {
                    try
                    {
                        UploadPart(i, partSize, uploadInfo.srcFile);

                        int passTime = (int)(DateTime.Now - startTime).TotalSeconds;
                        string content = $"{partETags.Count}/{partCount} | {Math.Ceiling((double)(partETags.Count) / partCount * 100)}% |Time: {passTime}s | Remain: {Math.Ceiling((double)(passTime) * partCount / partETags.Count - passTime)}s";
                        InfoLog.InfoPrintf($"Oss: {content}", InfoLog.InfoClass.上传系统信息);
                        uploadInfo.status["Oss"].comments = content;
                        uploadInfo.status["Oss"].progress = (int)Math.Ceiling((double)(partETags.Count) / partCount * 100);
                    }
                    catch (Aliyun.OSS.Common.OssException)
                    {
                        Thread.Sleep(1000);
                        try
                        {
                            UploadPart(i, partSize, uploadInfo.srcFile);

                            int passTime = (int)(DateTime.Now - startTime).TotalSeconds;
                            string content = $"{partETags.Count}/{partCount} | {Math.Ceiling((double)(partETags.Count) / partCount * 100)}% |Time: {passTime}s | Remain: {Math.Ceiling((double)(passTime) * partCount / partETags.Count - passTime)}s";
                            InfoLog.InfoPrintf($"Oss: {content}", InfoLog.InfoClass.上传系统信息);
                            uploadInfo.status["Oss"].comments = content;
                            uploadInfo.status["Oss"].progress = (int)Math.Ceiling((double)(partETags.Count) / partCount * 100);
                        }
                        catch (Aliyun.OSS.Common.OssException)
                        {
                            throw new UploadFailure("Network error.");
                        }
                    }
                }
                CompleteMultiUpload();
            }
            catch (Exception e)
            {
                throw new UploadFailure($"Unexpected error, {e.Message}");
            }
        }
    }
}
