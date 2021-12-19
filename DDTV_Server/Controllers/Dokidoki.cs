using Microsoft.AspNetCore.Mvc;

namespace DDTV_Server.Controllers
{
    public class Dokidoki : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "Dokidoki")]
        
        public MessageBase.pack<string> Post([FromForm] int Conut)
        {
            return MessageBase.Success(nameof(Ver), $"OK:{Conut}");
        }
    }
}