﻿using CLI.WebAppServices.Middleware;
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
    public class all_complete_room_information : ControllerBase
    {
        /// <summary>
        /// 获取所有配置中房间完整信息
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="quantity">分页后每页数量，非必填，默认或传0为全部</param>
        /// <param name="page">获取的页数，当分页数量不为0时有效</param>
        /// <returns></returns>
        [HttpPost(Name = "all_complete_room_information")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] int quantity = 0, [FromForm] int page = 0)
        {
            try
            {
                Data completeRoomInfoRes = new Data();
                var roomList = _Room.GetCardListDeepClone();
                completeRoomInfoRes.total = roomList.Count;
                if (quantity == 0)
                {
                    foreach (var room in roomList)
                    {
                        Data.CompleteInfo completeInfo = new Data.CompleteInfo();
                        completeInfo.userInfo = new()
                        {
                            isAutoRec = room.Value.IsAutoRec,
                            description = room.Value.Description,
                            isRecDanmu = room.Value.IsRecDanmu,
                            isRemind = room.Value.IsRemind,
                            name = room.Value.Name,
                            sex = room.Value.sex.Value,
                            sign = room.Value.sign.Value,
                            uid = room.Value.UID
                        };
                        completeInfo.roomInfo = new Data.CompleteInfo.RoomInfo()
                        {
                            areaName = room.Value.area_v2_name.Value,
                            attention = room.Value.attention.Value,
                            coverFromUser = room.Value.cover_from_user.Value,
                            face = room.Value.face.Value,
                            keyFrame = room.Value.keyframe.Value,
                            liveStatus = room.Value.live_status.Value == 1 ? true : false,
                            liveTime = room.Value.live_time.Value,
                            roomId = room.Value.RoomId,
                            shortId = room.Value.short_id.Value,
                            tags = room.Value.tags.Value,
                            title = room.Value.Title.Value,
                            url = $"https://live.bilibili.com/{room.Value.RoomId}"
                        };
                        completeInfo.taskStatus = new Data.CompleteInfo.TaskStatus()
                        {
                            downloadSize = room.Value.DownInfo.DownloadSize,
                            endTime = room.Value.DownInfo.EndTime,
                            isDownload = room.Value.DownInfo.IsDownload,
                            startTime = room.Value.DownInfo.StartTime,
                            title = room.Value.Title.Value,
                            status = room.Value.DownInfo.Status,
                        };
                        completeRoomInfoRes.completeInfoList.Add(completeInfo);
                    }
                }
                else
                {
                    for (int i = page * quantity - quantity; i < roomList.Count && i < page * quantity + quantity; i++)
                    {
                        Data.CompleteInfo completeInfo = new Data.CompleteInfo();
                        completeInfo.userInfo = new()
                        {
                            isAutoRec = roomList.ElementAt(i).Value.IsAutoRec,
                            description = roomList.ElementAt(i).Value.Description,
                            isRecDanmu = roomList.ElementAt(i).Value.IsRecDanmu,
                            isRemind = roomList.ElementAt(i).Value.IsRemind,
                            name = roomList.ElementAt(i).Value.Name,
                            sex = roomList.ElementAt(i).Value.sex.Value,
                            sign = roomList.ElementAt(i).Value.sign.Value,
                            uid = roomList.ElementAt(i).Value.UID
                        };
                        completeInfo.roomInfo = new Data.CompleteInfo.RoomInfo()
                        {
                            areaName = roomList.ElementAt(i).Value.area_v2_name.Value,
                            attention = roomList.ElementAt(i).Value.attention.Value,
                            coverFromUser = roomList.ElementAt(i).Value.cover_from_user.Value,
                            face = roomList.ElementAt(i).Value.face.Value,
                            keyFrame = roomList.ElementAt(i).Value.keyframe.Value,
                            liveStatus = roomList.ElementAt(i).Value.live_status.Value == 1 ? true : false,
                            liveTime = roomList.ElementAt(i).Value.live_time.Value,
                            roomId = roomList.ElementAt(i).Value.RoomId,
                            shortId = roomList.ElementAt(i).Value.short_id.Value,
                            tags = roomList.ElementAt(i).Value.tags.Value,
                            title = roomList.ElementAt(i).Value.Title.Value,
                            url = $"https://live.bilibili.com/{roomList.ElementAt(i).Value.RoomId}"
                        };
                        completeInfo.taskStatus = new Data.CompleteInfo.TaskStatus()
                        {
                            downloadSize = roomList.ElementAt(i).Value.DownInfo.DownloadSize,
                            endTime = roomList.ElementAt(i).Value.DownInfo.EndTime,
                            isDownload = roomList.ElementAt(i).Value.DownInfo.IsDownload,
                            startTime = roomList.ElementAt(i).Value.DownInfo.StartTime,
                            title = roomList.ElementAt(i).Value.Title.Value,
                            status = roomList.ElementAt(i).Value.DownInfo.Status,
                        };
                        completeRoomInfoRes.completeInfoList.Add(completeInfo);
                    }
                }
                roomList.Clear();
                roomList = null;
                return Content(MessageBase.Success(nameof(all_complete_room_information), completeRoomInfoRes), "application/json");
            }
            catch (Exception)
            {
                return Content(MessageBase.Success(nameof(all_complete_room_information), "", "请求错误", MessageBase.code.ParameterError), "application/json");
            }
        }
        public class Data
        {
            public int total { get; set; } = 0;
            public List<CompleteInfo> completeInfoList { get; set; } = new();
            public class CompleteInfo
            {
                public UserInfo userInfo { get; set; } = new();
                public RoomInfo roomInfo { get; set; } = new();
                public TaskStatus taskStatus { get; set; } = new();
                public class UserInfo
                {
                    public string name { get; set; }
                    public string description { get; set; }
                    public long uid { get; set; }
                    public bool isAutoRec { get; set; }
                    public bool isRemind { get; set; }
                    public bool isRecDanmu { get; set; }
                    public string sex { get; set; }
                    public string sign { get; set; }
                }
                public class RoomInfo
                {
                    public long roomId { get; set; }
                    public string title { get; set; }
                    public int attention { get; set; }
                    public long liveTime { get; set; }
                    public bool liveStatus { get; set; }
                    public int shortId { get; set; }
                    public string areaName { get; set; }
                    public string face { get; set; }
                    public string tags { get; set; }
                    public string coverFromUser { get; set; }
                    public string keyFrame { get; set; }
                    public string url { get; set; }

                }
                public class TaskStatus
                {
                    public bool isDownload { get; set; }
                    public long downloadSize { get; set; }
                    public DownloadStatus status { get; set; }
                    public DateTime startTime { get; set; }
                    public DateTime endTime { get; set; }
                    public string title { get; set; }
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
    public class all_basic_room_information : ControllerBase
    {
        /// <summary>
        /// 获取所有配置中房间基本信息
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="quantity">分页后每页数量，非必填，默认或传0为全部</param>
        /// <param name="page">获取的页数，当分页数量不为0时有效</param>
        /// <returns></returns>
        [HttpPost(Name = "all_basic_room_information")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] int quantity = 0, [FromForm] int page = 0)
        {
            try
            {
                Data basicRoomInfoRes = new Data();
                var roomList = _Room.GetCardListDeepClone();
                basicRoomInfoRes.total = roomList.Count;
                if (quantity == 0)
                {
                    foreach (var room in roomList)
                    {
                        Data.BasicInfo basicInfo = new Data.BasicInfo();
                        basicInfo.userInfo = new()
                        {
                            name = room.Value.Name,
                            uid = room.Value.UID
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
                    for (int i = page * quantity - quantity; i < roomList.Count && i < page * quantity + quantity; i++)
                    {
                        Data.BasicInfo basicInfo = new Data.BasicInfo();
                        basicInfo.userInfo = new()
                        {
                            name = roomList.ElementAt(i).Value.Name,
                            uid = roomList.ElementAt(i).Value.UID
                        };
                        basicInfo.roomInfo = new Data.BasicInfo.RoomInfo()
                        {
                            liveStatus = roomList.ElementAt(i).Value.live_status.Value == 1 ? true : false,
                            roomId = roomList.ElementAt(i).Value.RoomId,
                            title = roomList.ElementAt(i).Value.Title.Value,
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
                return Content(MessageBase.Success(nameof(all_basic_room_information), basicRoomInfoRes), "application/json");
            }
            catch (Exception)
            {
                return Content(MessageBase.Success(nameof(all_basic_room_information), "", "请求错误", MessageBase.code.ParameterError), "application/json");
            }
        }
        public class Data
        {
            public int total { get; set; } = 0;
            public int currentPage { get; set; } = 1;
            public List<BasicInfo> basicInfoList { get; set; } = new();
            public class BasicInfo
            {
                public UserInfo userInfo { get; set; } = new();
                public RoomInfo roomInfo { get; set; } = new();
                public TaskStatus taskStatus { get; set; } = new();
                public class UserInfo
                {
                    public string name { get; set; }
                    public long uid { get; set; }
                }
                public class RoomInfo
                {
                    public long roomId { get; set; }
                    public string title { get; set; }
                    public bool liveStatus { get; set; }

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