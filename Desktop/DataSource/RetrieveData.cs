using Core.LogModule;
using Core.RuntimeObject;
using Desktop.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static Core.Network.Methods.Room;

namespace Desktop.DataSource
{
    internal class RetrieveData
    {
        public class UI_RoomCards
        {
            public static async void RefreshRoomCards()
            {
                Server.WebAppServices.Api.batch_complete_room_information.Data Cards = await NetWork.Post.AsyncPostBody<Server.WebAppServices.Api.batch_complete_room_information.Data>("http://127.0.0.1:11419/api/get_rooms/batch_complete_room_information");
                foreach (var item in Cards.completeInfoList)
                {
                    var card = Views.Pages.DataPage.CardsCollection.FirstOrDefault(i => i.Uid == item.uid);
                    if (card.Uid != 0)
                    {
                        if (
                            card.CoverImageUrl != item.roomInfo.coverFromUser + "@300w_300h.webp"
                            || card.FaceImageUrl != item.roomInfo.face + "@50w_50h.webp"
                            || card.Title != item.roomInfo.title
                            || card.Live_Status != item.roomInfo.liveStatus
                            || card.Nickname != item.userInfo.name
                            || card.Room_Id != item.roomId
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
                BitmapImage CoverImageBitmap = new BitmapImage();
                CoverImageBitmap.BeginInit();
                CoverImageBitmap.UriSource = new Uri(item.roomInfo.coverFromUser + "@300w_300h.webp", UriKind.RelativeOrAbsolute);
                CoverImageBitmap.EndInit();

                var FaceImageBitmap = new BitmapImage();
                FaceImageBitmap.BeginInit();
                FaceImageBitmap.UriSource = new Uri(item.roomInfo.face + "@50w_50h.webp", UriKind.RelativeOrAbsolute);
                FaceImageBitmap.EndInit();

                DataCard dataCard = new DataCard
                {
                    Uid = item.uid,
                    Room_Id=item.roomId,
                    CoverImageUrl = item.roomInfo.coverFromUser,
                    CoverImage = CoverImageBitmap,
                    FaceImageUrl = item.roomInfo.face,
                    FaceImage = FaceImageBitmap,
                    Title = item.roomInfo.title,
                    Nickname = item.userInfo.name,
                    Live_Status = item.roomInfo.liveStatus,
                    Live_Status_IsVisible = item.roomInfo.liveStatus ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed
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
                RoomCardClass Card = NetWork.Post.PostBody<RoomCardClass>("http://127.0.0.1:11419/api/get_rooms/room_information",dic);
                return Card;
            }
        }
    }
}
