using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Net.Mime;

namespace DDTV_WEB_Server.Controllers
{
    [Route("api/[controller]")]
    public class loginqr: ControllerBase
    {
        [HttpPost(Name = "loginqr")]
        public ActionResult post()
        {
            Image bmp = Bitmap.FromFile("./BiliQR.png");
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            bmp.Dispose();
            return File(ms.ToArray(), "image/png");
        }
       
    }
}
