using Auxiliary.Upload;
using System.Collections.Generic;
using static Auxiliary.RequestMessage.MessageClass;

namespace Auxiliary.RequestMessage.封装消息
{
    public class 上传_获取上传中的任务信息列表信息
    { 
        public static string 上传中的任务信息列表信息()
        {
            List<ProjectMsg> A = new List<ProjectMsg>();
            foreach (var project in Upload.Configer.UploadList)
            {
                if (project.statusCode == Upload.Status.OnGoing)
                    A.Add(new ProjectMsg(project));
                
            }
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功, A);
        }
    }

}
