using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace DDTV_WEB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetFileList : ControllerBase
    {   
        [HttpGet(Name = "GetFileList")]
        public string get()
        {        
            return MessageBase.Success(nameof(Transcod), DDTV_Core.Tool.DownloadList.GetRecFileList());
        }   
    }
    [Route("api/[controller]")]
    [ApiController]
    public class GetRecFile : ControllerBase
    {
        [HttpGet(Name = "GetRecFile")]
        public ActionResult get(string FileName)
        {
            var _ = DDTV_Core.Tool.DownloadList.GetRecFileList();
            if(_.Contains(FileName))
            {
                using (FileStream fs = new FileStream(FileName, FileMode.Open))
                {
                    byte[] bts = new byte[fs.Length];
                    fs.Read(bts, 0, (int)fs.Length);
                    string type = FileName.Split('.')[FileName.Split('.').Length - 1];
                    string Name= FileName.Split('/')[FileName.Split('/').Length - 1];
                    switch (type)
                    {
                        case "flv":
                            return File(bts, "video/mpeg4", Name);         
                        case "mp4":
                            return File(bts, "video/mpeg4", Name);
                        case "xml":
                            return File(bts, "application/xml", Name);  
                        case "csv":
                            return File(bts, "text/plain", Name);
                        default:
                            return Content(MessageBase.Success(nameof(Transcod), "该文件不在支持列表内"), "application/json");

                    }
                }      
            }
            else
            {
                return Content(MessageBase.Success(nameof(Transcod), "该文件不存在"), "application/json");
            }    
        }     
    }
}
