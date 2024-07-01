using AngleSharp.Dom.Events;
using Core.LiveChat;
using Core.LogModule;
using Core.Network.Methods;
using Core.RuntimeObject.Download;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Core.RuntimeObject.Download.HLS;
using static Core.RuntimeObject.RoomInfo;

namespace Core.RuntimeObject
{
    public class Detect
    {
        public static DetectRoom detectRoom = new();//实例化房间监听    
        

        public Detect()
        {
            detectRoom.LiveStart += DetectRoom_LiveStart;//开播事件
            Log.Info(nameof(DetectRoom), $"注册开播事件");
            detectRoom.LiveEnd += DetectRoom_LiveEnd;//下播事件
            Log.Info(nameof(DetectRoom), $"注册下播事件");
            Detect.detectRoom.start();//启动房间监听
        }


        /// <summary>
        /// 开播事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="roomCard"></param>
        internal static async void DetectRoom_LiveStart(Object? sender, (RoomCardClass Card, bool IsFirst) LiveInvoke)
        {
            try
            {


                RoomCardClass roomCard = LiveInvoke.Card;
                List<TriggerType> triggerTypes = sender as List<TriggerType> ?? new List<TriggerType>();

                if (roomCard.live_status.Value != 1)
                {
                    Log.Info(nameof(DetectRoom_LiveStart), $"{roomCard.RoomId}({roomCard.Name})触发录制事件，但目前该房间检测到未开播，跳过本次录制任务");
                    return;
                }

                OperationQueue.Add(Opcode.Download.StartLiveEvent, $"开播事件，房间UID:{roomCard.UID}", roomCard.UID);

                if (roomCard.IsRemind && triggerTypes.Contains(TriggerType.RegularTasks))
                {
                    // 这里应该是开播广播事件

                    OperationQueue.Add(Opcode.Download.StartBroadcastingReminder, $"开播提醒，房间UID:{roomCard.UID}", roomCard.UID);
                }
                
                if (roomCard.IsAutoRec || triggerTypes.Contains(TriggerType.ManuallyTriggeringTasks) || roomCard.AppointmentRecord)
                {
                    if (roomCard.DownInfo.IsDownload)
                    {
                        Log.Info(nameof(DetectRoom_LiveStart), $"{roomCard.Name}({roomCard.RoomId})触发录制事件，但目前该房间已有录制任务，跳过本次录制任务");
                        return;
                    }
                    roomCard.DownInfo.IsDownload = true;
                    if (roomCard.IsRecDanmu)
                    {
                        
                        if (roomCard.DownInfo.LiveChatListener == null)
                        {
                            roomCard.DownInfo.LiveChatListener = new Core.LiveChat.LiveChatListener(roomCard.RoomId);
                            roomCard.DownInfo.LiveChatListener.Connect();
                        }
                        else if (!roomCard.DownInfo.LiveChatListener.State)
                        {
                            roomCard.DownInfo.LiveChatListener.Connect();
                        }
                        roomCard.DownInfo.LiveChatListener.Register.Add("DetectRoom_LiveStart");
                    }
                    
                    try
                    {
                        if (roomCard.IsRecDanmu)
                        {
                            if (roomCard.DownInfo.LiveChatListener != null)
                                roomCard.DownInfo.LiveChatListener.MessageReceived += Basics.LiveChatListener_MessageReceived;
                        }
                        bool Reconnection = false;
                        //保存封面
                        if(Config.Core_RunConfig._SaveCover)
                        {
                            Cover.SaveCover(roomCard);
                        }
                        do
                        {
                            //如果是启动后第一次房间状态查询就触发下载，那就当作重连处理
                            if (LiveInvoke.IsFirst)
                            {
                                Reconnection = true;
                            }
                            //核心下载函数
                            await Basics.HandleRecordingAsync(roomCard, triggerTypes, Reconnection, LiveInvoke.IsFirst);
                            //设置为重连模式
                            Reconnection = true;
                        }
                        //如果检测到还在开播，且用户没有取消，那么就再来一次
                        while ((RoomInfo.GetLiveStatus(roomCard.RoomId) && !roomCard.DownInfo.Unmark) && roomCard.DownInfo.Status != RoomCardClass.DownloadStatus.Special);

                        //执行shell
                        if (OperatingSystem.IsLinux() && Config.Core_RunConfig._Linux_Only_ShellSwitch)
                        {
                            Tools.Shell.Run(Tools.KeyCharacterReplacement.ReplaceKeyword(Config.Core_RunConfig._Linux_Only_ShellCommand, DateTime.Now, roomCard.UID));
                        }


                        //在这一步之前应该处理完所有本次录制任务的工作，执行完成后，清空本次除了录制的文件以外的所有记录
                        Basics.DownloadCompletedReset(ref roomCard);
                        string msg = $"{roomCard.Name}({roomCard.RoomId})录制结束" + (roomCard.DownInfo.Unmark ? "【原因：用户取消】" : "");
                        OperationQueue.Add(Opcode.Download.RecordingEnd, msg, roomCard.UID);
                        Log.Info(nameof(DetectRoom_LiveStart), msg);
                        roomCard.DownInfo.Unmark = false;
                        roomCard.DownInfo.IsDownload = false;
                    }
                    finally
                    {
                        if (roomCard.DownInfo.LiveChatListener != null)
                        {
                            roomCard.DownInfo.LiveChatListener.Register.Remove("DetectRoom_LiveStart");
                            if (roomCard.DownInfo.LiveChatListener.Register.Count == 0)
                            {
                                roomCard.DownInfo.LiveChatListener.DanmuMessage = null;
                                try
                                {
                                    roomCard.DownInfo.LiveChatListener.Dispose();
                                    roomCard.DownInfo.LiveChatListener = null;
                                }
                                catch (Exception)
                                { }
                            }
                        }

                    }
                }
            }
            catch (Exception)
            {
                ;
            }
        }




        /// <summary>
        /// 下播事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void DetectRoom_LiveEnd(object? sender, RoomCardClass e)
        {
            if (e.IsRemind)
            {
                string msg1 = $"下播提醒，房间：{e.RoomId}({e.Name})";
                OperationQueue.Add(Opcode.Download.EndBroadcastingReminder, msg1, e.UID);
                Log.Info(nameof(DetectRoom_LiveEnd), msg1);
            }

            string msg2 = $"下播事件，房间：{e.RoomId}({e.Name})";
            OperationQueue.Add(Opcode.Download.StopLiveEvent, msg2, e.UID);
            Log.Info(nameof(DetectRoom_LiveEnd), msg2);
        }



        public enum TriggerType
        {
            /// <summary>
            /// 普通任务
            /// </summary>
            RegularTasks,
            /// <summary>
            /// 切换状态触发任务
            /// </summary>
            SwitchingStatesTriggersTasks,
            /// <summary>
            /// 手动触发任务
            /// </summary>
            ManuallyTriggeringTasks,
        }
    }

    public class DetectRoom
    {
        #region Private Properties
        private bool _state = false;
        private bool Initialization = false;
        private static Timer DetectTimer;
        #endregion

        #region public Properties
        public event EventHandler<(RoomCardClass Card, bool IsFirst)> LiveStart;
        public event EventHandler<RoomCardClass> LiveEnd;
        public bool State { get { return _state; } }
        #endregion

        public DetectRoom()
        {

        }

        #region Public Method
        public void start()
        {
            _state = true;
            if (!Initialization)
            {
                Log.Info(nameof(DetectRoom), $"房间状态监听已启动");
                DetectTimer = new Timer(RoomLoopDetection, null, 0, Config.Core_RunConfig._DetectIntervalTime);
                Initialization = true;
            }

        }

        public void stop()
        {
            _state = false;
        }

        /// <summary>
        /// 手动触发一个直播间的录制
        /// </summary>
        /// <param name="UID">触发的UID</param>
        public void ManuallyTriggerRecord(long UID = 0)
        {
            RoomCardClass Card = new();
            if (UID != 0 && _Room.GetCardForUID(UID, ref Card) && LiveStart != null)
            {
                if (RoomInfo.GetLiveStatus(Card.RoomId))
                {
                    (RoomCardClass Card, bool IsFirst) LiveInvoke = new();
                    LiveInvoke.Card = Card;
                    LiveInvoke.IsFirst = true;
                    LiveStart.Invoke(new List<Detect.TriggerType>() { Detect.TriggerType.ManuallyTriggeringTasks }, LiveInvoke);
                    string msg = $"手动触发一个直播间的录制，房间UID:{UID}";
                    OperationQueue.Add(Opcode.Room.ManuallyTriggeringRecordingTasks, msg, UID);
                    Log.Info(nameof(ManuallyTriggerRecord), msg);
                }
                return;
            }
        }
        #endregion

        #region Private Method


        /// <summary>
        /// 是否为第一次房间状态检测
        /// </summary>
        public static bool IsFirst = true;
        /// <summary>
        /// 检查直播间直播状态，并触发开关播事件
        /// </summary>
        /// <returns></returns>
        private void RoomLoopDetection(object o)
        {
            try
            {
                if (!_state) return;
                BatchUpdateRoomStatusForLiveStream();
                var List = _Room.GetCardListClone();
                foreach (var item in List)
                {

                    (RoomCardClass Card, bool IsFirst) LiveInvoke = new();
                    LiveInvoke.Card = List.FirstOrDefault(x => x.Value.UID == item.Value.UID).Value;
                    LiveInvoke.IsFirst = IsFirst;
                    if (LiveInvoke.Card == null)
                    {
                        continue;
                    }
                    if (LiveInvoke.Card.live_status_start_event && LiveInvoke.Card.live_status.Value == 1)
                    {
                        LiveInvoke.Card.live_status_start_event = false;
                        LiveStart?.Invoke(new List<Detect.TriggerType>() { Detect.TriggerType.RegularTasks }, LiveInvoke);
                    }
                    if (LiveInvoke.Card.live_status_end_event && LiveInvoke.Card.live_status.Value != 1)
                    {
                        LiveInvoke.Card.live_status_end_event = false;
                        LiveEnd?.Invoke(new List<Detect.TriggerType>() { Detect.TriggerType.RegularTasks }, LiveInvoke.Card);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(nameof(RoomLoopDetection), $"直播间状态轮询发生意外错误，错误信息：{e.ToString()}", e);
            }
            DetectRoom.IsFirst = false;
        }
        #endregion

        #region Public Class

        #endregion
    }
}
