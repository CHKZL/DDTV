using Server.WebAppServices.Middleware;
using Core.LogModule;
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
using static Server.WebAppServices.Middleware.InterfaceAuthentication;

namespace Server.WebAppServices.Api
{
    /// <summary>
    /// 修改自动录制设置
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/set_rooms/[controller]")]
    [Login]
    [Tags("set_rooms")]
    public class modify_recording_settings : ControllerBase
    {
        /// <summary>
        /// 批量修改房间的录制设置
        /// </summary>
        /// <param name="uid">要修改录制状态的房间UID列表</param>
        /// <param name="state">将房间的录制状态设置为什么状态</param>
        /// <param name="commonParameters"></param>
        /// <returns></returns>
        [HttpPost(Name = "modify_recording_settings")]
        public ActionResult Post([FromForm] List<long> uid, [FromForm] bool state, PostCommonParameters commonParameters)
        {
            List<long> count = Core.RuntimeObject._Room.ModifyRecordingSettings(uid, state);
            return Content(MessageBase.MssagePack(nameof(modify_recording_settings), count, $"返回列表中的房间的自动录制修改为{state}"), "application/json");
        }
    }

    /// <summary>
    /// 修改开播提示设置
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/set_rooms/[controller]")]
    [Login]
    [Tags("set_rooms")]
    public class modify_room_prompt_settings : ControllerBase
    {
        /// <summary>
        /// 批量修改房间的开播提醒设置
        /// </summary>
        /// <param name="uid">要修改开播提示提示状态的房间UID列表</param>
        /// <param name="state">将房间的开播提示状态设置为什么状态</param>
        /// <param name="commonParameters"></param>
        /// <returns></returns>
        [HttpPost(Name = "modify_room_prompt_settings")]
        public ActionResult Post([FromForm] List<long> uid, [FromForm] bool state, PostCommonParameters commonParameters)
        {
            List<long> count = Core.RuntimeObject._Room.ModifyRoomPromptSettings(uid, state);
            return Content(MessageBase.MssagePack(nameof(modify_room_prompt_settings), count, $"返回列表中的房间的开播提示修改为{state}"), "application/json");
        }
    }

    /// <summary>
    /// 修改弹幕录制设置
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/set_rooms/[controller]")]
    [Login]
    [Tags("set_rooms")]
    public class modify_room_dm_settings : ControllerBase
    {
        /// <summary>
        /// 批量修改房间的弹幕录制设置
        /// </summary>
        /// <param name="uid">要修改弹幕录制状态的房间UID列表</param>
        /// <param name="state">将房间的弹幕录制设置为什么状态</param>
        /// <param name="commonParameters"></param>
        /// <returns></returns>
        [HttpPost(Name = "modify_room_dm_settings")]
        public ActionResult Post([FromForm] List<long> uid, [FromForm] bool state, PostCommonParameters commonParameters)
        {
            List<long> count = Core.RuntimeObject._Room.ModifyRoomDmSettings(uid, state);
            return Content(MessageBase.MssagePack(nameof(modify_room_dm_settings), count, $"返回列表中房间的弹幕录制修改为{state}"), "application/json");
        }
    }


    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/set_rooms/[controller]")]
    [Login]
    [Tags("set_rooms")]
    public class add_room : ControllerBase
    {
        /// <summary>
        /// 添加房间(UID和房间号二选一)
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="auto_rec">是否自动录制</param>
        /// <param name="remind">是否开播提示</param>
        /// <param name="rec_danmu">是否录制弹幕</param>
        /// <param name="uid"></param>
        /// <param name="room_id"></param>
        /// <returns></returns>
        [HttpPost(Name = "add_room")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] bool auto_rec, [FromForm] bool remind, [FromForm] bool rec_danmu, [FromForm] long uid = 0, [FromForm] long room_id = 0)
        {
            var TaskInfo = Core.RuntimeObject._Room.AddRoom(auto_rec, remind, rec_danmu, uid, room_id, false);
            return Content(MessageBase.MssagePack(nameof(modify_room_prompt_settings), TaskInfo.State, $"{TaskInfo.Message}"), "application/json");
        }
    }

    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/set_rooms/[controller]")]
    [Login]
    [Tags("set_rooms")]
    public class batch_add_room : ControllerBase
    {
        /// <summary>
        /// 批量增加房间
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="uids">使用半角逗号分割的uid字符串</param>
        /// <param name="auto_rec">是否自动录制</param>
        /// <param name="remind">是否开播提示</param>
        /// <param name="rec_danmu">是否录制弹幕</param>
        /// <returns></returns>
        [HttpPost(Name = "batch_add_room")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] string uids, [FromForm] bool auto_rec, [FromForm] bool remind, [FromForm] bool rec_danmu)
        {
            List<(long key, int State, string Message)> list = new();
            string[] uid = uids.Split(',');

            foreach (var item in uid)
            {
                if (long.TryParse(item, out long u))
                {
                    (long key, int State, string Message) Info = Core.RuntimeObject._Room.AddRoom(auto_rec, remind, rec_danmu, u, 0, true);
                    list.Add(Info); 
                }
            }
            return Content(MessageBase.MssagePack(nameof(batch_add_room), list), "application/json");
        }
    }

    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/set_rooms/[controller]")]
    [Login]
    [Tags("set_rooms")]
    public class batch_delete_rooms : ControllerBase
    {
        /// <summary>
        /// 批量删除房间
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="uids">使用半角逗号分割的uid字符串</param>
        /// <returns></returns>
        [HttpPost(Name = "batch_delete_rooms")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] string uids)
        {
            List<(long key, bool State, string Message)> list = new();
            string[] uid = uids.Split(',');
            foreach (var item in uid)
            {
                if (long.TryParse(item, out long u))
                {
                    (long key, bool State, string Message) Info = Core.RuntimeObject._Room.DelRoom(u, 0, true);
                    list.Add(Info);
                }

            }
            return Content(MessageBase.MssagePack(nameof(batch_delete_rooms), list), "application/json");
        }
    }


    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/set_rooms/[controller]")]
    [Login]
    [Tags("set_rooms")]
    public class del_room : ControllerBase
    {
        /// <summary>
        /// 删除房间(UID和房间号二选一)
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="uid"></param>
        /// <param name="room_id"></param>
        /// <returns></returns>
        [HttpPost(Name = "del_room")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] long uid = 0, [FromForm] long room_id = 0)
        {
            var TaskInfo = Core.RuntimeObject._Room.DelRoom(uid, room_id,true);
            return Content(MessageBase.MssagePack(nameof(del_room), TaskInfo.State, $"{TaskInfo.Message}"), "application/json");
        }
    }
    /// <summary>
    /// 修改单个房间配置
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/set_rooms/[controller]")]
    [Login]
    [Tags("set_rooms")]
    public class modify_room_settings : ControllerBase
    {
        /// <summary>
        /// 修改单个房间配置
        /// </summary>
        /// <param name="uid">要修改开播提示提示状态的房间UID</param>
        /// <param name="commonParameters"></param>
        /// <param name="AutoRec">是否开播录像</param>
        /// <param name="RecDanmu">是否录制弹幕(打开录像才生效)</param>
        /// <param name="Remind">是否开播提醒</param>
        /// <returns></returns>
        [HttpPost(Name = "modify_room_settings")]
        public ActionResult Post([FromForm] long uid, [FromForm] bool AutoRec, [FromForm] bool Remind, [FromForm] bool RecDanmu, PostCommonParameters commonParameters)
        {
            bool state = Core.RuntimeObject._Room.ModifyRoomSettings(uid, AutoRec, Remind, RecDanmu);
            return Content(MessageBase.MssagePack(nameof(modify_room_settings), state, $"修改房间设置" + (state ? "成功" : "失败")), "application/json");
        }
    }
}
