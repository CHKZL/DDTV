using Server.WebAppServices.Middleware;
using Core.LogModule;
using Core.Network.Methods;
using Core.RuntimeObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Server.WebAppServices.MessageCode;
using static Server.WebAppServices.Middleware.InterfaceAuthentication;
using static Core.LogModule.Opcode;
using static Core.RuntimeObject.RoomCardClass;
using System.Drawing;

namespace Server.WebAppServices.Api
{
    /// <summary>
    /// 获取当前房间监控列表的统计
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/get_rooms/[controller]")]
    [Login]
    [Tags("get_rooms")]
    public class room_statistics : ControllerBase
    {
        /// <summary>
        /// 获取当前房间监控列表的统计
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <returns></returns>
        [HttpPost(Name = "room_statistics")]
        public ActionResult Post(PostCommonParameters commonParameters)
        {
            (int MonitoringCount, int LiveCount, int RecCount) count = Core.RuntimeObject._Room.Overview.GetRoomStatisticsOverview();
            return Content(MessageBase.MssagePack(nameof(room_information), count), "application/json");
        }
    }


    /// <summary>
    /// 查询单个房间信息
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/get_rooms/[controller]")]
    [Login]
    [Tags("get_rooms")]
    public class room_information : ControllerBase
    {
        /// <summary>
        /// 请求单个房间信息(UID或房间号二选一即可)
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="uid">UID</param>
        /// <param name="room_id">房间号</param>
        /// <returns></returns>
        [HttpPost(Name = "room_information")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] long uid = 0, [FromForm] long room_id= 0)
        {
            RoomCardClass roomCardClass = new RoomCardClass();
            if(uid!=0 && _Room.GetCardForUID(uid,ref roomCardClass))
            {
                return Content(MessageBase.MssagePack(nameof(room_information), roomCardClass), "application/json");
            }
            else if(room_id!=0 && _Room.GetCardFoRoomId(room_id,ref roomCardClass))
            {
                return Content(MessageBase.MssagePack(nameof(room_information), roomCardClass), "application/json");
            }
            else
            {
                return Content(MessageBase.MssagePack(nameof(room_information), false,"请求的房间不存在",code.OperationFailed), "application/json");
            }      
        }
    }

    /// <summary>
    /// 批量获取配置中房间完整信息
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/get_rooms/[controller]")]
    [Login]
    [Tags("get_rooms")]
    public class batch_complete_room_information : ControllerBase
    {
        /// <summary>
        /// 批量获取配置中房间完整信息
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="quantity">【非必填】分页后每页数量，默认或传0为全部</param>
        /// <param name="page">【非必填】获取的页数，当分页数量不为0时有效</param>
        /// <param name="type">【非必填】返回数据类型(0：全部  1：录制中  2：开播中  3：未开播   4：开但未录制   5: 全部以原始字母顺序排列返回)</param>
        /// <param name="screen_name">【非必填】搜索的昵称含有对应字符串的房间</param>
        /// <returns></returns>
        [HttpPost(Name = "batch_complete_room_information")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] int quantity = 0, [FromForm] int page = 0, [FromForm] _Room.SearchType type=_Room.SearchType.All, [FromForm] string screen_name = "")
        {
            Core.RuntimeObject._Room.Overview.CardData completeRoomInfoRes = Core.RuntimeObject._Room.Overview.GetCardOverview(quantity,page,type,screen_name);
            if(completeRoomInfoRes==null)
            {
                return Content(MessageBase.MssagePack(nameof(batch_complete_room_information), "", "请求错误", code.ParameterError), "application/json");
            }
            else
            {
                return Content(MessageBase.MssagePack(nameof(batch_complete_room_information), completeRoomInfoRes), "application/json");
            }
        }
    }
    /// <summary>
    /// 批量获取房间基本信息
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/get_rooms/[controller]")]
    [Login]
    [Tags("get_rooms")]
    public class batch_basic_room_information : ControllerBase
    {
        /// <summary>
        /// 批量获取配置中房间基本信息
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="quantity">【非必填】分页后每页数量，默认或传0为全部</param>
        /// <param name="page">【非必填】获取的页数，当分页数量不为0时有效</param>
        /// <param name="type">【非必填】返回数据类型(0：全部  1：录制中  2：开播中  3：未开播   4：开但未录制   5: 全部以原始字母顺序排列返回4：开但未录制   5: 全部以原始字母顺序排列返回)</param>
        /// <param name="screen_name">【非必填】搜索的昵称含有对应字符串的房间</param>
        /// <returns></returns>
        [HttpPost(Name = "batch_basic_room_information")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] int quantity = 0, [FromForm] int page = 0, [FromForm] _Room.SearchType type = _Room.SearchType.All, [FromForm] string screen_name = "")
        {
            try
            {
                Data basicRoomInfoRes = new Data();
                var roomList = _Room.GetCardListClone(type, screen_name);
                basicRoomInfoRes.total = roomList.Count;
                if (quantity == 0)
                {
                    foreach (var room in roomList)
                    {
                        Data.BasicInfo basicInfo = new Data.BasicInfo();
                        basicInfo.uid = room.Value.UID;
                        basicInfo.roomId = room.Value.RoomId;
                        basicInfo.userInfo = new()
                        {
                            name = room.Value.Name,
                            uid = room.Value.UID,
                            appointmentRecord=room.Value.AppointmentRecord,
                        };
                        basicInfo.roomInfo = new Data.BasicInfo.RoomInfo()
                        {
                            liveStatus = room.Value.live_status.Value == 1 ? true : false,
                            roomId = room.Value.RoomId,
                            title = room.Value.Title.Value,
                        };
                        basicInfo.taskStatus = new Data.BasicInfo.TaskStatus()
                        {
                            downloadSize = room.Value.DownInfo.DownloadSize,
                            isDownload = room.Value.DownInfo.IsDownload,
                            status = room.Value.DownInfo.Status,
                        };
                        basicRoomInfoRes.basicInfoList.Add(basicInfo);
                    }
                }
                else
                {
                    for (int i = page * quantity - quantity; i < roomList.Count && i < page * quantity; i++)
                    {
                        Data.BasicInfo basicInfo = new Data.BasicInfo();
                        basicInfo.userInfo = new()
                        {
                            name = roomList.ElementAt(i).Value.Name,
                            uid = roomList.ElementAt(i).Value.UID,
                            appointmentRecord=roomList.ElementAt(i).Value.AppointmentRecord
                        };
                        basicInfo.roomInfo = new Data.BasicInfo.RoomInfo()
                        {
                            liveStatus = roomList.ElementAt(i).Value.live_status.Value == 1 ? true : false,
                            roomId = roomList.ElementAt(i).Value.RoomId,
                            title = roomList.ElementAt(i).Value.Title.Value,
                            specialType = roomList.ElementAt(i).Value.special_type.Value,
                        };
                        basicInfo.taskStatus = new Data.BasicInfo.TaskStatus()
                        {
                            downloadSize = roomList.ElementAt(i).Value.DownInfo.DownloadSize,
                            isDownload = roomList.ElementAt(i).Value.DownInfo.IsDownload,
                            status = roomList.ElementAt(i).Value.DownInfo.Status,
                        };
                        basicRoomInfoRes.basicInfoList.Add(basicInfo);
                    }
                }
                roomList.Clear();
                roomList = null;
                return Content(MessageBase.MssagePack(nameof(batch_basic_room_information), basicRoomInfoRes), "application/json");
            }
            catch (Exception)
            {
                return Content(MessageBase.MssagePack(nameof(batch_basic_room_information), "", "请求错误", code.ParameterError), "application/json");
            }
        }
        public class Data
        {
            public int total { get; set; } = 0;
            public int currentPage { get; set; } = 1;
            public List<BasicInfo> basicInfoList { get; set; } = new();
            public class BasicInfo
            {
                public long uid { get; set; }
                public long roomId { get; set; }
                public UserInfo userInfo { get; set; } = new();
                public RoomInfo roomInfo { get; set; } = new();
                public TaskStatus taskStatus { get; set; } = new();
                public class UserInfo
                {
                    public string name { get; set; }
                    public long uid { get; set; }
                    public bool appointmentRecord {  get; set; }
                }
                public class RoomInfo
                {
                    public long roomId { get; set; }
                    public string title { get; set; }
                    public bool liveStatus { get; set; }
                    public int specialType { get; set; }

                }
                public class TaskStatus
                {
                    public bool isDownload { get; set; }
                    public long downloadSize { get; set; }
                    public DownloadStatus status { get; set; }
                }
            }
        }

    }
}
