using CLI.WebAppServices.Middleware;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using static CLI.WebAppServices.Middleware.InterfaceAuthentication;

namespace CLI.WebAppServices.Api
{
    /// <summary>
    /// 修改开播提示设置
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/set_rooms/[controller]")]
    public class get_login_qr : ControllerBase
    {
        /// <summary>
        /// 获取登陆二维码
        /// </summary>
        /// <returns></returns>
        [HttpPost(Name = "get_login_qr")]
        public ActionResult Post([FromForm] List<long> uid, [FromForm] bool state, PostCommonParameters commonParameters)
        {
            FileInfo fi = new FileInfo(Core.Config.Core._QrFileNmae);
            FileStream fs = fi.OpenRead(); ;
            byte[] buffer = new byte[fi.Length];
            //读取图片字节流
            fs.Read(buffer, 0, Convert.ToInt32(fi.Length));
            var response = File(buffer, "image/png");
            fs.Close();
            return response;
        }
    }
    /// <summary>
    /// 修改开播提示设置
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/set_rooms/[controller]")]
    public class use_agree : ControllerBase
    {
        /// <summary>
        /// 同意用户协议
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        [HttpPost(Name = "use_agree")]
        public ActionResult Get([FromQuery] string check = "n")
        {
            if (check == "y")
            {
                Core.Config.Core._UseAgree = true;
                return Content(MessageBase.Success(nameof(use_agree), true, $"用户已同意使用须知"), "application/json");
            }
            else
            {
                Core.Config.Core._UseAgree = false;
                return Content(MessageBase.Success(nameof(use_agree), false, $"用户未同意使用须知",MessageBase.code.LoginInfoFailure), "application/json");
            }
        }
    }
}
