using Server.WebAppServices.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using static Server.WebAppServices.Middleware.InterfaceAuthentication;

namespace Server.WebAppServices.Api
{
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/rec_task/[controller]")]
    [Login]
    [Tags("rec_task")]
    public class cancel_task : ControllerBase
    {
        /// <summary>
        /// 取消录制任务(UID和房间号二选一)
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="uid"></param>
        /// <param name="room_id"></param>
        /// <returns></returns>
        [HttpPost(Name = "cancel_task")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] long uid = 0, [FromForm] long room_id = 0)
        {
            var TaskInfo = Core.RuntimeObject._Room.CancelTask(uid, room_id);
            return Content(MessageBase.MssagePack(nameof(cancel_task), TaskInfo.State, $"{TaskInfo.Message}"), "application/json");
        }
    }

    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/rec_task/[controller]")]
    [Login]
    [Tags("rec_task")]
    public class single_task : ControllerBase
    {
        /// <summary>
        /// 手动增加一个录制任务(UID和房间号二选一)，如果直播还未开始，则预约下一场开始的直播（如果主播中途下播再上播则不会再次录制）
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="uid"></param>
        /// <param name="room_id"></param>
        /// <returns></returns>
        [HttpPost(Name = "single_task")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] long uid = 0, [FromForm] long room_id = 0)
        {
            var TaskInfo = Core.RuntimeObject._Room.AddTask(uid, room_id);
            return Content(MessageBase.MssagePack(nameof(single_task), TaskInfo.State, $"{TaskInfo.Message}"), "application/json");
        }
    }

    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/rec_task/[controller]")]
    [Login]
    [Tags("rec_task")]
    public class cut_task : ControllerBase
    {
        /// <summary>
        /// 手动切断并重连当前任务(UID和房间号二选一)，网络状态和硬盘太慢的不推荐调用
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="uid"></param>
        /// <param name="room_id"></param>
        /// <returns></returns>
        [HttpPost(Name = "cut_task")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] long uid = 0, [FromForm] long room_id = 0)
        {
            var TaskInfo = Core.RuntimeObject._Room.CutTask(uid, room_id);
            return Content(MessageBase.MssagePack(nameof(cut_task), TaskInfo.State, $"{TaskInfo.Message}"), "application/json");
        }
    }
}
