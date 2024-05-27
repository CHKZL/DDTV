using Core.RuntimeObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Core.Network.Methods.User;

namespace Core.Network.Methods
{
    public class Room
    {
        #region Private Properties



        #endregion

        #region internal Method

        /// <summary>
        /// 获取直播间视频源信息
        /// </summary>
        /// <param name="RoomId"></param>
        /// <returns></returns>
        internal static PlayInfo_Class GetPlayInfo(long RoomId)
        {
            return _PlayInfo(RoomId);
        }

        /// <summary>
        /// 获取开播房间状态列表
        /// </summary>
        /// <param name="UIDList">要获取的直播中的房间</param>
        /// <returns></returns>
        public static UidsInfo_Class GetRoomList(List<long> UIDList)
        {
            return get_status_info_by_uids(UIDList);
        }

        /// <summary>
        /// 获取直播间状态信息
        /// </summary>
        /// <param name="RoomId"></param>
        /// <returns></returns>
        internal static RoomInfo_Class GetRoomInfo(long RoomId)
        {
            return room_init(RoomId);
        }

        #endregion

        #region Private Method

        private static PlayInfo_Class _PlayInfo(long RoomId)
        {
            string WebText = Get.GetBody($"{Config.Core_RunConfig._LiveDomainName}/xlive/web-room/v2/index/getRoomPlayInfo?room_id={RoomId}&protocol=0,1&format=0,1,2&codec=0,1&qn={Config.Core_RunConfig._DefaultResolution}&platform=web&ptype=8", true);
            PlayInfo_Class hLSHostClass = new();
            try
            {
                hLSHostClass = JsonSerializer.Deserialize<PlayInfo_Class>(WebText);
                return hLSHostClass;
            }
            catch (JsonException)
            {
                return null;
            }

        }

        /// <summary>
        /// 获取当前正在直播的直播间信息
        /// </summary>
        /// <param name="UIDList">待获取的UID列表</param>
        /// <returns></returns>
        private static UidsInfo_Class get_status_info_by_uids(List<long> UIDList)
        {
            if (UIDList == null || UIDList.Count == 0)
            {
                return null;
            }
            string LT = "{\"uids\":[" + string.Join(",", UIDList.Where(uid => uid != 0)) + "]}";
            string WebText = Post.PostBody($"{Config.Core_RunConfig._LiveDomainName}/room/v1/Room/get_status_info_by_uids", null, true, LT);
            UidsInfo_Class UserInfo_Class = new();
            try
            {
                UserInfo_Class = JsonSerializer.Deserialize<UidsInfo_Class>(WebText);
                return UserInfo_Class;
            }
            catch (JsonException)
            {
                return null;
            }

        }

        private static RoomInfo_Class room_init(long RoomId)
        {
            string WebText = Get.GetBody($"{Config.Core_RunConfig._LiveDomainName}/room/v1/Room/room_init?id=" + RoomId, true);
            RoomInfo_Class roomInfo = new();
            try
            {
                roomInfo = JsonSerializer.Deserialize<RoomInfo_Class>(WebText);
                return roomInfo;
            }
            catch (JsonException)
            {
                return null;
            }

        }

        #endregion

        #region Public Class

        public class UidsInfo_Class
        {
            public long code { get; set; }
            public string msg { get; set; }
            public string message { get; set; }
            public Dictionary<string, Data> data { get; set; }

            public class Data
            {
                public string title { get; set; }
                public long room_id { get; set; }
                public long uid { get; set; }
                public long online { get; set; }
                public long live_time { get; set; }
                public int live_status { get; set; }
                public int short_id { get; set; }
                public int area { get; set; }
                public string area_name { get; set; }
                public int area_v2_id { get; set; }
                public string area_v2_name { get; set; }
                public string area_v2_parent_name { get; set; }
                public int area_v2_parent_id { get; set; }
                public string uname { get; set; }
                public string face { get; set; }
                public string tag_name { get; set; }
                public string tags { get; set; }
                public string cover_from_user { get; set; }
                public string keyframe { get; set; }
                public string lock_till { get; set; }
                public string hidden_till { get; set; }
                public int broadcast_type { get; set; }
            }
        }

        public class RoomInfo_Class
        {
            public long code { get; set; }
            public string msg { get; set; }
            public string message { get; set; }
            public Data data { get; set; }
            public class Data
            {
                public long room_id { get; set; }
                public int short_id { get; set; }
                public long uid { get; set; }
                public int need_p2p { get; set; }
                public bool is_hidden { get; set; }
                public bool is_locked { get; set; }
                public bool is_portrait { get; set; }
                public int live_status { get; set; }
                public int hidden_till { get; set; }
                public int lock_till { get; set; }
                public bool encrypted { get; set; }
                public bool pwd_verified { get; set; }
                public long live_time { get; set; }
                public int room_shield { get; set; }
                public int is_sp { get; set; }
                public int special_type { get; set; }
            }
        }

        public class PlayInfo_Class
        {
            public long code { get; set; }
            public string message { get; set; }
            public long ttl { get; set; }
            public Data data { get; set; }

            public class Data
            {
                public long room_id { get; set; }
                public long short_id { get; set; }
                public long uid { get; set; }
                public bool is_hidden { get; set; }
                public bool is_locked { get; set; }
                public bool is_portrait { get; set; }
                public long live_status { get; set; }
                public long hidden_till { get; set; }
                public long lock_till { get; set; }
                public bool encrypted { get; set; }
                public bool pwd_verified { get; set; }
                public long live_time { get; set; }
                public long room_shield { get; set; }
                public List<long> all_special_types { get; set; }
                public Playurl_Info playurl_info { get; set; }
                public long official_type { get; set; }
                public long official_room_id { get; set; }
            }

            public class Playurl_Info
            {
                public string conf_json { get; set; }
                public Playurl playurl { get; set; }
            }

            public class Playurl
            {
                public long cid { get; set; }
                public List<G_Qn_Desc> g_qn_desc { get; set; }
                public List<Stream> stream { get; set; }
                public P2P_Data p2p_data { get; set; }
                public object dolby_qn { get; set; }
            }

            public class G_Qn_Desc
            {
                public long qn { get; set; }
                public string desc { get; set; }
                public string hdr_desc { get; set; }
                public object attr_desc { get; set; }
            }

            public class Stream
            {
                public string protocol_name { get; set; }
                public List<Format> format { get; set; }
            }

            public class Format
            {
                public string format_name { get; set; }
                public List<Codec> codec { get; set; }
            }

            public class Codec
            {
                public string codec_name { get; set; }
                public long current_qn { get; set; }
                public List<long> accept_qn { get; set; }
                public string base_url { get; set; }
                public List<Url_Info> url_info { get; set; }
                public object hdr_qn { get; set; }
                public long dolby_type { get; set; }
                public string attr_name { get; set; }
            }

            public class Url_Info
            {
                public string host { get; set; }
                public string extra { get; set; }
                public long stream_ttl { get; set; }
            }

            public class P2P_Data
            {
                public bool p2p { get; set; }
                public long p2p_type { get; set; }
                public bool m_p2p { get; set; }
                public object m_servers { get; set; }
            }
        }


        #endregion
    }
}
