using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace DDTV_WEB_Server.Controllers
{

    /// <summary>
    /// 通过B站搜索搜索直播用户
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class User_Search : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "User_Search")]
        public string post([FromForm] string cmd, [FromForm] string keyword)
        {
            return MessageBase.Success(nameof(User_Search), DDTV_Core.SystemAssembly.BilibiliModule.API.Search.Search.TypeSearch(keyword));
        }
    }

}
