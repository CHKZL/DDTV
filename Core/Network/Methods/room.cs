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

        #region Public Method

        public static UidsInfo_Class? GetRoomList(List<long> UIDList)
        {
            return get_status_info_by_uids(UIDList);
        }

        #endregion

        #region Private Method

        /// <summary>
        /// 获取当前正在直播的直播间信息
        /// </summary>
        /// <param name="UIDList">待获取的UID列表</param>
        /// <returns></returns>
        private static UidsInfo_Class? get_status_info_by_uids(List<long> UIDList)
        {
            if (UIDList == null || UIDList.Count == 0)
            {
                return null;
            }
            string LT = "{\"uids\":[" + string.Join(",", UIDList.Where(uid => uid != 0)) + "]}";
            string WebText = Post.PostBody($"{Config._LiveDomainName}/room/v1/Room/get_status_info_by_uids", null, true, LT);
            UidsInfo_Class? UserInfo_Class = JsonSerializer.Deserialize<UidsInfo_Class>(WebText);
            return UserInfo_Class;
        }

        private static void room_init(long RoomId)
        {
            string WebText = Get.GetBody($"{Config._LiveDomainName}/room/v1/Room/room_init?id=" + RoomId, true);
            （施工中）
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
                public long live_status { get; set; }
                public long short_id { get; set; }
                public long area { get; set; }
                public string area_name { get; set; }
                public long area_v2_id { get; set; }
                public string area_v2_name { get; set; }
                public string area_v2_parent_name { get; set; }
                public long area_v2_parent_id { get; set; }
                public string uname { get; set; }
                public string face { get; set; }
                public string tag_name { get; set; }
                public string tags { get; set; }
                public string cover_from_user { get; set; }
                public string keyframe { get; set; }
                public string lock_till { get; set; }
                public string hidden_till { get; set; }
                public long broadcast_type { get; set; }
                public Fans_Medal fans_medal { get; set; }
                public Official official { get; set; }
                public Vip vip { get; set; }
                public Pendant pendant { get; set; }
                public Nameplate nameplate { get; set; }
                public User_Honour_Info user_honour_info { get; set; }
                public bool is_followed { get; set; }
                public string top_photo { get; set; }
                public Theme theme { get; set; }
                public Sys_Notice sys_notice { get; set; }
                public Live_Room live_room { get; set; }
            }

            public class Fans_Medal
            {
                public bool show { get; set; }
                public bool wear { get; set; }
                public object medal { get; set; }
            }

            public class Official
            {
                public long role { get; set; }
                public string title { get; set; }
                public string desc { get; set; }
                public long type { get; set; }
            }

            public class Label
            {
                public string path { get; set; }
                public string text { get; set; }
                public string label_theme { get; set; }
                public string text_color { get; set; }
                public long bg_style { get; set; }
                public string bg_color { get; set; }
                public string border_color { get; set; }
                public bool use_img_label { get; set; }
                public string img_label_uri_hans { get; set; }
                public string img_label_uri_hant { get; set; }
                public string img_label_uri_hans_static { get; set; }
                public string img_label_uri_hant_static { get; set; }
            }

            public class Vip
            {
                public long type { get; set; }
                public long status { get; set; }
                public long due_date { get; set; }
                public long vip_pay_type { get; set; }
                public long theme_type { get; set; }
                public Label label { get; set; }
                public long avatar_subscript { get; set; }
                public string nickname_color { get; set; }
                public long role { get; set; }
                public string avatar_subscript_url { get; set; }
                public long tv_vip_status { get; set; }
                public long tv_vip_pay_type { get; set; }
                public long tv_due_date { get; set; }
            }

            public class Pendant
            {
                public long pid { get; set; }
                public string name { get; set; }
                public string image { get; set; }
                public long expire { get; set; }
                public string image_enhance { get; set; }
                public string image_enhance_frame { get; set; }
                public long n_pid { get; set; }
            }

            public class Nameplate
            {
                public long nid { get; set; }
                public string name { get; set; }
                public string image { get; set; }
                public string image_small { get; set; }
                public string level { get; set; }
                public string condition { get; set; }
            }

            public class User_Honour_Info
            {
                public long mid { get; set; }
                public object colour { get; set; }
                public List<object> tags { get; set; }
                public long is_latest_100honour { get; set; }
            }

            public class Theme
            {
            }

            public class Sys_Notice
            {
            }

            public class Watched_Show
            {
                public bool @switch { get; set; }
                public long num { get; set; }
                public string text_small { get; set; }
                public string text_large { get; set; }
                public string icon { get; set; }
                public string icon_location { get; set; }
                public string icon_web { get; set; }
            }

            public class Live_Room
            {
                public long roomStatus { get; set; }
                public long liveStatus { get; set; }
                public string url { get; set; }
                public string title { get; set; }
                public string cover { get; set; }
                public long roomid { get; set; }
                public long roundStatus { get; set; }
                public long broadcast_type { get; set; }
                public Watched_Show watched_show { get; set; }
            }
        }




        #endregion
    }
}
