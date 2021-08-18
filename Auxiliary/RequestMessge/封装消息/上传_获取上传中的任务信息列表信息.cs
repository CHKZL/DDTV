using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessage.MessageClass;

namespace Auxiliary.RequestMessage.封装消息
{
    public class 上传_获取上传中的任务信息列表信息
    { 
        public static string 上传中的任务信息列表信息()
        {
            List<Upload.UploadTask.UploadInfo> A = new List<Upload.UploadTask.UploadInfo>();
            foreach (var item1 in Upload.Uploader.UploadList)
            {
                foreach (var item2 in item1.status)
                {
                    if (item2.Value.statusCode != 0 && item2.Value.statusCode != -1)
                    {
                        A.Add(item1);
                    }
                }
            }
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功, A);
        }
    }
}
