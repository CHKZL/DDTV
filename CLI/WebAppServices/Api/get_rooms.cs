using CLI.WebAppServices.Middleware;
using Core.LogModule;
using Core.Network.Methods;
using Core.RuntimeObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static CLI.WebAppServices.Middleware.InterfaceAuthentication;
using static Core.RuntimeObject.RoomList.RoomCard;

namespace CLI.WebAppServices.Api
{
    /// <summary>
    /// 获取房间完整信息
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/get_rooms/[controller]")]
    [Login]
    public class AllCompleteRoomInformation : ControllerBase
    {
        /// <summary>
        /// 获取所有配置中房间完整信息
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="quantity">分页后每页数量，非必填，默认或传0为全部</param>
        /// <param name="page">获取的页数，当分页数量不为0时有效</param>
        /// <returns></returns>
        [HttpPost(Name = "AllCompleteRoomInformation")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] int quantity = 0, [FromForm] int page = 0)
        {
            try
            {
                Data basicInfolist = new Data();
                var roomlist = _Room.GetCardListDeepClone();
                basicInfolist.Total = roomlist.Count;
                if (quantity == 0)
                {
                    foreach (var room in roomlist)
                    {
                        Data.BasicInfo basicInfo = new Data.BasicInfo();
                        basicInfo.userInfo = new()
                        {
                            IsAutoRec = room.Value.IsAutoRec,
                            Description = room.Value.Description,
                            IsRecDanmu = room.Value.IsRecDanmu,
                            IsRemind = room.Value.IsRemind,
                            Name = room.Value.Name,
                            Sex = room.Value.sex.Value,
                            Sign = room.Value.sign.Value,
                            UID = room.Value.UID
                        };
                        basicInfo.roomInfo = new Data.BasicInfo.RoomInfo()
                        {
                            AreaName = room.Value.area_v2_name.Value,
                            Attention = room.Value.attention.Value,
                            CoverFromUser = room.Value.cover_from_user.Value,
                            Face = room.Value.face.Value,
                            KeyFrame = room.Value.keyframe.Value,
                            LiveStatus = room.Value.live_status.Value == 1 ? true : false,
                            LiveTime = room.Value.live_time.Value,
                            RoomId = room.Value.RoomId,
                            ShortId = room.Value.short_id.Value,
                            tags = room.Value.tags.Value,
                            Title = room.Value.Title.Value,
                            Url = $"https://live.bilibili.com/{room.Value.RoomId}"
                        };
                        basicInfo.taskStatus = new Data.BasicInfo.TaskStatus()
                        {
                            DownloadSize = room.Value.DownInfo.DownloadSize,
                            EndTime = room.Value.DownInfo.EndTime,
                            IsDownload = room.Value.DownInfo.IsDownload,
                            StartTime = room.Value.DownInfo.StartTime,
                            Title = room.Value.Title.Value,
                            Status = room.Value.DownInfo.Status,
                        };
                        basicInfolist.basicInfolist.Add(basicInfo);
                    }
                }
                else
                {
                    for (int i = page * quantity - quantity; i < roomlist.Count && i < page * quantity + quantity; i++)
                    {
                        Data.BasicInfo basicInfo = new Data.BasicInfo();
                        basicInfo.userInfo = new()
                        {
                            IsAutoRec = roomlist.ElementAt(i).Value.IsAutoRec,
                            Description = roomlist.ElementAt(i).Value.Description,
                            IsRecDanmu = roomlist.ElementAt(i).Value.IsRecDanmu,
                            IsRemind = roomlist.ElementAt(i).Value.IsRemind,
                            Name = roomlist.ElementAt(i).Value.Name,
                            Sex = roomlist.ElementAt(i).Value.sex.Value,
                            Sign = roomlist.ElementAt(i).Value.sign.Value,
                            UID = roomlist.ElementAt(i).Value.UID
                        };
                        basicInfo.roomInfo = new Data.BasicInfo.RoomInfo()
                        {
                            AreaName = roomlist.ElementAt(i).Value.area_v2_name.Value,
                            Attention = roomlist.ElementAt(i).Value.attention.Value,
                            CoverFromUser = roomlist.ElementAt(i).Value.cover_from_user.Value,
                            Face = roomlist.ElementAt(i).Value.face.Value,
                            KeyFrame = roomlist.ElementAt(i).Value.keyframe.Value,
                            LiveStatus = roomlist.ElementAt(i).Value.live_status.Value == 1 ? true : false,
                            LiveTime = roomlist.ElementAt(i).Value.live_time.Value,
                            RoomId = roomlist.ElementAt(i).Value.RoomId,
                            ShortId = roomlist.ElementAt(i).Value.short_id.Value,
                            tags = roomlist.ElementAt(i).Value.tags.Value,
                            Title = roomlist.ElementAt(i).Value.Title.Value,
                            Url = $"https://live.bilibili.com/{roomlist.ElementAt(i).Value.RoomId}"
                        };
                        basicInfo.taskStatus = new Data.BasicInfo.TaskStatus()
                        {
                            DownloadSize = roomlist.ElementAt(i).Value.DownInfo.DownloadSize,
                            EndTime = roomlist.ElementAt(i).Value.DownInfo.EndTime,
                            IsDownload = roomlist.ElementAt(i).Value.DownInfo.IsDownload,
                            StartTime = roomlist.ElementAt(i).Value.DownInfo.StartTime,
                            Title = roomlist.ElementAt(i).Value.Title.Value,
                            Status = roomlist.ElementAt(i).Value.DownInfo.Status,
                        };
                        basicInfolist.basicInfolist.Add(basicInfo);
                    }
                }
                roomlist.Clear();
                roomlist = null;
                return Content(MessageBase.Success(nameof(AllBasicRoomInformation), basicInfolist), "application/json");
            }
            catch (Exception)
            {
                return Content(MessageBase.Success(nameof(AllBasicRoomInformation), "", "请求错误", MessageBase.code.ParameterError), "application/json");
            }
        }
        public class Data
        {
            public int Total { get; set; } = 0;
            public List<BasicInfo> basicInfolist { get; set; } = new();
            public class BasicInfo
            {
                public UserInfo userInfo { get; set; } = new();
                public RoomInfo roomInfo { get; set; } = new();
                public TaskStatus taskStatus { get; set; } = new();
                public class UserInfo
                {
                    public string Name { get; set; }
                    public string Description { get; set; }
                    public long UID { get; set; }
                    public bool IsAutoRec { get; set; }
                    public bool IsRemind { get; set; }
                    public bool IsRecDanmu { get; set; }
                    public string Sex { get; set; }
                    public string Sign { get; set; }
                }
                public class RoomInfo
                {
                    public long RoomId { get; set; }
                    public string Title { get; set; }
                    public int Attention { get; set; }
                    public long LiveTime { get; set; }
                    public bool LiveStatus { get; set; }
                    public int ShortId { get; set; }
                    public string AreaName { get; set; }
                    public string Face { get; set; }
                    public string tags { get; set; }
                    public string CoverFromUser { get; set; }
                    public string KeyFrame { get; set; }
                    public string Url { get; set; }

                }
                public class TaskStatus
                {
                    public bool IsDownload { get; set; }
                    public long DownloadSize { get; set; }
                    public DownloadStatus Status { get; set; }
                    public DateTime StartTime { get; set; }
                    public DateTime EndTime { get; set; }
                    public string Title { get; set; }
                }
            }
        }
    }
    /// <summary>
    /// 获取房间基本信息
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/get_rooms/[controller]")]
    [Login]
    public class AllBasicRoomInformation : ControllerBase
    {
        /// <summary>
        /// 获取所有配置中房间基本信息
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="quantity">分页后每页数量，非必填，默认或传0为全部</param>
        /// <param name="page">获取的页数，当分页数量不为0时有效</param>
        /// <returns></returns>
        [HttpPost(Name = "AllBasicRoomInformation")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] int quantity = 0, [FromForm] int page = 0)
        {
            try
            {
                Data basicInfolist = new Data();
                var roomlist = _Room.GetCardListDeepClone();
                basicInfolist.Total = roomlist.Count;
                if (quantity == 0)
                {
                    foreach (var room in roomlist)
                    {
                        Data.BasicInfo basicInfo = new Data.BasicInfo();
                        basicInfo.userInfo = new()
                        {
                            Name = room.Value.Name,
                            UID = room.Value.UID
                        };
                        basicInfo.roomInfo = new Data.BasicInfo.RoomInfo()
                        {
                            LiveStatus = room.Value.live_status.Value == 1 ? true : false,
                            RoomId = room.Value.RoomId,
                            Title = room.Value.Title.Value,
                        };
                        basicInfo.taskStatus = new Data.BasicInfo.TaskStatus()
                        {
                            DownloadSize = room.Value.DownInfo.DownloadSize,
                            IsDownload = room.Value.DownInfo.IsDownload,
                            Status = room.Value.DownInfo.Status,
                        };
                        basicInfolist.basicInfolist.Add(basicInfo);
                    }
                }
                else
                {
                    for (int i = page * quantity - quantity; i < roomlist.Count && i < page * quantity + quantity; i++)
                    {
                        Data.BasicInfo basicInfo = new Data.BasicInfo();
                        basicInfo.userInfo = new()
                        {
                            Name = roomlist.ElementAt(i).Value.Name,
                            UID = roomlist.ElementAt(i).Value.UID
                        };
                        basicInfo.roomInfo = new Data.BasicInfo.RoomInfo()
                        {
                            LiveStatus = roomlist.ElementAt(i).Value.live_status.Value == 1 ? true : false,
                            RoomId = roomlist.ElementAt(i).Value.RoomId,
                            Title = roomlist.ElementAt(i).Value.Title.Value,
                        };
                        basicInfo.taskStatus = new Data.BasicInfo.TaskStatus()
                        {
                            DownloadSize = roomlist.ElementAt(i).Value.DownInfo.DownloadSize,
                            IsDownload = roomlist.ElementAt(i).Value.DownInfo.IsDownload,               
                            Status = roomlist.ElementAt(i).Value.DownInfo.Status,
                        };
                        basicInfolist.basicInfolist.Add(basicInfo);
                    }
                }
                roomlist.Clear();
                roomlist = null;
                return Content(MessageBase.Success(nameof(AllBasicRoomInformation), basicInfolist), "application/json");
            }
            catch (Exception)
            {
                return Content(MessageBase.Success(nameof(AllBasicRoomInformation), "", "请求错误", MessageBase.code.ParameterError), "application/json");
            }
        }
        public class Data
        {
            public int Total { get; set; } = 0;
            public int CurrentPage { get; set; } = 1;
            public List<BasicInfo> basicInfolist { get; set; } = new();
            public class BasicInfo
            {
                public UserInfo userInfo { get; set; } = new();
                public RoomInfo roomInfo { get; set; } = new();
                public TaskStatus taskStatus { get; set; } = new();
                public class UserInfo
                {
                    public string Name { get; set; }
                    public long UID { get; set; }
                }
                public class RoomInfo
                {
                    public long RoomId { get; set; }
                    public string Title { get; set; }
                    public bool LiveStatus { get; set; }

                }
                public class TaskStatus
                {
                    public bool IsDownload { get; set; }
                    public long DownloadSize { get; set; }
                    public DownloadStatus Status { get; set; }
                }
            }
        }

    }
}
