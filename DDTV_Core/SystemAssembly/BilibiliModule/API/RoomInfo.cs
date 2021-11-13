using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API
{
    public class RoomInfo
    {
        /// <summary>
        /// 使用uids获取房间状态信息
        /// </summary>
        /// <param name="UIDList">需要更新的UID列表</param>
        /// <returns></returns>
        internal static void get_status_info_by_uids(List<long> UIDList)
        {
            string LT = "";
            LT = "{\"uids\":[" + UIDList[0];
            if (UIDList.Count > 1)
            {
                for (int i = 1 ; i < UIDList.Count ; i++)
                {
                    if (UIDList[i] != 0)
                    {
                        LT += "," + UIDList[i];
                    }
                }
            }
            LT += "]}";
            JObject JO = (JObject)JsonConvert.DeserializeObject(NetworkRequestModule.Post.Post.GetWebInfo_JsonClass("https://api.live.bilibili.com/room/v1/Room/get_status_info_by_uids", LT, "UTF-8"));
            if (JO!=null&&JO.ContainsKey("code")&&JO["code"]!=null&&(int)JO["code"]==0)
            {
                if (JO.TryGetValue("data", out var RoomList))
                {
                    IList<JToken> obj = JObject.Parse(RoomList.ToString());
                    if (RoomList.Count()>0)
                    {
                        for(int i = 0 ; i < RoomList.Count() ; i++)
                        {
                            long uid = JsonConvert.DeserializeObject<Rooms.RoomInfoClass.RoomInfo>(RoomList[((JProperty)obj[i]).Name].ToString()).Uid;
                            if (Rooms.Rooms.RoomInfo.TryGetValue(uid,out var roomInfo))
                            {
                                roomInfo=JsonConvert.DeserializeObject<Rooms.RoomInfoClass.RoomInfo>(RoomList[((JProperty)obj[i]).Name].ToString());
                            }
                            else
                            {
                                Rooms.Rooms.RoomInfo.Add(uid, JsonConvert.DeserializeObject<Rooms.RoomInfoClass.RoomInfo>(RoomList[((JProperty)obj[i]).Name].ToString()));
                            }
                        }
                    }
                }
            }
        }
    }
}
