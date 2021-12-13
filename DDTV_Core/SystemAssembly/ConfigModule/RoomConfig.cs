using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DDTV_Core.SystemAssembly.ConfigModule.RoomConfigClass;

namespace DDTV_Core.SystemAssembly.ConfigModule
{

    public class RoomConfig
    {
        public static string RoomFile = CoreConfig.GetValue(CoreConfigClass.Key.RoomListConfig, "./RoomListConfig.json", CoreConfigClass.Group.Core);
        /// <summary>
        /// 添加房间记录
        /// </summary>
        /// <param name="uid">添加房间的用户uid(mid)</param>
        /// <param name="Description">描述或备注</param>
        /// <param name="IsAutoRec">是否自动录制</param>
        /// <param name="IsRemind">是否开播提醒</param>
        /// <returns></returns>
        public static string AddRoom(long uid, string Description, bool IsAutoRec = false, bool IsRemind = false, bool IsRecDanmu = false)
        {
            if (Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
            {
                Log.Log.AddLog(nameof(Rooms), Log.LogClass.LogType.Warn, "添加的房间已存在！");
                return "添加的房间已存在";
            }
            else
            {
                if (Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.roomStatus) == "1")
                {
                    int.TryParse(Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.room_id), out int RoomId);
                    if (Rooms.RoomInfo.ContainsKey(uid))
                    {
                        Rooms.RoomInfo[uid].IsAutoRec = IsAutoRec;
                        Rooms.RoomInfo[uid].IsRemind = IsRemind;
                        Rooms.RoomInfo[uid].Description = Description;
                        Rooms.RoomInfo[uid].Like = false;
                        Rooms.RoomInfo[uid].uname = Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname);
                        Rooms.RoomInfo[uid].room_id = RoomId;
                        Rooms.RoomInfo[uid].uid = uid;
                        Rooms.RoomInfo[uid].IsRecDanmu = IsRecDanmu;
                    }
                    else
                    {
                        Rooms.RoomInfo.Add(uid, new RoomInfoClass.RoomInfo()
                        {
                            IsAutoRec = IsAutoRec,
                            IsRemind = IsRemind,
                            Description = Description,
                            Like = false,
                            uname = Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname),
                            room_id = RoomId,
                            uid = uid,
                            IsRecDanmu = IsRecDanmu,
                        });
                    }

                    RoomConfigFile.WriteRoomConfigFile();
                    Log.Log.AddLog(nameof(Rooms), Log.LogClass.LogType.Info, $"添加房间记录成功:【[uid]:{uid} [room_id]:{RoomId} [uname]:{Rooms.GetValue(uid, DataCacheModule.DataCacheClass.CacheType.uname)} [IsAutoRec]:{IsAutoRec} [IsRemind]:{IsRemind}】");
                    return "添加房间记录成功";
                }
                else
                {
                    Log.Log.AddLog(nameof(Rooms), Log.LogClass.LogType.Warn, "填写的用户UID房间不存在");
                    return "填写的用户UID房间不存在";
                }
            }
        }
        /// <summary>
        /// 自动添加房间记录(所有数据手动填入)
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="RoomId"></param>
        /// <param name="Name"></param>
        public static void AddRoom(long uid, int RoomId, string Name,bool IsSave=false)
        {
            if (!Rooms.RoomInfo.TryGetValue(uid, out RoomInfoClass.RoomInfo roomInfo))
            {
                Rooms.RoomInfo.Add(uid, new RoomInfoClass.RoomInfo()
                {
                    IsAutoRec = false,
                    IsRemind = false,
                    Description = "",
                    Like = false,
                    uname = Name,
                    room_id = RoomId,
                    uid = uid,
                    IsRecDanmu = false,
                });
            }
            if (IsSave)
            {
                RoomConfigFile.WriteRoomConfigFile();
            }
        }

        /// <summary>
        /// 批量修改房间配置内容
        /// </summary>
        /// <param name="roomCards">需要修改的房间信息list</param>
        /// <param name="IsALL">是否修改全部</param>
        /// <param name="Type">选择修改的内容(1:房间号 2:自动录制 3:开播提醒 4:名称 5:特别关注)</param>
        public static void ReviseRoom(List<RoomConfigClass.RoomCard> roomCards, bool IsALL, int Type)
        {
            foreach (var item in roomCards)
            {
                if (item.UID > 0)
                {
                    if (Rooms.RoomInfo.TryGetValue(item.UID, out RoomInfoClass.RoomInfo roomInfo))
                    {
                        if (IsALL)
                        {
                            roomInfo.room_id = item.RoomId;
                            roomInfo.IsAutoRec = item.IsAutoRec;
                            roomInfo.IsRemind = item.IsRemind;
                            roomInfo.uname = item.name;
                            roomInfo.Like = item.Like;
                        }
                        else
                        {
                            roomInfo.room_id = roomInfo.room_id;
                            roomInfo.IsAutoRec = roomInfo.IsAutoRec;
                            roomInfo.IsRemind = roomInfo.IsRemind;
                            roomInfo.uname = roomInfo.uname;
                            roomInfo.Like = roomInfo.Like;
                            switch (Type)
                            {
                                case 1:
                                    roomInfo.room_id = item.RoomId;
                                    break;
                                case 2:
                                    roomInfo.IsAutoRec = item.IsAutoRec;
                                    break;
                                case 3:
                                    roomInfo.IsRemind = item.IsRemind;
                                    break;
                                case 4:
                                    roomInfo.uname = item.name;
                                    break;
                                case 5:
                                    roomInfo.Like = item.Like;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Log.Log.AddLog(nameof(RoomConfig), Log.LogClass.LogType.Warn, $"未在当前房间配置内容中找到想修改的房间(UID:{item.UID})信息，修改失败");

                    }
                }
                else
                {
                    Log.Log.AddLog(nameof(RoomConfig), Log.LogClass.LogType.Warn, $"提交的uid信息(UID:{item.UID})不正确，修改失败");
                }
            }
            RoomConfigFile.WriteRoomConfigFile();
        }
        /// <summary>
        /// 修改单个房间配置内容
        /// </summary>
        /// <param name="roomCards">需要修改的房间信息list</param>
        /// <param name="IsALL">是否修改全部</param>
        /// <param name="Type">选择修改的内容(1:房间号 2:自动录制 3:开播提醒 4:名称 5:特别关注)</param>
        public static bool ReviseRoom(RoomCard roomCards, bool IsALL, int Type)
        {

            try
            {
                if (roomCards.UID > 0)
                {
                    if (Rooms.RoomInfo.TryGetValue(roomCards.UID, out RoomInfoClass.RoomInfo roomInfo))
                    {
                        if (IsALL)
                        {
                            roomInfo.room_id = roomCards.RoomId;
                            roomInfo.IsAutoRec = roomCards.IsAutoRec;
                            roomInfo.IsRemind = roomCards.IsRemind;
                            roomInfo.uname = roomCards.name;
                            roomInfo.Like = roomCards.Like;
                        }
                        else
                        {
                            roomInfo.room_id = roomInfo.room_id;
                            roomInfo.IsAutoRec = roomInfo.IsAutoRec;
                            roomInfo.IsRemind = roomInfo.IsRemind;
                            roomInfo.uname = roomInfo.uname;
                            roomInfo.Like = roomInfo.Like;
                            switch (Type)
                            {
                                case 1:
                                    roomInfo.room_id = roomCards.RoomId;
                                    break;
                                case 2:
                                    roomInfo.IsAutoRec = roomCards.IsAutoRec;
                                    break;
                                case 3:
                                    roomInfo.IsRemind = roomCards.IsRemind;
                                    break;
                                case 4:
                                    roomInfo.uname = roomCards.name;
                                    break;
                                case 5:
                                    roomInfo.Like = roomCards.Like;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Log.Log.AddLog(nameof(RoomConfig), Log.LogClass.LogType.Warn, $"未在当前房间配置内容中找到想修改的房间(UID:{roomCards.UID})信息，修改失败");
                        return false;
                    }
                }
                else
                {
                    Log.Log.AddLog(nameof(RoomConfig), Log.LogClass.LogType.Warn, $"提交的uid信息(UID:{roomCards.UID})不正确，修改失败");
                    return false;
                }

                RoomConfigFile.WriteRoomConfigFile();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 批量删除房间
        /// </summary>
        /// <param name="uidLis">要删除的房间列表</param>
        public static void DeleteRoom(List<long> uidLis)
        {
            if (uidLis.Count > 0)
            {
                foreach (var item in uidLis)
                {
                    if (Rooms.RoomInfo.TryGetValue(item, out RoomInfoClass.RoomInfo roomInfo))
                    {
                        Rooms.RoomInfo.Remove(item);
                    }
                    else
                    {
                        Log.Log.AddLog(nameof(RoomConfig), Log.LogClass.LogType.Warn, $"想要删除的房间不存在于配置中(UID:{item})，删除失败");
                    }
                }
                RoomConfigFile.WriteRoomConfigFile();
            }
        }
        /// <summary>
        /// 删除房间
        /// </summary>
        /// <param name="uidLis"></param>
        public static bool DeleteRoom(long uidLis)
        {
            bool IsOK = false;
            if (Rooms.RoomInfo.TryGetValue(uidLis, out RoomInfoClass.RoomInfo roomInfo))
            {
                Rooms.RoomInfo.Remove(uidLis);
                IsOK = true;
            }
            else
            {
                Log.Log.AddLog(nameof(RoomConfig), Log.LogClass.LogType.Warn, $"想要删除的房间不存在于配置中(UID:{uidLis})，删除失败");
                IsOK = false;
            }
            RoomConfigFile.WriteRoomConfigFile();
            return IsOK;
        }
    }
}
