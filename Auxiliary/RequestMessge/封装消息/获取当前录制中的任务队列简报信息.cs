using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessge.MessgeClass;
using static Auxiliary.RequestMessge.Rec;

namespace Auxiliary.RequestMessge.封装消息
{
    public class 获取当前录制中的任务队列简报信息
    {
        public static string 当前录制中的任务队列简报信息()
        {
            List<RecLProcessinist> list = new List<RecLProcessinist>();
            foreach (var item in MMPU.DownList)
            {
                if (item.DownIofo.下载状态)
                {
                    list.Add(new RecLProcessinist
                    {
                        RoomId = item.DownIofo.房间_频道号,
                        Name = item.DownIofo.主播名称,
                        StartTime = item.DownIofo.开始时间,
                        EndTime = item.DownIofo.结束时间,
                        Downloaded_bit = item.DownIofo.已下载大小bit,
                        Downloaded_str = item.DownIofo.已下载大小str,
                        GUID = item.DownIofo.事件GUID,
                    });
                }
            }
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功, list);
        }
    }
}
