using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Auxiliary.Upload
{
    class CosUpload
    {
        private CosXml cosXml;
        private string uploadId;
        private string key;
        private string bucket;
        DateTime startTime;
        Dictionary<int, string> partNumberAndETags = new Dictionary<int, string>();

        public CosUpload()
        {
            CosXmlConfig config = new CosXmlConfig.Builder()
              .SetRegion(Uploader.cosRegion) //设置一个默认的存储桶地域
              .Build();
            long durationSecond = 600;//每次请求签名有效时长，单位为秒
            QCloudCredentialProvider qCloudCredentialProvider = new DefaultQCloudCredentialProvider(Uploader.cosSecretId,
              Uploader.cosSecretKey, durationSecond);
            this.bucket = Uploader.cosBucket;
            this.cosXml = new CosXmlServer(config, qCloudCredentialProvider);
        }

        /// 初始化分片上传
        private void InitMultiUpload()
        {
            InitMultipartUploadRequest request = new InitMultipartUploadRequest(bucket, key);
            //执行请求
            InitMultipartUploadResult result = cosXml.InitMultipartUpload(request);
            //请求成功
            this.uploadId = result.initMultipartUpload.uploadId; //用于后续分块上传的 uploadId
            //Console.WriteLine(result.GetResultInfo());
        }

        /// 上传一个分片
        private void UploadPart(int partNumber, long partSize, string srcPath)
        {

            UploadPartRequest request = new UploadPartRequest(bucket, key, partNumber,
              this.uploadId, srcPath, partSize * (partNumber - 1), partSize);
            //执行请求
            UploadPartResult result = cosXml.UploadPart(request);
            //请求成功
            //获取返回分块的eTag,用于后续CompleteMultiUploads
            partNumberAndETags.Add(partNumber, result.eTag);
            //Console.WriteLine(result.GetResultInfo());
        }

        /// 完成分片上传任务
        private void CompleteMultiUpload()
        {
            try
            {
                CompleteMultipartUploadRequest request = new CompleteMultipartUploadRequest(bucket, key, uploadId);
                //设置已上传的parts,必须有序，按照partNumber递增
                Dictionary<int, string> partNumberAndETags_SortedByKey = partNumberAndETags.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
                request.SetPartNumberAndETag(partNumberAndETags_SortedByKey);
                //执行请求
                CompleteMultipartUploadResult result = cosXml.CompleteMultiUpload(request);
                //请求成功
                //Console.WriteLine(result.GetResultInfo());
            }
            catch (ArgumentNullException e)
            {
                throw new UploadFailure($"Complete multi part failed, ArgumentNullException:{e.Message}");
            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                //请求失败
                throw new UploadFailure($"Complete multi part failed, CosClientException:{clientEx.Message}");
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                //请求失败
                throw new UploadFailure($"Complete multi part failed, CosServerException:{serverEx.GetInfo()}");
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
                this.key = Uploader.cosPath + uploadInfo.remotePath + uploadInfo.fileName;
                InitMultiUpload();
                FileInfo fileInfo = null;
                long partSize = 50 * 1048576;//初始块大小 50M
                long sourceLength = 0;
                int partNum = 0;
                try
                {
                    long tmp;
                    fileInfo = new FileInfo(uploadInfo.srcFile);
                    sourceLength = fileInfo.Length;
                    partSize /= 2;
                    do
                    {
                        partSize *= 2;
                        tmp = (sourceLength - 1) / partSize + 1;
                    }
                    while (tmp > 500);//超过500块时自动增加块大小
                    partNum = (int)tmp;
                }
                catch (FileNotFoundException)
                {
                    throw new UploadFailure($"该文件{uploadInfo.srcFile}不存在");
                }
                startTime = DateTime.Now;
                for (int i = 1; i <= partNum; i++)
                {
                    try
                    { 
                        UploadPart(i, partSize, uploadInfo.srcFile);
                        int passTime = (int)(DateTime.Now - startTime).TotalSeconds;
                        string content = $"{i}/{partNum} | {Math.Ceiling((double)(i) / partNum * 100)}% |Time: {passTime}s | Remain: {Math.Ceiling((double)(passTime) * partNum / i - passTime)}s";
                        InfoLog.InfoPrintf($"Cos: {content}", InfoLog.InfoClass.上传系统信息);
                        uploadInfo.status["Cos"].comments = content;
                        uploadInfo.status["Cos"].progress = (int)Math.Ceiling((double)(i) / partNum * 100);
                    }
                    catch (COSXML.CosException.CosServerException)
                    {
                        Thread.Sleep(1000);
                        try
                        {
                            UploadPart(i, partSize, uploadInfo.srcFile);
                            int passTime = (int)(DateTime.Now - startTime).TotalSeconds;
                            string content = $"{i}/{partNum} | {Math.Ceiling((double)(i) / partNum * 100)}% |Time: {passTime}s | Remain: {Math.Ceiling((double)(passTime) * partNum / i - passTime)}s";
                            InfoLog.InfoPrintf($"Cos: {content}", InfoLog.InfoClass.上传系统信息);
                            uploadInfo.status["Cos"].comments = content;
                            uploadInfo.status["Cos"].progress = (int)Math.Ceiling((double)(i) / partNum * 100);
                        }
                        catch (COSXML.CosException.CosServerException)
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
