using Core.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Network.Methods
{
    public class Nav
    {
        #region internal Method
        internal static Nav_Class? GetNav()
        {
            return _NAV();
        }
        #endregion

        #region Private Method
        /// <summary>
        /// 获取目前cookie所属账号状态和基本信息
        /// </summary>
        /// <returns></returns>
        private static Nav_Class? _NAV()
        {
            string WebText = Get.GetBody($"{Config.Core._MainDomainName}/x/web-interface/nav", true);
            Nav_Class? Nav_Class = JsonSerializer.Deserialize<Nav_Class>(WebText);
            return Nav_Class;
        }
        #endregion

        #region Public Class

        public class Nav_Class
        {
            public long code { get; set; }
            public string message { get; set; }
            public long ttl { get; set; }
            public Data data { get; set; }
        }

        public class Data
        {
            public bool isLogin { get; set; }
            public long email_verified { get; set; }
            public string face { get; set; }
            public long face_nft { get; set; }
            public long face_nft_type { get; set; }
            public Level_Info level_info { get; set; }
            public long mid { get; set; }
            public long mobile_verified { get; set; }
            public double money { get; set; }
            public long moral { get; set; }
            public Official official { get; set; }
            public OfficialVerify officialVerify { get; set; }
            public Pendant pendant { get; set; }
            public long scores { get; set; }
            public string uname { get; set; }
            public long vipDueDate { get; set; }
            public long vipStatus { get; set; }
            public long vipType { get; set; }
            public long vip_pay_type { get; set; }
            public long vip_theme_type { get; set; }
            public Vip_Label vip_label { get; set; }
            public long vip_avatar_subscript { get; set; }
            public string vip_nickname_color { get; set; }
            public Vip vip { get; set; }
            public Wallet wallet { get; set; }
            public bool has_shop { get; set; }
            public string shop_url { get; set; }
            public long allowance_count { get; set; }
            public long answer_status { get; set; }
            public long is_senior_member { get; set; }
            public Wbi_Img wbi_img { get; set; }
            public bool is_jury { get; set; }
        }

        public class Level_Info
        {
            public long current_level { get; set; }
            public long current_min { get; set; }
            public long current_exp { get; set; }
            public string next_exp { get; set; }
        }

        public class Official
        {
            public long role { get; set; }
            public string title { get; set; }
            public string desc { get; set; }
            public long type { get; set; }
        }

        public class OfficialVerify
        {
            public long type { get; set; }
            public string desc { get; set; }
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

        public class Vip_Label
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

        public class Wallet
        {
            public long mid { get; set; }
            public long bcoin_balance { get; set; }
            public long coupon_balance { get; set; }
            public long coupon_due_time { get; set; }
        }

        public class Wbi_Img
        {
            public string img_url { get; set; }
            public string sub_url { get; set; }
        }


        #endregion
    }
}
