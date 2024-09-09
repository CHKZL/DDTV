using Core.LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.RuntimeObject.Download
{
    public class Snapshot
    {
        /// <summary>
        /// 生成录制中的直播间快照用于切片
        /// </summary>
        /// <param name="Uid"></param>
        /// <returns></returns>
        public static (bool state, string message) CreateRecordingSnapshot(long Uid)
        {
            if (Uid == 0) return (false, "UID不能为空");

            RoomCardClass Card = new();
            if (!_Room.GetCardForUID(Uid, ref Card) || Card == null || !Card.DownInfo.IsDownload) return (false, "当前房间未开播或未录制");

            try
            {
                if (Card.DownInfo.DownloadFileList.SnapshotGenerationInProgress) Card.DownInfo.DownloadFileList.SnapshotGenerationInProgress = true;
                else return (false, "当前房间已有快照正在生成中");

                string videoFile = Card.DownInfo.DownloadFileList.CurrentOperationVideoFile;
                if (!File.Exists(videoFile)) return (false, "当前直播间还未开始录制流");

                string tempFile = $"{Core.Config.Core_RunConfig._TemporaryFileDirectory}{Path.GetFileName(videoFile)}";
                File.Copy(videoFile, tempFile, true);

                var listener = Card.DownInfo.LiveChatListener;
                if (listener != null)
                {
                    Danmu.SevaDanmu(listener.DanmuMessage.Danmu, tempFile, Card.Name, Card.RoomId);
                    Danmu.SevaGift(listener.DanmuMessage.Gift, tempFile);
                    Danmu.SevaGuardBuy(listener.DanmuMessage.GuardBuy, tempFile);
                    Danmu.SevaSuperChat(listener.DanmuMessage.SuperChat, tempFile);
                }
                return (true, tempFile);
            }
            finally
            {
                Card.DownInfo.DownloadFileList.SnapshotGenerationInProgress = false;
            }

        }
    }
}