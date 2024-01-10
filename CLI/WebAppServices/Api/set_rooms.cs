using CLI.WebAppServices.Middleware;
using Core.LogModule;
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

namespace CLI.WebAppServices.Api
{
    /// <summary>
    /// 修改自动录制设置
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/set_rooms/[controller]")]
    [Login]
    public class ModifyRecordingSettings : ControllerBase
    {
        /// <summary>
        /// 批量修改房间的录制设置
        /// </summary>
        /// <param name="uid">要修改录制状态的房间UID列表</param>
        /// <param name="state">将房间的录制状态设置为什么状态</param>
        /// <param name="commonParameters"></param>
        /// <returns></returns>
        [HttpPost(Name = "ModifyRecordingSettings")]
        public ActionResult Post([FromForm] List<long> uid, [FromForm] bool state, PostCommonParameters commonParameters)
        {
            List<long> count = Core.RuntimeObject._Room.ModifyRecordingSettings(uid, state);
            return Content(MessageBase.Success(nameof(ModifyRecordingSettings),count,$"返回列表中的房间的自动录制修改为{state}"), "application/json");
        }
    }

    /// <summary>
    /// 修改开播提示设置
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/Rooms/[controller]")]
    [Login]
    public class ModifyRoomPromptSettings : ControllerBase
    {
        /// <summary>
        /// 批量修改房间的录制设置
        /// </summary>
        /// <param name="uid">要修改开播提示提示状态的房间UID列表</param>
        /// <param name="state">将房间的开播提示状态设置为什么状态</param>
        /// <param name="commonParameters"></param>
        /// <returns></returns>
        [HttpPost(Name = "ModifyRoomPromptSettings")]
        public ActionResult Post([FromForm] List<long> uid, [FromForm] bool state, PostCommonParameters commonParameters)
        {
            List<long> count = Core.RuntimeObject._Room.ModifyRoomPromptSettings(uid, state);
            return Content(MessageBase.Success(nameof(ModifyRoomPromptSettings), count, $"返回列表中的房间的开播提示修改为{state}"), "application/json");
        }
    }
}
