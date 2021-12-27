using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace DDTV_WEB_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class File_GetFileList : ProcessingControllerBase.ApiControllerBase
    {   
        [HttpPost(Name = "File_GetFileList")]
        public string post([FromForm] string cmd)
        {        
            return MessageBase.Success(nameof(File_GetFileList), DDTV_Core.Tool.DownloadList.GetRecFileList());
        }   
    }
    [Route("api/[controller]")]
    [ApiController]
    public class File_GetRecFile : ProcessingControllerBase.ApiControllerBase
    {
        [HttpGet(Name = "File_GetRecFile")]
        public ActionResult get([FromForm] string cmd,string FileName)
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
                            return Content(MessageBase.Success(nameof(File_GetRecFile), "该文件不在支持列表内"), "application/json");

                    }
                }      
            }
            else
            {
                return Content(MessageBase.Success(nameof(File_GetRecFile), "该文件不存在"), "application/json");
            }    
        }     
    }
}
