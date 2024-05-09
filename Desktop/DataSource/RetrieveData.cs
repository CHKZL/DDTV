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
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;
using static Core.Network.Methods.Room;

namespace Desktop.DataSource
{
    internal class RetrieveData
    {
        public class UI_RoomCards
        {
            private static Dictionary<long, CardImage> CardImagePairs = new Dictionary<long, CardImage>();
            public class CardImage
            {
                public string CoverImageUrl { get; set; }
                public BitmapImage CoverImageBitmap { get; set; }
                public string FaceImageUrl { get; set; }
                public BitmapImage FaceImageBitmap { get; set; }
            }
            public static void RefreshRoomCards()
            {
                Server.WebAppServices.Api.batch_complete_room_information.Data Cards = NetWork.Post.PostBody<Server.WebAppServices.Api.batch_complete_room_information.Data>("http://127.0.0.1:11419/api/get_rooms/batch_complete_room_information");

                foreach (var item in Cards.completeInfoList)
                {

                 
                        if (CardImagePairs.TryGetValue(item.uid, out CardImage cardImage))
                        {
                            if (cardImage.CoverImageUrl != item.roomInfo.coverFromUser + "@300w_300h.webp")
                            {
                                BitmapImage CoverImageBitmap = new BitmapImage();
                                if (!string.IsNullOrEmpty(item.roomInfo.coverFromUser))
                                {
                                    CoverImageBitmap.BeginInit();
                                    CoverImageBitmap.UriSource = new Uri(item.roomInfo.coverFromUser + "@300w_300h.webp", UriKind.RelativeOrAbsolute);
                                    CoverImageBitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    CoverImageBitmap.EndInit();
                                    cardImage.CoverImageBitmap = CoverImageBitmap;
                                    cardImage.CoverImageUrl = item.roomInfo.coverFromUser + "@300w_300h.webp";
                                }
                            }
                            if (cardImage.FaceImageUrl != item.roomInfo.face + "@50w_50h.webp")
                            {
                                BitmapImage FaceImageBitmap = new BitmapImage();
                                if (!string.IsNullOrEmpty(item.roomInfo.face))
                                {

                                    FaceImageBitmap.BeginInit();
                                    FaceImageBitmap.UriSource = new Uri(item.roomInfo.face + "@50w_50h.webp", UriKind.RelativeOrAbsolute);
                                    FaceImageBitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    FaceImageBitmap.EndInit();
                                    CardImagePairs[item.uid].FaceImageBitmap = FaceImageBitmap;
                                    cardImage.FaceImageUrl = item.roomInfo.face + "@50w_50h.webp";
                                }
                            }

                        }
                        else
                        {
                            CardImage CI = new CardImage();
                            if (!string.IsNullOrEmpty(item.roomInfo.coverFromUser))
                            {
                                BitmapImage CoverImageBitmap = new BitmapImage();
                                CoverImageBitmap.BeginInit();
                                CoverImageBitmap.UriSource = new Uri(item.roomInfo.coverFromUser + "@300w_300h.webp", UriKind.RelativeOrAbsolute);
                                CoverImageBitmap.CacheOption = BitmapCacheOption.OnLoad;
                                CoverImageBitmap.EndInit();
                                CI.CoverImageBitmap = CoverImageBitmap;
                                CI.CoverImageUrl = item.roomInfo.coverFromUser + "@300w_300h.webp";
                            }

                            if (!string.IsNullOrEmpty(item.roomInfo.face))
                            {
                                BitmapImage FaceImageBitmap = new BitmapImage();
                                FaceImageBitmap.BeginInit();
                                FaceImageBitmap.UriSource = new Uri(item.roomInfo.face + "@50w_50h.webp", UriKind.RelativeOrAbsolute);
                                FaceImageBitmap.CacheOption = BitmapCacheOption.OnLoad;
                                FaceImageBitmap.EndInit();
                                CI.FaceImageBitmap = FaceImageBitmap;
                                CI.FaceImageUrl = item.roomInfo.face + "@50w_50h.webp";
                            }
                            CardImagePairs.Add(item.uid, CI);
                        }
                }


                foreach (var item in Cards.completeInfoList)
                {
                    var card = Views.Pages.DataPage.CardsCollection.FirstOrDefault(i => i.Uid == item.uid);
                    if (card.Uid != 0)
                    {
                        if (
                            //card.CoverImageUrl != item.roomInfo.coverFromUser + "@300w_300h.webp"
                            //|| card.FaceImageUrl != item.roomInfo.face + "@50w_50h.webp"
                            card.CoverImageUrl != (CardImagePairs.ContainsKey(card.Uid) ? CardImagePairs[card.Uid].CoverImageUrl : "")
                            || card.FaceImageUrl != (CardImagePairs.ContainsKey(card.Uid) ? CardImagePairs[card.Uid].FaceImageUrl : "")
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
                DataCard dataCard = new DataCard
                {
                    Uid = item.uid,
                    Room_Id = item.roomId,
                    CoverImageUrl = item.roomInfo.coverFromUser,
                    CoverImage = CardImagePairs.ContainsKey(item.uid) ? CardImagePairs[item.uid].CoverImageBitmap : new BitmapImage(),
                    FaceImageUrl = item.roomInfo.face,
                    FaceImage = CardImagePairs.ContainsKey(item.uid) ? CardImagePairs[item.uid].FaceImageBitmap : new BitmapImage(),
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
                RoomCardClass Card = NetWork.Post.PostBody<RoomCardClass>("http://127.0.0.1:11419/api/get_rooms/room_information", dic);
                return Card;
            }
        }
    }
}
