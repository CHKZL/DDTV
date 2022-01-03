using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.DataCacheModule;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DDTV_Core.SystemAssembly.ConfigModule.RoomConfigClass;
using static DDTV_Core.SystemAssembly.DataCacheModule.DataCacheClass;

namespace DDTV_Core.SystemAssembly.ConfigModule
{
    internal class RoomConfigFile
    {
        
        /// <summary>
        /// 读取房间配置文件
        /// </summary>
        internal static void ReadRoomConfigFile()
        {
            var rlc = new RoomListDiscard();
            try
            {
                rlc = JsonConvert.DeserializeObject<RoomListDiscard>(File.ReadAllText(RoomConfig.RoomFile));
            }
            catch (Exception)
            {
                rlc = JsonConvert.DeserializeObject<RoomListDiscard>("{}");
                Log.Log.AddLog(nameof(RoomConfigFile),Log.LogClass.LogType.Error, "房间json配置文件格式错误！请检测核对！");
            }
            List<RoomCardDiscard> RoomConfigList = rlc?.data;
            //RoomConfigList = rlc?.data;
            //如果房间配置文件错误或者为空，默认生成一个新的List<RoomCard>对象
            if (RoomConfigList == null)
            {
                RoomConfigList = new List<RoomCardDiscard>();
            }
            foreach (var item in RoomConfigList)
            {
                if (Rooms.RoomInfo.TryGetValue(item.UID, out var roomInfo))
                {
                    Rooms.RoomInfo[item.UID].uname =string.IsNullOrEmpty(item.name)?item.Name: item.name;
                    Rooms.RoomInfo[item.UID].Description=item.Description;
                    Rooms.RoomInfo[item.UID].room_id=item.RoomId==0?int.Parse(item.RoomNumber) :item.RoomId;
                    Rooms.RoomInfo[item.UID].IsAutoRec = (item.IsAutoRec||item.VideoStatus);
                    Rooms.RoomInfo[item.UID].IsRemind = (item.IsRemind||item.RemindStatus);
                    Rooms.RoomInfo[item.UID].Like = item.Like;
                    Rooms.RoomInfo[item.UID].IsRecDanmu = item.IsRecDanmu;
                }
                else
                {
                    Rooms.RoomInfo.Add(item.UID,new RoomInfoClass.RoomInfo() 
                    { 
                        uname = string.IsNullOrEmpty(item.name) ? item.Name : item.name,
                        Description = item.Description,
                        room_id = item.RoomId==0 ? int.Parse(item.RoomNumber) : item.RoomId,
                        IsAutoRec = (item.IsAutoRec||item.VideoStatus),
                        IsRemind =(item.IsRemind||item.RemindStatus),
                        Like = item.Like,
                        uid = item.UID,
                        IsRecDanmu=item.IsRecDanmu,
                    });
                }
                DataCache.SetCache(CacheType.uname, item.UID.ToString(), Rooms.RoomInfo[item.UID].uname.ToString(), int.MaxValue);
                DataCache.SetCache(CacheType.Description, item.UID.ToString(), Rooms.RoomInfo[item.UID].Description!=null?Rooms.RoomInfo[item.UID].Description.ToString():"", int.MaxValue);
                DataCache.SetCache(CacheType.room_id, item.UID.ToString(), Rooms.RoomInfo[item.UID].room_id.ToString(), int.MaxValue);
                DataCache.SetCache(CacheType.IsAutoRec, item.UID.ToString(), Rooms.RoomInfo[item.UID].IsAutoRec.ToString(), int.MaxValue);
                DataCache.SetCache(CacheType.IsRemind, item.UID.ToString(), Rooms.RoomInfo[item.UID].IsRemind.ToString(), int.MaxValue);
                DataCache.SetCache(CacheType.Like, item.UID.ToString(), Rooms.RoomInfo[item.UID].Like.ToString(), int.MaxValue);
                DataCache.SetCache(CacheType.uid, item.UID.ToString(), Rooms.RoomInfo[item.UID].uid.ToString(), int.MaxValue);
                DataCache.SetCache(CacheType.IsRecDanmu, item.UID.ToString(), Rooms.RoomInfo[item.UID].IsRecDanmu.ToString(), int.MaxValue);
            }
            Log.Log.AddLog(nameof(RoomConfigFile), Log.LogClass.LogType.Debug, $"读取房间配置文件完成，一共读取到[{RoomConfigList.Count}]个房间配置");
            WriteRoomConfigFile();
        }
        /// <summary>
        /// 保存房间配置文件
        /// </summary>
        internal static void WriteRoomConfigFile()
        {
            RoomWrite roomCards = new() { data=new List<RoomCard>() };
            foreach (var item in Rooms.RoomInfo)
            {
                roomCards.data.Add(new RoomCard
                { 
                    IsAutoRec=item.Value.IsAutoRec,
                    Description=item.Value.Description,
                    IsRemind=item.Value.IsRemind,
                    Like=item.Value.Like,
                    name=item.Value.uname,
                    RoomId=item.Value.room_id,
                    UID=item.Value.uid,
                    IsRecDanmu=item.Value.IsRecDanmu
                });
                DataCache.SetCache(CacheType.uname, item.Value.uid.ToString(), item.Value.uname.ToString(), int.MaxValue);
                DataCache.SetCache(CacheType.Description, item.Value.uid.ToString(), item.Value.Description != null ? item.Value.Description.ToString() : "", int.MaxValue);
                DataCache.SetCache(CacheType.room_id, item.Value.uid.ToString(), item.Value.room_id.ToString(), int.MaxValue);
                DataCache.SetCache(CacheType.IsAutoRec, item.Value.uid.ToString(), item.Value.IsAutoRec.ToString(), int.MaxValue);
                DataCache.SetCache(CacheType.IsRemind, item.Value.uid.ToString(), item.Value.IsRemind.ToString(), int.MaxValue);
                DataCache.SetCache(CacheType.Like, item.Value.uid.ToString(), item.Value.Like.ToString(), int.MaxValue);
                DataCache.SetCache(CacheType.uid, item.Value.uid.ToString(), item.Value.uid.ToString(), int.MaxValue);
                DataCache.SetCache(CacheType.IsRecDanmu, item.Value.uid.ToString(), item.Value.IsRecDanmu.ToString(), int.MaxValue);
            }
            File.WriteAllText(RoomConfig.RoomFile, JsonConvert.SerializeObject(roomCards));
            Log.Log.AddLog(nameof(RoomConfigFile), Log.LogClass.LogType.Debug, $"更新写入房间配置文件完成,当前房间配置文件有[{roomCards.data.Count}]个房间配置");
        }
        public class RoomWrite
        {
            public List<RoomCard> data { set; get; }
        }
    }
}
