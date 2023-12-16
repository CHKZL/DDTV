using Core.LogModule;
using Core.RuntimeObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLI
{
    internal class Record
    {
       static bool TEST = true;

        /// <summary>
        /// 开播事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static async void DetectRoom_LiveStart(object? sender, RoomList.RoomCard e)
        {
            //if (TEST)
            {
               
                TEST = false;
                if (e.IsRemind)
                {
                    Log.Info(nameof(DetectRoom_LiveStart), $"{e.RoomId}({e.Name})开播，等待30秒使HLS源生效");
                }
                if (e.IsAutoRec)
                {
                    Log.Info(nameof(DetectRoom_LiveStart), $"{e.RoomId}({e.Name})触发开播事件,开始录制");
                    do
                    {
                        await Download.File.DlwnloadHls_avc_mp4(e);
                    }
                    while (RoomList.GetLiveStatus(e.RoomId));
                    Log.Info(nameof(DetectRoom_LiveStart), $"{e.RoomId}({e.Name})录制结束");
                }
            }


        }

        /// <summary>
        /// 下播事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void DetectRoom_LiveEnd(object? sender, RoomList.RoomCard e)
        {
            if (e.IsRemind)
            {
                Log.Info(nameof(DetectRoom_LiveEnd), $"{e.RoomId}({e.Name})下播");
            }

        }
    }
}
