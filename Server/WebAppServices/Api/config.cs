using Server.WebAppServices.Middleware;
using Core.Network.Methods;
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
                Core.Config.Core_RunConfig._RecFileDirectory = path;
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
            return Content(MessageBase.MssagePack(nameof(get_recording_path), Core.Config.Core_RunConfig._RecFileDirectory, $"获取录制文件储存路径（字符串）"), "application/json");
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
        /// 设置录制储存路径中的子路径和格式(default_liver_folder_name/default_data_folder_name/default_file_name.* 就是最终在录制文件夹里面的格式
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="default_liver_folder_name">默认一级主播名文件夹格式</param>
        /// <param name="default_data_folder_name">默认二级主播名下日期分类文件夹格式</param>
        /// <param name="default_file_name">默认下载文件名格式</param>
        /// <param name="check">二次确认key</param>
        /// <returns></returns>
        [HttpPost(Name = "set_default_file_path_name_format")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] string default_liver_folder_name,[FromForm] string default_data_folder_name,[FromForm] string default_file_name, [FromForm] string check = "")
        {
            if (string.IsNullOrEmpty(check))
            {
                string A = $"{default_liver_folder_name}/{default_data_folder_name}{(string.IsNullOrEmpty(default_data_folder_name)?"":"/")}{default_file_name}";
                cache.set_default_all_name = A;
                cache.set_default_liver_folder_name = default_liver_folder_name;
                cache.set_default_data_folder_name = default_data_folder_name;
                cache.set_default_file_name = default_file_name;
                return Content(MessageBase.MssagePack(nameof(set_default_file_path_name_format), A,
                    $"正在将录制储存路径中的子路径和格式修改为[{A}]，请二次确认，将返回的data数据中的key，加到到接口中再次提交。请注意，如果格式有误将录制失败和错误，二次确认提交后“get_file_structure”接口以及返回具体的文件流功能将会失效或出现异常，直到下一次启动"),
                    "application/json");
            }
            if (check != cache.set_default_all_name)
            {
                return Content(MessageBase.MssagePack(nameof(set_default_file_path_name_format), false, $"二次确认的key不正确"), "application/json");
            }
            else
            {
                Core.Config.Core_RunConfig._DefaultLiverFolderName = cache.set_default_liver_folder_name;
                Core.Config.Core_RunConfig._DefaultDataFolderName = cache.set_default_data_folder_name;
                Core.Config.Core_RunConfig._DefaultFileName = cache.set_default_file_name;
                cache.set_default_all_name = Guid.NewGuid().ToString();
                cache.set_default_liver_folder_name = Guid.NewGuid().ToString();
                cache.set_default_data_folder_name = Guid.NewGuid().ToString();
                cache.set_default_file_name = Guid.NewGuid().ToString();
                return Content(MessageBase.MssagePack(nameof(set_default_file_path_name_format), true,
                    $"正在将录制储存路径中的子路径和格式修改为[{default_liver_folder_name}/{default_data_folder_name}/{default_file_name}]，二次确认完成，“get_file_structure”接口以及返回具体的文件流功能将已失效或出现异常，重启后恢复"),
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
            string Full = $"{Core.Config.Core_RunConfig._DefaultLiverFolderName}/{Core.Config.Core_RunConfig._DefaultDataFolderName}{(string.IsNullOrEmpty(Core.Config.Core_RunConfig._DefaultDataFolderName) ? "" : "/")}{Core.Config.Core_RunConfig._DefaultFileName}";
            (string Full, string default_liver_folder_name, string default_data_folder_name, string default_file_name) data = new()
            {
                Full = Full,
                default_liver_folder_name = Core.Config.Core_RunConfig._DefaultLiverFolderName,
                default_data_folder_name = Core.Config.Core_RunConfig._DefaultDataFolderName,
                default_file_name = Core.Config.Core_RunConfig._DefaultFileName
            };
            return Content(MessageBase.MssagePack(nameof(get_default_file_path_name_format), data, $"录制储存路径中的子路径和格式"), "application/json");
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
            Core.Tools.FileOperations.Delete(Core.Config.Core_RunConfig._ConfigurationFile);
            return Content(MessageBase.MssagePack(nameof(restore_all_settings_to_default), "", $"恢复所有设置为默认值，请重新启动程序，重启后生效"), "application/json");
        }
    }

    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/config/[controller]")]
    [Login]
    [Tags("config")]
    public class set_hls_waiting_time : ControllerBase
    {

        /// <summary>
        /// 修改HLS等待时间
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="waitingtime">等待时间</param>
        /// <returns></returns>
        [HttpPost(Name = "set_hls_waiting_time")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] int waiting_time)
        {
            Core.Config.Core_RunConfig._HlsWaitingTime = waiting_time;
            return Content(MessageBase.MssagePack(nameof(set_hls_waiting_time), "", $"将HLS等待时间修改为{waiting_time}秒"), "application/json");
        }
    }

    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/config/[controller]")]
    [Login]
    [Tags("config")]
    public class get_hls_waiting_time : ControllerBase
    {

        /// <summary>
        /// 获取HLS等待时间
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <returns></returns>
        [HttpGet(Name = "get_hls_waiting_time")]
        public ActionResult Get(GetCommonParameters commonParameters)
        {
            return Content(MessageBase.MssagePack(nameof(get_hls_waiting_time), Core.Config.Core_RunConfig._HlsWaitingTime, $""), "application/json");
        }
    }

    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/config/[controller]")]
    [Login]
    [Tags("config")]
    public class set_automatic_repair : ControllerBase
    {

        /// <summary>
        /// 设置自动修复设置状态
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="automatic_repair">文件写入完成的时候是否修复</param>
        /// <returns></returns>
        [HttpPost(Name = "set_automatic_repair")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] bool automatic_repair)
        {
            Core.Config.Core_RunConfig._AutomaticRepair = automatic_repair;
            return Content(MessageBase.MssagePack(nameof(set_automatic_repair), "", $"将自动修复设置为{automatic_repair}"), "application/json");
        }
    }
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/config/[controller]")]
    [Login]
    [Tags("config")]
    public class get_automatic_repair : ControllerBase
    {

        /// <summary>
        /// 获取自动修复设置状态
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <returns></returns>
        [HttpGet(Name = "get_automatic_repair")]
        public ActionResult Get(GetCommonParameters commonParameters)
        {
            return Content(MessageBase.MssagePack(nameof(get_automatic_repair), Core.Config.Core_RunConfig._AutomaticRepair, $""), "application/json");
        }
    }

    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/config/[controller]")]
    [Login]
    [Tags("config")]
    public class reinitialize : ControllerBase
    {

        /// <summary>
        /// 设置录制储存路径中的子路径和格式
        /// </summary>
        /// <param name="commonParameters"></param>
        /// <param name="check">二次确认key</param>
        /// <returns></returns>
        [HttpPost(Name = "reinitialize")]
        public ActionResult Post(PostCommonParameters commonParameters, [FromForm] string check = "")
        {
            if (string.IsNullOrEmpty(check))
            {
                cache.reinitialize = Guid.NewGuid().ToString();
                return Content(MessageBase.MssagePack(nameof(reinitialize), cache.reinitialize,
                    $"正在进行重新初始化，请二次确认，将返回的data数据中的key，加到到接口中再次提交。请注意，该操作完成后，将会把所有现有配置文件清空，并且自动结束运行。运行后请自行重新启动应进程。"),
                    "application/json");
            }
            if (check != cache.reinitialize)
            {
                return Content(MessageBase.MssagePack(nameof(reinitialize), false, $"二次确认的key不正确"), "application/json");
            }
            else
            {
                Task.Run(() =>
                {
                    Thread.Sleep(3000);
                    //删除user文件
                    string[] files = Directory.GetFiles(Core.Config.Core_RunConfig._ConfigDirectory, $"*{Core.Config.Core_RunConfig._UserInfoCoinfFileExtension}");
                    foreach (string file in files)
                    {
                        if (System.IO.File.Exists(file))
                        {
                            System.IO.File.Delete(file);
                        }
                    }
                    //删除房间配置文件
                    if (System.IO.File.Exists(Core.Config.Core_RunConfig._RoomConfigFile))
                    {
                        System.IO.File.Delete(Core.Config.Core_RunConfig._RoomConfigFile);
                    }
                    //删除配置文件
                    if (System.IO.File.Exists(Core.Config.Core_RunConfig._ConfigurationFile))
                    {
                        System.IO.File.Delete(Core.Config.Core_RunConfig._ConfigurationFile);
                    }

                    while (System.IO.File.Exists(Core.Config.Core_RunConfig._RoomConfigFile) || System.IO.File.Exists(Core.Config.Core_RunConfig._ConfigurationFile))
                    {
                        Thread.Sleep(10);
                    }
                    Environment.FailFast("核心配置被重置，手动触发异常，停止运行。");
                });
                return Content(MessageBase.MssagePack(nameof(reinitialize), true,
                    $"正在进行重新初始化，二次确认完成。所有现有配置文件清空，3秒后程序自动结束运行。请自行重新启动应进程。"),
                    "application/json");
            }
        }
    }



}
