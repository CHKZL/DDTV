using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.DataCacheModule
{
    public class DataCacheClass
    {
        /// <summary>
        /// 内建缓存表
        /// </summary>
        internal static Dictionary<int, Dictionary<string, Data>> Caches = new Dictionary<int, Dictionary<string, Data>>(new Dictionary<int, Dictionary<string, Data>> { })
            {
                { (int)DataCacheClass.CacheType.LongRoomId_BiliUserId, new Dictionary<string, Data>() },
                { (int)DataCacheClass.CacheType.LongRoomId_ShortRoomId, new Dictionary<string, Data>() }
            };
        /// <summary>
        /// 缓存表子对象
        /// </summary>
        internal class Data
        {
            /// <summary>
            /// 缓存表子对象的值
            /// </summary>
            internal string Value { set; get; }
            /// <summary>
            /// 缓存表子对象的有效期
            /// </summary>
            internal long ExTime { set; get; }
        }
        public enum CacheType
        {
            /// <summary>
            /// 使用UID获取_获取长房间号
            /// </summary>
            LongRoomId_BiliUserId = 1000,
            /// <summary>
            /// 短房间号_获取长房间号
            /// </summary>
            LongRoomId_ShortRoomId = 1001,
            /// <summary>
            /// 其他临时内容
            /// </summary>
            Other=int.MaxValue
        }
    };
}