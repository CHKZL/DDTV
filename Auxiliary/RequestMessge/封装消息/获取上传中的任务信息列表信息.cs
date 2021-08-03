using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessge.MessgeClass;

namespace Auxiliary.RequestMessge.封装消息
{
    public class 获取上传中的任务信息列表信息
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
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功, A);
        }
    }
}
