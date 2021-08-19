using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessage.MessageClass;
using static Auxiliary.RequestMessage.Rec;

namespace Auxiliary.RequestMessage.封装消息
{
    public class 下载_获取当前录制中的任务队列简报信息
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
                        State = item.DownIofo.下载状态,
                        Remark=item.DownIofo.备注,
                        Transcoding = item.DownIofo.是否转码中
                    });
                }
            }
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功, list);
        }
    }
}
