using Core.Account;
using Core.LogModule;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Core.RuntimeObject
{
    /// <summary>
    /// 观看时长心跳(x25Kn)会话管理器。
    /// 播放窗口打开时Register、关闭时Unregister；同一房间多个窗口共享一条心跳会话(引用计数)，
    /// 仅当配置项_PlayWindowWatchHeartbeat打开时才会实际启动会话。
    /// </summary>
    public static class WatchHeartbeatManager
    {
        /// <summary>
        /// 按房间号管理的心跳会话（key:真实房间号）
        /// </summary>
        private static readonly ConcurrentDictionary<long, HeartbeatSession> _sessions = new();

        /// <summary>
        /// 注册一个观看者。配置开关关闭时直接忽略，由本管理器统一收口开关判断
        /// </summary>
        /// <param name="roomId">真实房间号(长号)</param>
        /// <param name="anchorUid">主播UID</param>
        /// <param name="tag">注册方标识(如"VlcPlayWindow")，用于多窗口引用计数</param>
        public static void Register(long roomId, long anchorUid, string tag)
        {
            if (!Config.Core_RunConfig._PlayWindowWatchHeartbeat)
            {
                return;
            }
            if (roomId <= 0)
            {
                return;
            }
            var session = _sessions.GetOrAdd(roomId, rid =>
            {
                var s = new HeartbeatSession(rid, anchorUid);
                s.Start();
                return s;
            });
            session.AddTag(tag);
        }

        /// <summary>
        /// 注销一个观看者；该房间的引用清零后停止心跳会话
        /// </summary>
        public static void Unregister(long roomId, string tag)
        {
            if (_sessions.TryGetValue(roomId, out var session) && session.RemoveTag(tag))
            {
                if (_sessions.TryRemove(roomId, out var s))
                {
                    s.Stop();
                }
            }
        }

        /// <summary>
        /// 停止所有心跳会话（配置开关被关闭时由设置页调用，使关闭操作即时生效）
        /// </summary>
        public static void StopAll()
        {
            foreach (var kv in _sessions)
            {
                if (_sessions.TryRemove(kv.Key, out var s))
                {
                    s.Stop();
                }
            }
        }

        /// <summary>
        /// 单房间的观看心跳会话：取分区信息 → 发E → 按服务器给定间隔循环发X(链式递推secret)
        /// </summary>
        internal class HeartbeatSession
        {
            private readonly long _roomId;
            private readonly long _anchorUid;
            private readonly HashSet<string> _tags = new();
            private readonly object _tagLock = new();
            private readonly CancellationTokenSource _cts = new();
            /// <summary>
            /// 停止标志(0=运行中，1=已请求停止)，防止Stop重入
            /// </summary>
            private int _stopFlag = 0;
            /// <summary>
            /// 连续失败上限：E/X持续失败(如登录态失效、协议变更)时放弃，避免无限重试刷请求
            /// </summary>
            private const int MaxConsecutiveFailures = 3;

            public HeartbeatSession(long roomId, long anchorUid)
            {
                _roomId = roomId;
                _anchorUid = anchorUid;
            }

            public void AddTag(string tag)
            {
                lock (_tagLock)
                {
                    _tags.Add(tag);
                }
            }

            /// <summary>
            /// 移除引用，返回是否已清零(清零后应由管理器停止本会话)
            /// </summary>
            public bool RemoveTag(string tag)
            {
                lock (_tagLock)
                {
                    _tags.Remove(tag);
                    return _tags.Count == 0;
                }
            }

            public void Start()
            {
                Task.Run(RunAsync);
            }

            public void Stop()
            {
                if (Interlocked.Exchange(ref _stopFlag, 1) == 0)
                {
                    _cts.Cancel();
                }
            }

            private async Task RunAsync()
            {
                int failures = 0;
                try
                {
                    while (!_cts.IsCancellationRequested)
                    {
                        AccountInformation account = Account.AccountInformation;
                        if (account == null || !account.State)
                        {
                            Log.Info(nameof(WatchHeartbeatManager), $"房间号:[{_roomId}]，未登录，观看时长心跳不启动");
                            return;
                        }
                        if (!RoomInfo.GetLiveStatus(_roomId))
                        {
                            Log.Info(nameof(WatchHeartbeatManager), $"房间号:[{_roomId}]，已下播，观看时长心跳停止");
                            return;
                        }

                        //分区信息获取失败时按0,0兜底（room_id是结算的关键字段），并记警告
                        var area = Network.Methods.WatchHeartbeat.GetRoomAreaInfo(_roomId);
                        if (area == null)
                        {
                            Log.Warn(nameof(WatchHeartbeatManager), $"房间号:[{_roomId}]，获取分区信息失败，观看心跳按默认分区参数继续", null, false);
                        }
                        string ua = Config.Core_RunConfig._HTTP_UA;
                        string deviceJson = Network.Methods.WatchHeartbeat.BuildDeviceJson(account);
                        string idJson = Network.Methods.WatchHeartbeat.BuildIdJson(area?.ParentAreaId ?? 0, area?.AreaId ?? 0, _roomId);

                        var secret = Network.Methods.WatchHeartbeat.SendE(idJson, deviceJson, _anchorUid, ua, account);
                        if (secret == null)
                        {
                            failures++;
                            Log.Warn(nameof(WatchHeartbeatManager), $"房间号:[{_roomId}]，观看心跳E请求失败(连续第{failures}次)", null, false);
                            if (failures >= MaxConsecutiveFailures)
                            {
                                Log.Warn(nameof(WatchHeartbeatManager), $"房间号:[{_roomId}]，观看心跳连续失败已达上限({MaxConsecutiveFailures}次)，停止尝试", null, false);
                                return;
                            }
                            await Task.Delay(TimeSpan.FromSeconds(10), _cts.Token);
                            continue;
                        }
                        failures = 0;
                        Log.Info(nameof(WatchHeartbeatManager), $"房间号:[{_roomId}]，观看时长心跳已启动(间隔{secret.HeartbeatInterval}秒)");

                        //X心跳循环：间隔以每次响应的heartbeat_interval为准，secret链式递推
                        while (!_cts.IsCancellationRequested)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(secret.HeartbeatInterval), _cts.Token);
                            if (!RoomInfo.GetLiveStatus(_roomId))
                            {
                                Log.Info(nameof(WatchHeartbeatManager), $"房间号:[{_roomId}]，已下播，观看时长心跳停止");
                                return;
                            }
                            account = Account.AccountInformation;
                            if (account == null || !account.State)
                            {
                                Log.Info(nameof(WatchHeartbeatManager), $"房间号:[{_roomId}]，登录态失效，观看时长心跳停止");
                                return;
                            }
                            var next = Network.Methods.WatchHeartbeat.SendX(idJson, deviceJson, _anchorUid, ua, account, secret);
                            if (next == null)
                            {
                                failures++;
                                Log.Warn(nameof(WatchHeartbeatManager), $"房间号:[{_roomId}]，观看心跳X请求失败(连续第{failures}次)，准备重建心跳链", null, false);
                                if (failures >= MaxConsecutiveFailures)
                                {
                                    Log.Warn(nameof(WatchHeartbeatManager), $"房间号:[{_roomId}]，观看心跳连续失败已达上限({MaxConsecutiveFailures}次)，停止尝试", null, false);
                                    return;
                                }
                                //跳出X循环，回到外层重新发E重建secret链
                                break;
                            }
                            failures = 0;
                            secret = next;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    //窗口关闭/开关关闭触发的正常停止
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(WatchHeartbeatManager), $"房间号:[{_roomId}]，观看时长心跳循环出现异常", ex, false);
                }
                finally
                {
                    Log.Info(nameof(WatchHeartbeatManager), $"房间号:[{_roomId}]，观看时长心跳已停止");
                }
            }
        }
    }
}
