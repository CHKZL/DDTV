using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Auxiliary.RequestMessge.File;
using static Auxiliary.RequestMessge.MessgeClass;
using System.IO;

namespace Auxiliary.RequestMessge.封装消息
{
    public class 删除录制的文件
    {
        public static string 删除(string Dir,string Name)
        {
            try
            {
                if (Directory.Exists(MMPU.缓存路径 + Dir))
                {
                    if (System.IO.File.Exists(MMPU.缓存路径 + Dir + "\\" + Name))
                    {
                        Auxiliary.MMPU.文件删除委托(Auxiliary.MMPU.缓存路径 + Dir + "\\" + Name, "来自API接口'file_delete'的请求，删除文件");
                        return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功, new List<FileDeleteInfo>() { new FileDeleteInfo() { result = true, messge = "文件已提加入删除委托列表，等待文件锁解锁后自动删除" } });
                    }
                    else
                    {
                        return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功但出现了错误, new List<FileDeleteInfo>() { new FileDeleteInfo() { result = false } }, "该文件不存在");
                    }
                }
                else
                {
                    return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功但出现了错误, new List<FileDeleteInfo>() { new FileDeleteInfo() { result = false } }, "路径有误");
                }
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功但出现了错误, new List<FileDeleteInfo>() { new FileDeleteInfo() { result = false } }, "文件路径错误或者格式符合法！");
            }
        }
    }
}
