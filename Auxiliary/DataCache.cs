using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    public class DataCache
    {
        /// <summary>
        /// 一个通用的WebClient。通过单例模式保证全局唯一。
        /// </summary>
        public WebClient GeneralWebClient { get; private set; }

        private static readonly Lazy<DataCache> _lazy = new Lazy<DataCache>(() =>
        {
            var instance = new DataCache();
            var wc = new WebClient();
            instance.GeneralWebClient = wc;

            wc.Headers.Add("Accept: */*");
            wc.Headers.Add("User-Agent: " + MMPU.UA.Ver.UA());
            wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            if (!string.IsNullOrEmpty(MMPU.Cookie))
            {
                wc.Headers.Add("Cookie", MMPU.Cookie);
            }

            return instance;
        });

        private DataCache() { }

        public static DataCache Instance { get { return _lazy.Value; } }

        public static int BilibiliApiCount = 0;
    }


    public class UIDToRoomIDHandler : BaseRequestHandler<string>
    {
        protected override WebClient getWebClient()
        {
            return DataCache.Instance.GeneralWebClient;
        }

        protected override IAPIRequest buildRequest(string id)
        {
            return new BiliAPIRequest
            {
                BaseUrl = "https://api.live.bilibili.com/room/v1/Room/getRoomInfoOld",
                ExceptionString = "获取房间信息（旧API）失败",
                Args = new Dictionary<string, string>
                {
                    { "mid", id }
                }
            };
        }

        protected override string responseToResult(JObject responseJObject)
        {
            if (responseJObject == null) return "";

            return responseJObject["data"]["roomid"].ToString();
        }
    }

    public class RoomIDToTitleHandler : BaseRequestHandler<string>
    {
        protected override WebClient getWebClient()
        {
            return DataCache.Instance.GeneralWebClient;
        }

        protected override IAPIRequest buildRequest(string id)
        {
            return new BiliAPIRequest
            {
                BaseUrl = "https://api.live.bilibili.com/room/v1/Room/get_info",
                ExceptionString = "获取房间信息失败",
                Args = new Dictionary<string, string>
                {
                    { "id", id }
                }
            };
        }

        protected override string responseToResult(JObject responseJObject)
        {
            if (responseJObject == null) return "";

            string roomName = responseJObject["data"]["title"].ToString()
                 .Replace(" ", "")
                 .Replace("/", "")
                 .Replace("\\", "")
                 .Replace("\"", "")
                 .Replace(":", "")
                 .Replace("*", "")
                 .Replace("?", "")
                 .Replace("<", "")
                 .Replace(">", "")
                 .Replace("|", "")
                 .ToString();

            InfoLog.InfoPrintf("根据RoomId获取到标题:" + roomName, InfoLog.InfoClass.Debug);

            return roomName;
        }
    }

    public class RoomIDToRealRoomIDHandler : BaseRequestHandler<string>
    {
        protected override WebClient getWebClient()
        {
            return DataCache.Instance.GeneralWebClient;
        }

        protected override IAPIRequest buildRequest(string id)
        {
            return new BiliAPIRequest
            {
                BaseUrl = "https://api.live.bilibili.com/room/v1/Room/get_info",
                ExceptionString = "获取房间信息失败",
                Args = new Dictionary<string, string>
                {
                    { "id", id }
                }
            };
        }

        protected override string responseToResult(JObject responseJObject)
        {
            if (responseJObject == null) return "";

            var live_status = responseJObject["data"]["live_status"].ToString();
            if (live_status != "1")
            {
                return "-1";
            }
            var roomid = responseJObject["data"]["room_id"].ToString();
            return roomid;
        }
    }

    public class RoomIDToRoomInfoHandler : BaseRequestHandler<RoomInit.RoomInfo>
    {
        protected override WebClient getWebClient()
        {
            return DataCache.Instance.GeneralWebClient;
        }

        protected override IAPIRequest buildRequest(string id)
        {
            return new BiliAPIRequest
            {
                BaseUrl = "https://api.live.bilibili.com/room/v1/Room/get_info",
                ExceptionString = "获取房间信息失败",
                Args = new Dictionary<string, string>
                {
                    { "id", id }
                }
            };
        }

        protected override RoomInit.RoomInfo responseToResult(JObject responseJObject)
        {
            if (responseJObject == null) return null;

            if (responseJObject["data"]["room_id"].ToString() != _id)
            {
                for (int i = 0; i < bilibili.RoomList.Count(); i++)
                {
                    if (bilibili.RoomList[i].房间号 == _id)
                    {
                        bilibili.RoomList[i].房间号 = responseJObject["data"]["room_id"].ToString();
                        break;
                    }
                }
            }

            var roominfo = new RoomInit.RoomInfo
            {
                房间号 = responseJObject["data"]["room_id"].ToString(),
                标题 = responseJObject["data"]["title"].ToString().Replace(" ", "").Replace("/", "").Replace("\\", "").Replace("\"", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("<", "").Replace(">", "").Replace("|", ""),
                直播状态 = responseJObject["data"]["live_status"].ToString() == "1" ? true : false,
                UID = responseJObject["data"]["uid"].ToString(),
                直播开始时间 = responseJObject["data"]["live_time"].ToString(),
                平台 = "bilibili"
            };

            InfoLog.InfoPrintf("获取到房间信息:" + roominfo.UID + " " + (roominfo.直播状态 ? "已开播" : "未开播") + " " + (roominfo.直播状态 ? "开播时间:" + roominfo.直播开始时间 : ""), InfoLog.InfoClass.Debug);

            return roominfo;
        }
    }


    public class BiliCache
    {
        private static readonly Lazy<BiliCache> _lazy = new Lazy<BiliCache>(() =>
        {
            var instance = new BiliCache();

            instance.UIDsToRoomIDs = new CachedObjectCollection<string>(
                new UIDToRoomIDHandler());
            instance.RoomIDsToTitles = new CachedObjectCollection<string>(
                new RoomIDToTitleHandler(), 3600);
            instance.RoomIDsToRealRoomIDs = new CachedObjectCollection<string>(
                new RoomIDToRealRoomIDHandler());
            instance.RoomIDsToRoomInfos = new CachedObjectCollection<RoomInit.RoomInfo>(
                new RoomIDToRoomInfoHandler(), 0/*不缓存（超时时间为0）*/);

            return instance;
        });

        public static BiliCache Instance { get { return _lazy.Value; } }

        private BiliCache() { }


        public CachedObjectCollection<string> UIDsToRoomIDs { get; private set; }
        public CachedObjectCollection<string> RoomIDsToTitles { get; private set; }
        public CachedObjectCollection<string> RoomIDsToRealRoomIDs { get; private set; }
        public CachedObjectCollection<RoomInit.RoomInfo> RoomIDsToRoomInfos { get; private set; }
    }
}