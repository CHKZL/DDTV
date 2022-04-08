using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using System.Drawing;
using System.Net.Mime;
using static BiliAccount.Core.ByQRCode;

namespace DDTV_WEB_Server.Controllers
{
    [Route("api/[controller]")]
    public class loginqr: ControllerBase
    {
        [HttpGet(Name = "loginqr")]
        public ActionResult get()
        {
            FileInfo fi = new FileInfo("./BiliQR.png");
            FileStream fs = fi.OpenRead(); ;
            byte[] buffer = new byte[fi.Length];
            //读取图片字节流
            fs.Read(buffer, 0, Convert.ToInt32(fi.Length));
            var response = File(buffer, "image/png");
            fs.Close();
            return response; 
        }  
    }
}
