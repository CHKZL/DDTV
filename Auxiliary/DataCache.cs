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
        public static int BilibiliApiCount = 0;
    }

    public class GetRoomInfoOldAPIRequestBuilder : BiliAPIRequest, IAPIRequestBuilder
    {
        public GetRoomInfoOldAPIRequestBuilder()
        {
            BaseUrl = "https://api.live.bilibili.com/room/v1/Room/getRoomInfoOld";
            Platform = APIPlatforms.Bilibili;
            ExceptionString = "获取房间信息（旧API）失败";
        }

        public IAPIRequest Build(string args)
        {
            Args = new Dictionary<string, string>
            {
                { "mid", args }
            };

            return this;
        }
    }

    public class GetRoomInfoAPIRequestBuilder : BiliAPIRequest, IAPIRequestBuilder
    {
        public GetRoomInfoAPIRequestBuilder()
        {
            BaseUrl = "https://api.live.bilibili.com/room/v1/Room/get_info";
            Platform = APIPlatforms.Bilibili;
            ExceptionString = "获取房间信息（旧API）失败";
        }

        public IAPIRequest Build(string args)
        {
            Args = new Dictionary<string, string>
            {
                { "id", args }
            };

            return this;
        }
    }

    public class ResponseJObjectToRoomIDPipe : IPipe
    {
        public object Invoke(object input)
        {
            var jObject = (JObject)input;

            var roomid = jObject["data"]["roomid"].ToString();
            return roomid;
        }
    }

    public class ResponseJObjectToRealRoomIDPipe : IPipe
    {
        public object Invoke(object input)
        {
            var jObject = (JObject)input;

            var live_status = jObject["data"]["live_status"].ToString();
            if (live_status != "1")
            {
                return "-1";
            }
            var roomid = jObject["data"]["room_id"].ToString();
            return roomid;
        }
    }

    public class ResponseJObjectToRoomTitlePipe : IPipe
    {
        public object Invoke(object input)
        {
            var jObject = (JObject)input;

            string roomName = jObject["data"]["title"].ToString()
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

    public class ResponseJObjectToRoomInfoObjectPipe : IPipe
    {
        public object Invoke(object input)
        {
            var result = (JObject)input;
            var originalRoomId = result["@@"].ToString();

            if (result["data"]["room_id"].ToString() != originalRoomId)
            {
                for (int i = 0; i < bilibili.RoomList.Count(); i++)
                {
                    if (bilibili.RoomList[i].房间号 == originalRoomId)
                    {
                        bilibili.RoomList[i].房间号 = result["data"]["room_id"].ToString();
                        break;
                    }
                }
            }

            var roominfo = new RoomInit.RoomInfo
            {
                房间号 = result["data"]["room_id"].ToString(),
                标题 = result["data"]["title"].ToString().Replace(" ", "").Replace("/", "").Replace("\\", "").Replace("\"", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("<", "").Replace(">", "").Replace("|", ""),
                直播状态 = result["data"]["live_status"].ToString() == "1" ? true : false,
                UID = result["data"]["uid"].ToString(),
                直播开始时间 = result["data"]["live_time"].ToString(),
                平台 = "bilibili"
            };

            InfoLog.InfoPrintf("获取到房间信息:" + roominfo.UID + " " + (roominfo.直播状态 ? "已开播" : "未开播") + " " + (roominfo.直播状态 ? "开播时间:" + roominfo.直播开始时间 : ""), InfoLog.InfoClass.Debug);

            return roominfo;
        }
    }

    public class BiliCache
    {
        public WebClient GeneralWebClient { get; private set; }

        private static readonly Lazy<BiliCache> _lazy = new Lazy<BiliCache>(() =>
        {
            var instance = new BiliCache();
            var wc = new WebClient();
            instance.GeneralWebClient = wc;

            wc.Headers.Add("Accept: */*");
            wc.Headers.Add("User-Agent: " + MMPU.UA.Ver.UA());
            wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            if (!string.IsNullOrEmpty(MMPU.Cookie))
            {
                wc.Headers.Add("Cookie", MMPU.Cookie);
            }

            var pipeline = new Pipeline<IAPIRequest, string>();
            pipeline.AddStep(new APIRequestToResponseJObjectPipe());
            pipeline.AddStep(new ResponseJObjectToRoomIDPipe());
            instance.UIDsToRoomIDs = new CachedObjectCollection<string>(
                new GetRoomInfoOldAPIRequestBuilder
                {
                    WebClient = wc
                }, pipeline);

            pipeline = new Pipeline<IAPIRequest, string>();
            pipeline.AddStep(new APIRequestToResponseJObjectPipe());
            pipeline.AddStep(new ResponseJObjectToRoomTitlePipe());
            instance.RoomIDsToTitles = new CachedObjectCollection<string>(
                new GetRoomInfoAPIRequestBuilder
                {
                    WebClient = wc
                }, pipeline, 3600);

            pipeline = new Pipeline<IAPIRequest, string>();
            pipeline.AddStep(new APIRequestToResponseJObjectPipe());
            pipeline.AddStep(new ResponseJObjectToRealRoomIDPipe());
            instance.RoomIDsToRealRoomIDs = new CachedObjectCollection<string>(
                new GetRoomInfoAPIRequestBuilder
                {
                    WebClient = wc
                }, pipeline);

            var pipeline2 = new Pipeline<IAPIRequest, RoomInit.RoomInfo>();
            pipeline2.AddStep(new APIRequestToResponseJObjectWithArgPipe());
            pipeline2.AddStep(new ResponseJObjectToRoomInfoObjectPipe());
            instance.RoomIDsToRoomInfos = new CachedObjectCollection<RoomInit.RoomInfo>(
                new GetRoomInfoAPIRequestBuilder
                {
                    WebClient = wc
                }, pipeline2, 0/*不缓存*/);

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