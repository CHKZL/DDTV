using Core.Account;
using Core.LogModule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Core.Network.Methods.User;

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
            const int maxAttempts = 3;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                string WebText = "";
                try
                {
                    WebText = Get.GetBody($"{Config.Core._MainDomainName}/x/web-interface/nav", true);
                    Nav_Class? Nav_Class = System.Text.Json.JsonSerializer.Deserialize<Nav_Class>(WebText);
                    RuntimeObject.Account.nav_info = Nav_Class.data;
                    return Nav_Class;
                }
                catch (Exception e)
                {
                    Log.Error(nameof(_NAV), $"获取Nva状态出现错误，重试。获取到的状态内容文本:{WebText}",e);
                    if (attempt == maxAttempts - 1)
                    {
                        return null;
                    }
                }
                Thread.Sleep(500);
            }
            return null;
        }

        #endregion

        #region Public Class

        public class Nav_Class
        {
            public long code { get; set; }
            public string message { get; set; }
            public long ttl { get; set; }
            public Data data { get; set; }

            public class Data
            {
                public bool isLogin { get; set; }
                [JsonIgnore]
                public long email_verified { get; set; }
                public string face { get; set; }
                [JsonIgnore]
                public long face_nft { get; set; }
                [JsonIgnore]
                public long face_nft_type { get; set; }
                [JsonIgnore]
                public Level_Info level_info { get; set; }
                public long mid { get; set; }
                [JsonIgnore]
                public long mobile_verified { get; set; }
                [JsonIgnore]
                public double money { get; set; }
                [JsonIgnore]
                public long moral { get; set; }
                [JsonIgnore]
                public Official official { get; set; }
                [JsonIgnore]
                public OfficialVerify officialVerify { get; set; }
                [JsonIgnore]
                public Pendant pendant { get; set; }
                [JsonIgnore]
                public long scores { get; set; }
                public string uname { get; set; }
                [JsonIgnore]
                public long vipDueDate { get; set; }
                [JsonIgnore]
                public long vipStatus { get; set; }
                [JsonIgnore]
                public long vipType { get; set; }
                [JsonIgnore]
                public long vip_pay_type { get; set; }
                [JsonIgnore]
                public long vip_theme_type { get; set; }
                [JsonIgnore]
                public Vip_Label vip_label { get; set; }
                [JsonIgnore]
                public long vip_avatar_subscript { get; set; }
                [JsonIgnore]
                public string vip_nickname_color { get; set; }
                [JsonIgnore]
                public Vip vip { get; set; }
                [JsonIgnore]
                public Wallet wallet { get; set; }
                [JsonIgnore]
                public bool has_shop { get; set; }
                [JsonIgnore]
                public string shop_url { get; set; }
                [JsonIgnore]
                public long allowance_count { get; set; }
                [JsonIgnore]
                public long answer_status { get; set; }
                [JsonIgnore]
                public long is_senior_member { get; set; }
                [JsonIgnore]
                public Wbi_Img wbi_img { get; set; }
                [JsonIgnore]
                public bool is_jury { get; set; }
            }

            public class Level_Info
            {
                public long current_level { get; set; }
                public long current_min { get; set; }
                public long current_exp { get; set; }
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
        }




        #endregion
    }
}
