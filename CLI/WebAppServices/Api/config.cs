using CLI.WebAppServices.Middleware;
using Core.Network.Methods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using static CLI.WebAppServices.Middleware.InterfaceAuthentication;
using static Core.Tools.FileOperations;

namespace CLI.WebAppServices.Api
{
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/config/[controller]")]
    [Login]
    [Tags("config")]
    public class reload_configuration : ControllerBase
    {
        /// <summary>
        /// 重新从配置文件加载配置到内存
        /// </summary>
        /// <returns></returns>
        [HttpPost(Name = "reload_configuration")]
        public ActionResult Post(PostCommonParameters commonParameters)
        {
            Core.Config.ReadConfiguration();
            return Content(MessageBase.MssagePack(nameof(reload_configuration), true, $"从配置文件重新加载配置\r\n请注意，如果修改了路径相关配置，之后调用路径相关接口获取到的都会是新的配置，可能会造成一场直播写到两个路径中的问题"), "application/json");
        }
    }


    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/config/[controller]")]
    [Login]
    [Tags("config")]
    public class set_recording_path : ControllerBase
    {

        /// <summary>
        /// 设置录制文件储存路径（字符串）
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="path">录制文件储存路径（字符串）</param>
        /// <param name="check">二次确认key</param>
        /// <returns></returns>
        [HttpPost(Name = "set_recording_path")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] string path, [FromForm] string check = "")
        {
            if (string.IsNullOrEmpty(check))
            {
                path = CreateAll(path);
                if (string.IsNullOrEmpty(path))
                {
                    return Content(MessageBase.MssagePack(nameof(set_recording_path), "错误",
                   $"正在将录制路径格式修改为{path}，格式不符合要求，无法创建，请检查"),
                   "application/json");
                }
                cache.set_recording_path = Guid.NewGuid().ToString();
                return Content(MessageBase.MssagePack(nameof(set_recording_path), cache.set_recording_path,
                    $"正在将录制路径格式修改为{path}，请二次确认，将返回的data数据中的key，加到到接口中再次提交。请注意，二次确认提交后“get_file_structure”接口以及返回具体的文件流功能将会失效，直到下一次启动"),
                    "application/json");
            }
            if (check != cache.set_recording_path)
            {
                return Content(MessageBase.MssagePack(nameof(set_recording_path), false, $"二次确认的key不正确"), "application/json");
            }
            else
            {
                Core.Config.Core._RecFileDirectory = path;
                return Content(MessageBase.MssagePack(nameof(set_recording_path), true,
                    $"正在将录制路径格式修改为{path}，二次确认完成，“get_file_structure”接口以及返回具体的文件流功能将已失效，重启后恢复"),
                    "application/json");
            }
        }
    }

    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/config/[controller]")]
    [Login]
    [Tags("config")]
    public class get_recording_path : ControllerBase
    {

        /// <summary>
        /// 获取录制文件储存路径（字符串）
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <returns></returns>
        [HttpGet(Name = "get_recording_path")]
        public ActionResult Get(GetCommonParameters commonParameters)
        {
            return Content(MessageBase.MssagePack(nameof(get_recording_path), Core.Config.Core._RecFileDirectory, $"获取录制文件储存路径（字符串）"), "application/json");
        }
    }


    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/config/[controller]")]
    [Login]
    [Tags("config")]
    public class set_default_file_path_name_format : ControllerBase
    {

        /// <summary>
        /// 设置录制储存路径中的子路径和格式
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="path_and_format">保存的文件以怎样的路径和名称格式保存在录制文件夹中</param>
        /// <param name="check">二次确认key</param>
        /// <returns></returns>
        [HttpPost(Name = "set_default_file_path_name_format")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] string path_and_format, [FromForm] string check = "")
        {
            if (string.IsNullOrEmpty(check))
            {
                cache.set_default_file_path_name_format = Guid.NewGuid().ToString();
                return Content(MessageBase.MssagePack(nameof(set_default_file_path_name_format), cache.set_default_file_path_name_format,
                    $"正在将录制储存路径中的子路径和格式修改为{path_and_format}，请二次确认，将返回的data数据中的key，加到到接口中再次提交。请注意，如果格式有误将录制失败和错误，二次确认提交后“get_file_structure”接口以及返回具体的文件流功能将会失效或出现异常，直到下一次启动"),
                    "application/json");
            }
            if (check != cache.set_default_file_path_name_format)
            {
                return Content(MessageBase.MssagePack(nameof(set_default_file_path_name_format), false, $"二次确认的key不正确"), "application/json");
            }
            else
            {
                Core.Config.Core._DefaultFilePathNameFormat = path_and_format;
                return Content(MessageBase.MssagePack(nameof(set_default_file_path_name_format), true,
                    $"正在将录制储存路径中的子路径和格式修改为{path_and_format}，二次确认完成，“get_file_structure”接口以及返回具体的文件流功能将已失效或出现异常，重启后恢复"),
                    "application/json");
            }
        }
    }
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/config/[controller]")]
    [Login]
    [Tags("config")]
    public class get_default_file_path_name_format : ControllerBase
    {

        /// <summary>
        /// 获取录制储存路径中的子路径和格式
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <returns></returns>
        [HttpGet(Name = "get_default_file_path_name_format")]
        public ActionResult Get(GetCommonParameters commonParameters)
        {
            return Content(MessageBase.MssagePack(nameof(get_default_file_path_name_format), Core.Config.Core._DefaultFilePathNameFormat, $"录制储存路径中的子路径和格式"), "application/json");
        }
    }

    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/config/[controller]")]
    [Login]
    [Tags("config")]
    public class restore_all_settings_to_default : ControllerBase
    {

        /// <summary>
        /// 恢复所有设置为默认
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <returns></returns>
        [HttpGet(Name = "restore_all_settings_to_default")]
        public ActionResult Get(GetCommonParameters commonParameters)
        {
            //直接删掉配置文件，重启后会自动生成的
            Core.Tools.FileOperations.Delete(Core.Config.Core._ConfigurationFile);
            return Content(MessageBase.MssagePack(nameof(restore_all_settings_to_default), "", $"恢复所有设置为默认值，请重新启动程序，重启后生效"), "application/json");
        }
    }
}
