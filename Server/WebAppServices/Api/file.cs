using Server.WebAppServices.Middleware;
using Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using static Server.WebAppServices.Middleware.InterfaceAuthentication;
using static Core.Tools.FileOperations;

namespace Server.WebAppServices.Api
{
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/file/[controller]")]
    [Login]
    [Tags("file")]
    public class get_file_structure : ControllerBase
    {
        /// <summary>
        /// 获取录播文件夹下文件的结构以json格式返回
        /// </summary>
        /// <returns></returns>
        [HttpPost(Name = "get_file_structure")]
        public ActionResult Post(PostCommonParameters commonParameters)
        {
            DirectoryNode Structure = new();
            if(Core.RuntimeObject.RecordingFiles.GetDirectoryStructure(Config.Core_RunConfig._RecFileDirectory, out Structure))
            {
                return Content(MessageBase.MssagePack(nameof(get_file_structure), Structure, $"获取录制文件路径下的文件结构成功"), "application/json");
            }
            else
            {
                return Content(MessageBase.MssagePack(nameof(get_file_structure), false, $"目标路径不存在"), "application/json");
            }
        }
    }
}
