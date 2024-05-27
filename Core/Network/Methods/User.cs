using Core.LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Core.Network.Methods.Nav;

namespace Core.Network.Methods
{
    public class User
    {
        #region Private Properties

        private static string imgKey = string.Empty;
        private static string subKey = string.Empty;
        private static long uid = 0;

        #endregion

        #region internal Method

        internal static UserInfo GetUserInfo(long Uid)
        {
            return _UserInfo(Uid);
        }

        #endregion

        #region Private Method

        private static UserInfo _UserInfo(long Uid)
        {
            if (string.IsNullOrEmpty(imgKey) || string.IsNullOrEmpty(subKey) || uid == 0)
            {
                var LoginStatus = GetNav();
                if (LoginStatus != null && LoginStatus.code == 0)
                {
                    string pattern = @"([a-z0-9]+)(?=\.png)";
                    imgKey = Regex.Match(LoginStatus.data.wbi_img.img_url, pattern).Value;
                    subKey = Regex.Match(LoginStatus.data.wbi_img.sub_url, pattern).Value;
                    uid = LoginStatus.data.mid;
                }
                else
                {
                    Log.Error(nameof(_UserInfo), "获取Nva_Key出现错误");
                    return null;
                }
            }
            long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            string salt = Get_salt(imgKey, subKey);
            string Query = Get_w_rid_string(Uid, timestamp, salt);
            string WebText = Get.GetBody($"{Config.Core_RunConfig._MainDomainName}/x/space/wbi/acc/info?{Query}", true);
            UserInfo UserInfo_Class = new();
            try
            {
                UserInfo_Class = JsonSerializer.Deserialize<UserInfo>(WebText);
                return UserInfo_Class;
            }
            catch (Exception)
            {
                return null;
            }

        }
        private static string Get_salt(string imgKey, string subKey)
        {
            var n = imgKey + subKey;
            var array = n.ToCharArray();
            var order = new int[] { 46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35, 27, 43, 5, 49,
                33, 9, 42, 19, 29, 28, 14, 39, 12,
                38, 41, 13, 37, 48, 7, 16,
                24,55 ,40 ,61 ,26 ,17 ,0 ,1 ,60 ,51 ,30 ,4 ,22 ,25 ,54 ,21 ,56 ,59 ,6 ,63 ,57 ,62 ,
                11 ,36 ,20 ,34 ,44 ,52 };
            var salt = new string(order.Select(i => array[i]).ToArray()).Substring(0, 32); // 按照特定顺序混淆并取前32位
            return salt;
        }
        private static string Get_w_rid_string(long uid, long timestamp, string salt)
        {
            string w_rid = GetMd5Hash("mid=" + uid + "&platform=web&token=&web_location=1550101&wts=" + timestamp + salt);
            return $"mid={uid}&token=&platform=web&web_location=1550101&w_rid={w_rid}&wts={timestamp}";
        }
        private static string GetMd5Hash(string input)
        {
            MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        #endregion

        #region Public Class
        public class UserInfo
        {
            public long code { get; set; }
            public string message { get; set; }
            public long ttl { get; set; }
            public Data data { get; set; }
            public class Data
            {
                public long mid { get; set; }
                public string name { get; set; }
                public string sex { get; set; }
                public string face { get; set; }
                public long face_nft { get; set; }
                public long face_nft_type { get; set; }
                public string sign { get; set; }
                public long rank { get; set; }
                public int level { get; set; }
                public long jointime { get; set; }
                public long moral { get; set; }
                public long silence { get; set; }
                public double coins { get; set; }
                public bool fans_badge { get; set; }
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
                public string birthday { get; set; }
                public School school { get; set; }
                public Profession profession { get; set; }
                public object tags { get; set; }
                public Series series { get; set; }
                public long is_senior_member { get; set; }
                public object mcn_info { get; set; }
                public long gaia_res_type { get; set; }
                public object gaia_data { get; set; }
                public bool is_risk { get; set; }
                public Elec elec { get; set; }
                public Contract contract { get; set; }
                public bool certificate_show { get; set; }
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
                public int roomStatus { get; set; }
                public int liveStatus { get; set; }
                public string url { get; set; }
                public string title { get; set; }
                public string cover { get; set; }
                public long roomid { get; set; }
                public long roundStatus { get; set; }
                public long broadcast_type { get; set; }
                public Watched_Show watched_show { get; set; }
            }

            public class School
            {
                public string name { get; set; }
            }

            public class Profession
            {
                public string name { get; set; }
                public string department { get; set; }
                public string title { get; set; }
                public long is_show { get; set; }
            }

            public class Series
            {
                public long user_upgrade_status { get; set; }
                public bool show_upgrade_window { get; set; }
            }

            public class Show_Info
            {
                public bool show { get; set; }
                public long state { get; set; }
                public string title { get; set; }
                public string icon { get; set; }
                public string jump_url { get; set; }
            }

            public class Elec
            {
                public Show_Info show_info { get; set; }
            }

            public class Contract
            {
                public bool is_display { get; set; }
                public bool is_follow_display { get; set; }
            }
        }



        #endregion
    }
}
