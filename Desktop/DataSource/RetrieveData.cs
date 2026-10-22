using Core;
using Core.LogModule;
using Core.RuntimeObject;
using Desktop.Models;
using Desktop.Views.Pages;
using System.Windows;

namespace Desktop.DataSource
{
    /// <summary>
    /// Desktop 层的数据拉取入口，主要负责把 Core/远程 API 的数据同步到 UI 上
    /// </summary>
    internal class RetrieveData
    {
        /// <summary>
        /// 房间卡片相关的数据操作，包括拉取、排序、更新
        /// </summary>
        public class UI_RoomCards
        {
            /// <summary>
            /// 刷新房间卡片列表。先从 Core（或远程）拿数据，再和本地 UI 集合做增量同步。
            /// 避免直接 Clear 整个列表导致闪烁，采用 diff 更新的方式。
            /// </summary>
            public static async Task RefreshRoomCardsAsync()
            {
                try
                {
                    if (DataPage.CardsCollection == null)
                    {
                        return;
                    }

                    Core.RuntimeObject._Room.Overview.CardData Cards = new();

                    // 判断走远程接口还是直接调本地 Core
                    if (Core.Config.Core_RunConfig._DesktopRemoteServer || Core.Config.Core_RunConfig._LocalHTTPMode)
                    {
                        Dictionary<string, string> dir = new Dictionary<string, string>();
                        dir = new Dictionary<string, string>()
                        {
                            {"quantity", string.IsNullOrEmpty(DataPage.screen_name) ? "102" : "0" },
                            {"page", string.IsNullOrEmpty(DataPage.screen_name) ? DataPage.PageIndex.ToString() : "0" },
                            {"type", string.IsNullOrEmpty(DataPage.screen_name) ? DataPage.CardType.ToString() : "0" },
                            {"screen_name", DataPage.screen_name }
                        };
                        AddOptionalFilter(dir, "auto_rec", DataPage.GetRecConfigFilter());
                        AddOptionalFilter(dir, "rec_danmu", DataPage.GetDanmuConfigFilter());
                        AddOptionalFilter(dir, "remind", DataPage.GetRemindConfigFilter());
                        Cards = await NetWork.Post.PostBody<Core.RuntimeObject._Room.Overview.CardData>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/get_rooms/batch_complete_room_information", dir);
                    }
                    else
                    {
                        bool hasSearch = !string.IsNullOrEmpty(DataPage.screen_name);
                        Cards = Core.RuntimeObject._Room.Overview.GetCardOverview(
                            hasSearch ? 0 : 102,
                            hasSearch ? 0 : DataPage.PageIndex,
                            hasSearch ? _Room.SearchType.All : (_Room.SearchType)DataPage.CardType,
                            DataPage.screen_name,
                            DataPage.GetRecConfigFilter(),
                            DataPage.GetDanmuConfigFilter(),
                            DataPage.GetRemindConfigFilter());
                    }

                    if (Cards == null)
                    {
                        Log.Warn(nameof(RefreshRoomCardsAsync), "调用Core的API[batch_complete_room_information]获取房间信息失败，获取到的信息为Null", null, true);
                        return;
                    }

                    //卡片构建和UID集合计算放在UI线程外执行（字符串格式化/时间计算不占UI线程）
                    List<DataCard> newCards = new List<DataCard>(Cards.completeInfoList.Count);
                    foreach (var item in Cards.completeInfoList)
                    {
                        newCards.Add(CreateDataCard(item));
                    }
                    HashSet<long> webUidSet = new HashSet<long>();
                    foreach (var item in Cards.completeInfoList)
                    {
                        webUidSet.Add(item.uid);
                    }

                //异步封送到UI线程做diff：同步Invoke会让后台线程阻塞等UI线程，UI卡顿时线程池线程被占用堆积
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // 根据总数算一下需要多少页
                    int pg = (Cards.total / 102) + (Cards.total % 102 > 0 ? 1 : 0);
                    if (DataPage.PageCount != pg)
                    {
                        DataPage.PageCount = pg;
                        DataPage.UpdatePageCount(DataPage.PageCount);
                    }

                    // 1. 先 diff 出本地有、但服务端已经删掉的房间，从 UI 里移除（HashSet单趟O(n)，不再逐卡FirstOrDefault）
                    List<DataCard> toRemove = new List<DataCard>();
                    foreach (var card in Views.Pages.DataPage.CardsCollection)
                    {
                        if (!webUidSet.Contains(card.Uid))
                        {
                            toRemove.Add(card);
                        }
                    }
                    foreach (var card in toRemove)
                    {
                        Views.Pages.DataPage.CardsCollection.Remove(card);
                    }

                    // 2. 按服务端已排序的列表做索引对齐
                    // Core 层 GetCardListClone 已经排好序，直接按索引一一对应即可
                    int i = 0;
                    for (; i < newCards.Count && i < Views.Pages.DataPage.CardsCollection.Count; i++)
                    {
                        var newCard = newCards[i];

                        if (Views.Pages.DataPage.CardsCollection[i].Uid == newCard.Uid)
                        {
                            // 位置正确，检查属性是否有变化；有变化时就地更新属性（不Replace，避免卡片容器重建）
                            if (HasSignificantChanges(Views.Pages.DataPage.CardsCollection[i], newCard))
                            {
                                UpdateCard(Views.Pages.DataPage.CardsCollection[i], newCard);
                            }
                        }
                        else
                        {
                            // 位置不对，查找该卡片当前在哪
                            int existingIndex = -1;
                            for (int j = i + 1; j < Views.Pages.DataPage.CardsCollection.Count; j++)
                            {
                                if (Views.Pages.DataPage.CardsCollection[j].Uid == newCard.Uid)
                                {
                                    existingIndex = j;
                                    break;
                                }
                            }

                            if (existingIndex >= 0)
                            {
                                Views.Pages.DataPage.CardsCollection.Move(existingIndex, i);
                                if (HasSignificantChanges(Views.Pages.DataPage.CardsCollection[i], newCard))
                                {
                                    UpdateCard(Views.Pages.DataPage.CardsCollection[i], newCard);
                                }
                            }
                            else
                            {
                                // 本地没有，是新卡片
                                Views.Pages.DataPage.CardsCollection.Insert(i, newCard);
                            }
                        }
                    }

                    // 3. 服务端还有多的，Append
                    for (; i < newCards.Count; i++)
                    {
                        Views.Pages.DataPage.CardsCollection.Add(newCards[i]);
                    }

                    // 4. 本地还有多的，从末尾删除
                    while (Views.Pages.DataPage.CardsCollection.Count > newCards.Count)
                    {
                        Views.Pages.DataPage.CardsCollection.RemoveAt(Views.Pages.DataPage.CardsCollection.Count - 1);
                    }
                });
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(RefreshRoomCardsAsync), "刷新房间卡片数据时发生异常", ex);
                }
            }

            private static void AddOptionalFilter(Dictionary<string, string> parameters, string key, bool? value)
            {
                if (value.HasValue)
                {
                    parameters[key] = value.Value.ToString();
                }
            }

            /// <summary>
            /// 判断两个 DataCard 的关键属性是否有变化，避免无意义的 Replace
            /// </summary>
            private static bool HasSignificantChanges(DataCard oldCard, DataCard newCard)
            {
                return oldCard.Title != newCard.Title
                    || oldCard.Nickname != newCard.Nickname
                    || oldCard.Room_Id != newCard.Room_Id
                    || oldCard.IsRec != newCard.IsRec
                    || oldCard.IsDanmu != newCard.IsDanmu
                    || oldCard.IsRemind != newCard.IsRemind
                    || oldCard.IsDownload != newCard.IsDownload
                    || oldCard.IsDanmaRecording != newCard.IsDanmaRecording
                    || oldCard.Rec_Status != newCard.Rec_Status
                    || oldCard.Live_Status != newCard.Live_Status
                    || oldCard.DownloadSpe != newCard.DownloadSpe
                    || oldCard.DownloadSpe_str != newCard.DownloadSpe_str
                    || oldCard.LiveTime != newCard.LiveTime
                    || oldCard.LiveTime_str != newCard.LiveTime_str;
            }

            /// <summary>
            /// 把新卡片的数据就地更新到现有卡片实例上（通过属性赋值触发绑定更新，
            /// 避免Replace整个对象导致的CollectionChanged和卡片容器重建）
            /// </summary>
            private static void UpdateCard(DataCard target, DataCard source)
            {
                target.Room_Id = source.Room_Id;
                target.Title = source.Title;
                target.Nickname = source.Nickname;
                target.IsRec = source.IsRec;
                target.IsDanmu = source.IsDanmu;
                target.IsRemind = source.IsRemind;
                target.Rec_Status = source.Rec_Status;
                target.Live_Status = source.Live_Status;
                target.DownloadSpe = source.DownloadSpe;
                target.DownloadSpe_str = source.DownloadSpe_str;
                target.IsDownload = source.IsDownload;
                target.LiveTime = source.LiveTime;
                target.LiveTime_str = source.LiveTime_str;
                target.IsDanmaRecording = source.IsDanmaRecording;
            }

            /// <summary>
            /// 把 Core 返回的原始数据转成 UI 用的 DataCard
            /// </summary>
            private static DataCard CreateDataCard(Core.RuntimeObject._Room.Overview.CardData.CompleteInfo item)
            {
                long liveSeconds = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() - item.roomInfo.liveTime;
                DataCard dataCard = new DataCard
                {
                    Uid = item.uid,
                    Room_Id = item.roomId,
                    Title = item.roomInfo.title,
                    Nickname = item.userInfo.name,
                    IsRec = item.userInfo.isAutoRec,
                    IsDanmu = item.userInfo.isRecDanmu,
                    IsRemind = item.userInfo.isRemind,
                    IsDownload = item.taskStatus.isDownload,
                    IsDanmaRecording = item.taskStatus.isDanma,
                    Rec_Status = item.taskStatus.isDownload,
                    Live_Status = item.roomInfo.liveStatus,
                    DownloadSpe = item.taskStatus.downloadRate,
                    DownloadSpe_str = item.taskStatus.isDownload ? Core.Tools.Linq.ConversionSize(item.taskStatus.downloadRate, Core.Tools.Linq.ConversionSizeType.BitRate) : "",
                    LiveTime = liveSeconds,
                    LiveTime_str = item.roomInfo.liveStatus ? ("已直播 " + TimeSpan.FromSeconds(liveSeconds).ToString(@"hh\:mm\:ss")) : ""
                };
                return dataCard;
            }
        }

        /// <summary>
        /// 房间配置修改，根据当前模式决定走远程 HTTP 还是本地 Core 调用
        /// </summary>
        public class RoomInfo
        {
            public static async Task<bool> ModifyRoomSettingsAsync(long uid, bool IsAutoRec, bool IsRecDanmu, bool IsRemind)
            {
                if (Core.Config.Core_RunConfig._DesktopRemoteServer || Core.Config.Core_RunConfig._LocalHTTPMode)
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>
                    {
                        {"uid", uid.ToString() },
                        {"AutoRec",IsAutoRec.ToString() },
                        {"Remind",IsRemind.ToString() },
                        {"RecDanmu",IsRecDanmu.ToString() },
                    };
                    if (await NetWork.Post.PostBody<bool>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/set_rooms/modify_room_settings", dic))
                    {
                        Log.Info(nameof(ModifyRoomSettingsAsync), "调用Core的API[modify_room_settings]修改房间配置成功");
                        return true;
                    }
                    else
                    {
                        Log.Warn(nameof(ModifyRoomSettingsAsync), "调用Core的API[modify_room_settings]修改房间配置失败");
                        return false;
                    }
                }
                else
                {
                    if (await Task.Run(() => Core.RuntimeObject._Room.ModifyRoomSettings(uid, IsAutoRec, IsRemind, IsRecDanmu)))
                    {
                        Log.Info(nameof(ModifyRoomSettingsAsync), "调用Core的API[modify_room_settings]修改房间配置成功");
                        return true;
                    }
                    else
                    {
                        Log.Warn(nameof(ModifyRoomSettingsAsync), "调用Core的API[modify_room_settings]修改房间配置失败");
                        return false;
                    }
                }
            }
        }
    }
}
