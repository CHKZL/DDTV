using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessage.MessageClass;

namespace Auxiliary.RequestMessage.封装消息
{
    public class 获取上传中的任务信息列表信息
    { 
        public static string 上传中的任务信息列表信息()
        {
            List<Upload.Info.ProjectInfo> A = new List<Upload.Info.ProjectInfo>();
            foreach (var project in Upload.Configer.UploadList)
            {
                checkAdd(A, project);
            }
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功, A);
        }

        private static void checkAdd(List<Upload.Info.ProjectInfo> A, Upload.Info.ProjectInfo project)
        {
            foreach (var filePair in project.files)
            {
                var file = filePair.Value;
                foreach (var taskPair in file.tasks)
                {
                    var task = taskPair.Value;
                    if (task.statusCode == Upload.Status.OnGoing)
                    {
                        A.Add(project);
                        return;
                    }
                }
            }
        }
    }
}
