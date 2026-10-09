using Core.LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
        public static async Task<(bool state, string message)> CreateRecordingSnapshot(long Uid)
        {
            if (Uid == 0) return (false, "UID不能为空");

            RoomCardClass Card = new();
            if (!_Room.GetCardForUID(Uid, ref Card) || Card == null || !Card.DownInfo.IsDownload) return (false, "当前房间未开播或未录制");

            //原子占位防止并发快照：原check-and-set两步非原子，UI与API同时请求会双双通过并互相覆盖临时文件
            if (Interlocked.CompareExchange(ref Card.DownInfo.DownloadFileList.SnapshotGenerationInProgress, 1, 0) != 0)
                return (false, "当前房间已有快照正在生成中");
            try
            {
                //拷贝可能达数GB的录制文件是重IO操作，放后台线程避免阻塞UI/请求线程
                return await Task.Run(() =>
                {
                    string videoFile = Card.DownInfo.DownloadFileList.CurrentOperationVideoFile;
                    if (!File.Exists(videoFile)) return (false, "当前直播间还未开始录制流");

                    string tempFile = $"{Core.Config.Core_RunConfig._TemporaryFileDirectory}{Path.GetFileName(videoFile)}";
                    File.Copy(videoFile, tempFile, true);

                    var listener = Card.DownInfo.LiveChatListener;
                    var danmuMessage = listener?.DanmuMessage;
                    if (danmuMessage != null)
                    {
                        //与写入线程（Basics.LiveChatListener_MessageReceived）互斥，持锁浅拷贝快照后保存
                        List<Danmu.DanmuInfo> danmuSnapshot;
                        List<Danmu.GiftInfo> giftSnapshot;
                        List<Danmu.GuardBuyInfo> guardBuySnapshot;
                        List<Danmu.SuperChatInfo> superChatSnapshot;
                        lock (danmuMessage.MessageLock)
                        {
                            danmuSnapshot = new List<Danmu.DanmuInfo>(danmuMessage.Danmu);
                            giftSnapshot = new List<Danmu.GiftInfo>(danmuMessage.Gift);
                            guardBuySnapshot = new List<Danmu.GuardBuyInfo>(danmuMessage.GuardBuy);
                            superChatSnapshot = new List<Danmu.SuperChatInfo>(danmuMessage.SuperChat);
                        }
                        Danmu.SaveDanmu(danmuSnapshot, tempFile, Card.Name, Card.RoomId);
                        Danmu.SevaGift(giftSnapshot, tempFile);
                        Danmu.SevaGuardBuy(guardBuySnapshot, tempFile);
                        Danmu.SevaSuperChat(superChatSnapshot, tempFile);
                    }
                    return (true, tempFile);
                });
            }
            finally
            {
                Interlocked.Exchange(ref Card.DownInfo.DownloadFileList.SnapshotGenerationInProgress, 0);
            }

        }
    }
}
