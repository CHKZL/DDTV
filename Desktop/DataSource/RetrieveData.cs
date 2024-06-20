using Core;
using Core.LogModule;
using Core.RuntimeObject;
using Desktop.Models;
using Desktop.Views.Pages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;
using static Core.Network.Methods.Room;
using static Desktop.Views.Pages.DataPage;





namespace Desktop.DataSource
{
    internal class RetrieveData
    {
        public class UI_RoomCards
        {
            public static void RefreshRoomCards()
            {
                if (DataPage.CardsCollection == null)
                {
                    return;
                }
                Dictionary<string, string> dir = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(DataPage.screen_name))
                {
                    dir = new Dictionary<string, string>()
                    {
                        {"screen_name",DataPage.screen_name }
                    };
                }
                else
                {
                    dir = new Dictionary<string, string>()
                    {
                        {"quantity","102" },
                        {"page",DataPage.PageIndex.ToString() },
                        {"type",DataPage.CardType.ToString() },
                        {"screen_name","" }
                    };
                }
                Server.WebAppServices.Api.batch_complete_room_information.Data Cards = NetWork.Post.PostBody<Server.WebAppServices.Api.batch_complete_room_information.Data>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/get_rooms/batch_complete_room_information", dir).Result;

                if (Cards == null)
                {
                    Log.Warn(nameof(RefreshRoomCards), "调用Core的API[batch_complete_room_information]获取房间信息失败，获取到的信息为Null", null, true);
                    return;
                };
                Application.Current.Dispatcher.Invoke(() =>
                {

                    int pg = (Cards.total / 102) + (Cards.total % 102 > 0 ? 1 : 0);
                    if (DataPage.PageCount != pg)
                    {
                        DataPage.PageCount = pg;
                        DataPage.UpdatePageCount(DataPage.PageCount);
                    }

                    List<long> _uid_Web = new List<long>();
                    foreach (var item in Cards.completeInfoList)
                    {
                        _uid_Web.Add(item.uid);
                    }
                    List<long> _uid_local = new List<long>();
                    foreach (var item in Views.Pages.DataPage.CardsCollection)
                    {
                        _uid_local.Add(item.Uid);
                    }
                    List<long> result = _uid_local.Except(_uid_Web).ToList();
                    foreach (var item in result)
                    {
                        Views.Pages.DataPage.CardsCollection.Remove(Views.Pages.DataPage.CardsCollection.FirstOrDefault(i => i.Uid == item));
                    }


                    foreach (var item in Cards.completeInfoList)
                    {
                        var card = Views.Pages.DataPage.CardsCollection.FirstOrDefault(i => i.Uid == item.uid);
                        if (card.Uid != 0)
                        {
                            if (
                                card.Title != item.roomInfo.title
                                || card.Live_Status != item.roomInfo.liveStatus
                                || card.Nickname != item.userInfo.name
                                || card.Room_Id != item.roomId
                                || card.IsRec != item.userInfo.isAutoRec
                                || card.IsDanmu != item.userInfo.isRecDanmu
                                || card.IsRemind != item.userInfo.isRemind
                                || card.IsDownload != item.taskStatus.isDownload
                                || card.DownloadSpe != item.taskStatus.downloadRate
                                || card.LiveTime != TimeSpan.FromSeconds(new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() - item.roomInfo.liveTime).Seconds
                                )
                            {
                                DataCard dataCard = CreateDataCard(item);
                                int index = Views.Pages.DataPage.CardsCollection.IndexOf(card);
                                Views.Pages.DataPage.CardsCollection[index] = dataCard;
                            }
                        }
                        else
                        {
                            DataCard dataCard = CreateDataCard(item);
                            Views.Pages.DataPage.CardsCollection.Add(dataCard);

                        }
                    }
                });
            }

            private static DataCard CreateDataCard(Server.WebAppServices.Api.batch_complete_room_information.Data.CompleteInfo item)
            {
                DataCard dataCard = new DataCard
                {
                    Uid = item.uid,
                    Room_Id = item.roomId,
                    Title = item.roomInfo.title,
                    Nickname = item.userInfo.name,
                    //Live_Status = item.roomInfo.liveStatus,
                    //Live_Status_IsVisible = item.roomInfo.liveStatus ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed,
                    IsRec = item.userInfo.isAutoRec,
                    RecSign = item.userInfo.isAutoRec ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fb7299")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777777")),
                    IsDanmu = item.userInfo.isRecDanmu,
                    DanmuSign = item.userInfo.isRecDanmu ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fb7299")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777777")),
                    IsRemind = item.userInfo.isRemind,
                    RemindSign = item.userInfo.isRemind ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fb7299")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777777")),

                    Rec_Status = item.taskStatus.isDownload,
                    Rec_Status_IsVisible = item.taskStatus.isDownload ? Visibility.Visible : Visibility.Collapsed,

                    Live_Status = !item.taskStatus.isDownload && item.roomInfo.liveStatus ? true : false,
                    Live_Status_IsVisible = !item.taskStatus.isDownload && item.roomInfo.liveStatus ? Visibility.Visible : Visibility.Collapsed,

                    Rest_Status = !item.roomInfo.liveStatus ? true : false,
                    Rest_Status_IsVisible = (!item.roomInfo.liveStatus && !item.taskStatus.isDownload) ? Visibility.Visible : Visibility.Collapsed,

                    IsDownload = item.taskStatus.isDownload,
                    DownloadSpe = item.taskStatus.downloadRate,
                    DownloadSpe_str = item.taskStatus.isDownload ? Core.Tools.Linq.ConversionSize(item.taskStatus.downloadRate, Core.Tools.Linq.ConversionSizeType.BitRate) : "",
                    LiveTime = TimeSpan.FromSeconds(new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() - item.roomInfo.liveTime).Seconds,
                    LiveTime_str = item.roomInfo.liveStatus ? ("已直播 " + TimeSpan.FromSeconds(new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() - item.roomInfo.liveTime).ToString(@"hh\:mm\:ss")) : ""
                };

                return dataCard;
            }
        }

        public class RoomInfo
        {
            public static void ModifyRoomSettings(long uid, bool IsAutoRec, bool IsRecDanmu, bool IsRemind)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>
                {
                    {"uid", uid.ToString() },
                    {"AutoRec",IsAutoRec.ToString() },
                    {"Remind",IsRemind.ToString() },
                    {"RecDanmu",IsRecDanmu.ToString() },
                };
                Task.Run(() =>
                {
                    if (NetWork.Post.PostBody<bool>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/set_rooms/modify_room_settings", dic).Result)
                    {
                        Log.Info(nameof(ModifyRoomSettings), "调用Core的API[batch_delete_rooms]修改房间配置成功");
                    }
                    else
                    {
                        Log.Warn(nameof(ModifyRoomSettings), "调用Core的API[batch_delete_rooms]修改房间配置失败");
                    }
                });
            }
        }
    }
}
