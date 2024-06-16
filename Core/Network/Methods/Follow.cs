using Core.LogModule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Network.Methods
{
    public class Follow
    {

        #region Public Method

        #endregion

        #region internal Method
        /// <summary>
        /// 获取关注列表统计概览
        /// </summary>
        /// <returns></returns>
        internal static List<(string name, long tagid, int Count)> GetFollowGroups()
        {
            try
            {
                string web = Get.GetBody("https://api.bilibili.com/x/relation/tags", true);
                var jo = JsonConvert.DeserializeObject<FollowGroups>(web);
                List<(string name, long tagid, int Count)> list = new();
                if (jo != null && jo.data.Count > 0)
                {
                    int SumCount = 0;
                    foreach (var item in jo.data)
                    {
                        SumCount += item.count;
                        list.Add((item.name,item.tagid, item.count));
                    }
                    Log.Info(nameof(GetFollowGroups), $"获取关注分组统计概览成功，一共获取到{list.Count}个关注分组，一共{SumCount}人");
                    return list;
                }
                else
                {
                    Log.Info(nameof(GetFollowGroups), $"获取关注分组统计概览失败，可能是获取失败或者本身就没有关注用户");
                    return list;
                }

            }
            catch (Exception EX)
            {
                Log.Error(nameof(GetFollowGroups), $"获取关注分组统计概览错误", EX, true);
                List<(string name, long tagid, int Count)> list = new();
                return list;
            }
        }

        /// <summary>
        /// 获取某个关注分组中的关注详情
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="tagid">分组id</param>
        /// <param name="Page">第几页(最多一次500个)</param>
        internal static List<FollowLists.Data> GetFollowLists(string uid, long tagid, int Page)
        {
            List<FollowLists.Data> followLists = new();
            try
            {
                string web = Get.GetBody($"https://api.bilibili.com/x/relation/tag?mid={uid}&tagid={tagid}&pn={Page}&ps=500", true);
                var jo = JsonConvert.DeserializeObject<FollowLists>(web);
                if (jo != null && jo.data.Count > 0)
                {
                    followLists = jo.data;
                    Log.Info(nameof(GetFollowLists), $"获取(tagid:{tagid})分组中的关注列表成功，一共获{followLists.Count}人");
                    return followLists;
                }
                else
                {
                    Log.Info(nameof(GetFollowLists), $"获取分组中的关注详情失败，可能是获取失败或者本身就没有关注用户");
                    return followLists;
                }

            }
            catch (Exception EX)
            {
                Log.Error(nameof(GetFollowLists), $"获取(tagid:{tagid})分组中的详情失败", EX, true);
                followLists = new();
                return followLists;
            }
        }
        #endregion

        #region Public Class

        public class FollowLists
        {
            public int code { get; set; }
            public string message { get; set; }
            public int ttl { get; set; }
            public List<Data> data { get; set; } = new();
            public class Data
            {
                public long mid { get; set; }
                public string uname { get; set; }
                public string face { get; set; }
                public string sign { get; set; }
                public Live live { get; set; } = new();
                public string rec_reason { get; set; }
                public class Live
                {
                    public int live_status { get; set; }
                    public string jump_url { get; set; }
                }
            }
        }



        public class FollowGroups
        {
            public int code { get; set; }
            public string message { get; set; }
            public int ttl { get; set; }
            public List<Data> data { get; set; } = new();
            public class Data
            {
                /// <summary>
                /// 分组id
                /// </summary>
                public int tagid { get; set; }
                /// <summary>
                /// 关注分组名称
                /// </summary>
                public string name { get; set; }
                /// <summary>
                /// 数量
                /// </summary>
                public int count { get; set; }
                /// <summary>
                /// 提示
                /// </summary>
                public string tip { get; set; }
            }
        }



        #endregion

        #region Public Enmu

        #endregion

    }
}
