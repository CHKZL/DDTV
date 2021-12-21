using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Net.Mime;

namespace DDTV_WEB_API.Controllers
{
    [Route("api/[controller]")]
    public class loginqr: ControllerBase
    {
        [HttpGet(Name = "loginqr")]
        public ActionResult get()
        {
            Image bmp = Bitmap.FromFile("./BiliQR.png");
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            bmp.Dispose();
            return File(ms.ToArray(), "image/png");
        }
       
    }
    [Route("api/[controller]")]
    public class test : ControllerBase
    {
        [HttpGet(Name = "test")]
        public string get()
        {

            return "stsss";
        }
    }
}
