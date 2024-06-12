using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Network.Methods.Follow;

namespace Core.RuntimeObject
{
    public class Follow
    {
        /// <summary>
        /// 获取关注列表统计概览
        /// </summary>
        /// <returns></returns>
        public static List<(string name, long tagid, int Count)> GetFollowGroups()
        {
            return Network.Methods.Follow.GetFollowGroups();
        }

        /// <summary>
        /// 获取某个关注分组中的关注详情
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="tagid">分组id</param>
        /// <param name="page">第几页(最多一次500个)</param>
        /// <param name="datas">返回的详情列表</param>
        /// <returns>一共成功几个</returns>
        public static int GetFollowLists(string uid,long tagid,int page,out List<FollowLists.Data> datas)
        {
            datas = Network.Methods.Follow.GetFollowLists(uid,tagid,page);
            return datas.Count;
        }
    }
}
