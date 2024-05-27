using Server.WebAppServices.Middleware;
using Core.LogModule;
using Core.RuntimeObject;
using Masuit.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using static Server.WebAppServices.MessageCode;
using static Server.WebAppServices.Middleware.InterfaceAuthentication;

namespace Server.WebAppServices.Api
{
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/login/[controller]")]
    [Tags("login")]
    public class get_login_qr : ControllerBase
    {
        /// <summary>
        /// 获取登陆二维码
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "get_login_qr")]
        public async Task<ActionResult> Get()
        {
            int waitTime = 0;
            while (waitTime <= 3000)
            {
                if (System.IO.File.Exists(Core.Config.Core_RunConfig._QrFileNmae))
                {
                    FileInfo fi = new FileInfo(Core.Config.Core_RunConfig._QrFileNmae);
                    using (FileStream fs = fi.OpenRead())
                    {
                        byte[] buffer = new byte[fi.Length];
                        //读取图片字节流
                        await fs.ReadAsync(buffer, 0, Convert.ToInt32(fi.Length));
                        return File(buffer, "image/png");
                    }
                }
                else
                {
                    await Task.Delay(1000);
                    waitTime += 1000;
                }
            }
            return Content(MessageBase.MssagePack(nameof(get_login_qr), false, $"登陆二维码不存在，请检查是否调用登陆接口且未过期"), "application/json");
        }
    }

    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/login/[controller]")]
    [Tags("login")]
    public class get_login_url : ControllerBase
    {
        /// <summary>
        /// 获取用于生成登陆二维码的字符串
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "get_login_url")]
        public async Task<ActionResult> Get()
        {
            int waitTime = 0;
            while (waitTime <= 3000)
            {
                if (System.IO.File.Exists(Core.Config.Core_RunConfig._QrUrl))
                {
                    FileInfo fi = new FileInfo(Core.Config.Core_RunConfig._QrUrl);
                    using (FileStream fs = fi.OpenRead())
                    {
                        return Content(MessageBase.MssagePack(nameof(get_login_url), fs.ReadAllText(Encoding.UTF8), $"获取用于生成登陆二维码的URL字符串"), "application/json");
                    }
                }
                else
                {
                    await Task.Delay(1000);
                    waitTime += 1000;
                }
            }
            return Content(MessageBase.MssagePack(nameof(get_login_url), false, $"登陆文件不存在，请检查是否调用登陆接口且未过期"), "application/json");
        }
    }

    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/login/[controller]")]
    [Login]
    [Tags("login")]
    public class use_agree : ControllerBase
    {
        /// <summary>
        /// 同意用户协议
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        [HttpPost(Name = "use_agree")]
        public ActionResult Post([FromForm] string check = "n")
        {
             string Message = "用户已同意使用须知";
            if (check == "y")
            {
                Core.Config.Core_RunConfig._UseAgree = true;   
                OperationQueue.Add(Opcode.Account.UserConsentAgreement, Message);
                Log.Info(nameof(use_agree), Message);
                return Content(MessageBase.MssagePack(nameof(use_agree), true, Message), "application/json");
            }
            else
            {
                Core.Config.Core_RunConfig._UseAgree = false;
                Message = "用户未同意使用须知";
                OperationQueue.Add(Opcode.Account.UserDoesNotAgreeToAgreement, Message);
                Log.Info(nameof(use_agree), Message);
                return Content(MessageBase.MssagePack(nameof(use_agree), false, Message, code.LoginInfoFailure), "application/json");
            }
        }
    }
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/login/[controller]")]
    [Login]
    [Tags("login")]
    public class re_login : ControllerBase
    {
        /// <summary>
        /// 重新登陆(登陆后覆盖当前登录态，覆盖前，当前登录态依旧有效)
        /// </summary>
        /// <returns></returns>
        [HttpPost(Name = "re_login")]
        public async Task<ActionResult> Post(PostCommonParameters commonParameters)
        {
            await Login.QR();
            return Content(MessageBase.MssagePack(nameof(re_login), true, $"触发登陆功能，请在1分钟内使用get_login_qr获取登陆二维码进行登陆", code.LoginInfoFailure), "application/json");
        }
    }
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/login/[controller]")]
    [Login]
    [Tags("login")]
    public class use_agree_state : ControllerBase
    {
        /// <summary>
        /// 获得用户初始化授权状态
        /// </summary>
        /// <returns></returns>
        [HttpPost(Name = "use_agree_state")]
        public ActionResult Post(PostCommonParameters commonParameters)
        {
            return Content(MessageBase.MssagePack(nameof(use_agree_state), Core.Config.Core_RunConfig._UseAgree, $"获取用户初始化授权状态"), "application/json");
        }
    }
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/login/[controller]")]
    [Login]
    [Tags("login")]
    public class get_login_status : ControllerBase
    {
        /// <summary>
        /// 获取本地登录态AccountInformation的有效状态
        /// </summary>
        /// <returns></returns>
        [HttpPost(Name = "get_login_status")]
        public ActionResult Post(PostCommonParameters commonParameters)
        {
            return Content(MessageBase.MssagePack(nameof(get_login_status), Core.RuntimeObject.Account.GetLoginStatus(), $"获取本地登录态AccountInformation的有效状态"), "application/json");
        }
    }
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/login/[controller]")]
    [Login]
    [Tags("login")]
    public class get_nav : ControllerBase
    {
        /// <summary>
        /// 获取登陆用户基本信息
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "get_nav")]
        public ActionResult Get(GetCommonParameters commonParameters)
        {
            return Content(MessageBase.MssagePack(nameof(get_nav), Account.nav_info, $""), "application/json");
        }
    }
}
