using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace DDTV_Server
{
    public class ProcessingControllerBase
    {
        [Produces(MediaTypeNames.Application.Json)]
        [ApiController]
        [Route("api/[controller]")]
        [AllowAnonymous]
        public class ApiControllerBase : ControllerBase
        {

        }
    }
}
