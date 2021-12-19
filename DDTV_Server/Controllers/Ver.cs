using Microsoft.AspNetCore.Mvc;

namespace DDTV_Server.Controllers
{
    public class Ver : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "Ver")]
        //[Consumes("application/json")]
        public MessageBase.pack<VerClass> Post([FromForm] int Ver)
        {
            //Response.ContentType = "application/json";
            switch (Ver)
            {
                case 1:
                    break;
                case 2:
                    break;
            }
            VerClass verClass = new VerClass()
            {
                Ver = System.IO.File.ReadAllText("./Ver.ini")
            };
            return MessageBase.Success(nameof(Ver), verClass);
        }
        public class VerClass
        {
            public string Ver { get; set; }
        }
    }
}