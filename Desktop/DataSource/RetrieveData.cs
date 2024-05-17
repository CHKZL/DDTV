using Core.LogModule;
using Core.RuntimeObject;
using Desktop.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;
using static Core.Network.Methods.Room;

namespace Desktop.DataSource
{
    internal class RetrieveData
    {
        public class UI_RoomCards
        {
            public static void RefreshRoomCards()
            {
                Server.WebAppServices.Api.batch_complete_room_information.Data Cards = NetWork.Post.PostBody<Server.WebAppServices.Api.batch_complete_room_information.Data>($"http://127.0.0.1:{Core.Config.Web._Port}/api/get_rooms/batch_complete_room_information");

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
                            || card.DownloadSpe !=item.taskStatus.downloadRate
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
                    IsRec=item.userInfo.isAutoRec,
                    RecSign=item.userInfo.isAutoRec?new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fb7299")):new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777777")),
                    IsDanmu=item.userInfo.isRecDanmu,
                    DanmuSign=item.userInfo.isRecDanmu?new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fb7299")):new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777777")),
                    IsRemind=item.userInfo.isRemind,
                    RemindSign=item.userInfo.isRemind?new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fb7299")):new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777777")),
                    Rec_Status=item.taskStatus.isDownload,
                    Rec_Status_IsVisible=item.taskStatus.isDownload?Visibility.Visible:Visibility.Collapsed,
                    Live_Status=!item.taskStatus.isDownload&&item.roomInfo.liveStatus?true:false,
                    Live_Status_IsVisible=!item.taskStatus.isDownload&&item.roomInfo.liveStatus?Visibility.Visible:Visibility.Collapsed,
                    Rest_Status =!item.roomInfo.liveStatus?true:false,
                    Rest_Status_IsVisible=!item.roomInfo.liveStatus?Visibility.Visible:Visibility.Collapsed,
                    IsDownload=item.taskStatus.isDownload,
                    DownloadSpe=item.taskStatus.downloadRate,
                    DownloadSpe_str=item.taskStatus.isDownload?Core.Tools.Linq.ConversionSize(item.taskStatus.downloadRate, Core.Tools.Linq.ConversionSizeType.BitRate):"",
                    LiveTime=TimeSpan.FromSeconds(new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() - item.roomInfo.liveTime).Seconds,
                    LiveTime_str=item.roomInfo.liveStatus?("已直播 "+TimeSpan.FromSeconds(new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() - item.roomInfo.liveTime).ToString(@"hh\:mm\:ss")):""
                };

                return dataCard;
            }
        }

        public class RoomInfo
        {
            public static RoomCardClass GetRoomInfo(long uid)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>
                {
                    { "uid", uid.ToString() }
                };
                RoomCardClass Card = NetWork.Post.PostBody<RoomCardClass>("http://127.0.0.1:11419/api/get_rooms/room_information", dic);
                return Card;
            }
        }
    }
}
