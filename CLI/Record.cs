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
            //if(TEST)
            {
                TEST = false;
                if (e.IsRemind)
                {
                    Log.Info(nameof(DetectRoom_LiveStart), $"{e.RoomId}({e.Name})开播，等待30秒使HLS源生效");
                }
                if (e.IsAutoRec)
                {
                    do
                    {
                        Log.Info(nameof(DetectRoom_LiveStart), $"{e.RoomId}({e.Name})触发开播事件,开始录制");
                        var result = await Download.File.DlwnloadHls_avc_mp4(e);
                        if(result.Item1)
                        {
                            Core.Tools.Transcode transcode = new Core.Tools.Transcode();
                            try
                            {
                                transcode.TranscodeAsync(result.Item2, result.Item2.Replace(".mp4", "_fix.mp4"),e.RoomId);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(nameof(DetectRoom_LiveStart),$"{e.RoomId}({e.Name})完成录制任务后修复时出现意外错误，文件:{result.Item2}");
                            }
                        }
                    }
                    while (RoomList.GetLiveStatus(e.RoomId));
                    Log.Info(nameof(DetectRoom_LiveStart), $"{e.RoomId}({e.Name})录制结束");
                }
            }
        }

        private static void Transcode_ProgressChanged((double Progress, long RoomId, string before, string after) obj)
        {
            Console.WriteLine($"房间{obj.RoomId}的录制任务[{obj.after}]修复进度:{obj.Progress.ToString("0.00")}%");
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
