using DDTV_Core.SystemAssembly.BilibiliModule.API;
using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.DownloadModule;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DDTV_WEB_Server.Controllers
{
   /// <summary>
   /// 自动转码开关接口
   /// </summary>
    public class Config_Transcod : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "Config_Transcod")]
        public ActionResult post([FromForm] bool state,[FromForm] string cmd)
        {
            DDTV_Core.Tool.TranscodModule.Transcod.IsAutoTranscod = state;
            CoreConfig.SetValue(CoreConfigClass.Key.IsAutoTranscod, state.ToString(), CoreConfigClass.Group.Core);
            return Content(MessageBase.Success(nameof(Config_Transcod), (state ? "打开" : "关闭") + "自动转码成功"), "application/json");
        }
    }
    /// <summary>
    /// 自动切片接口
    /// </summary>
    public class Config_FileSplit : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "Config_FileSplit")]
        public ActionResult post([FromForm] long Size, [FromForm] string cmd)
        {
            string text="";
            if(Size>0)
            {
                Download.IsFlvSplit = true;
                CoreConfig.SetValue(CoreConfigClass.Key.IsFlvSplit, Download.IsFlvSplit.ToString(), CoreConfigClass.Group.Download);
                text = $"启用录制文件大小限制(自动分割)，设置大小为:{NetClass.ConversionSize(Size)}";
               
            }
            else if(Size == 0)
            {
                Download.IsFlvSplit = false;
                CoreConfig.SetValue(CoreConfigClass.Key.IsFlvSplit, Download.IsFlvSplit.ToString(), CoreConfigClass.Group.Download);
                text = $"已关闭文件大小限制(自动分割)";
            }
            else
            {
                Size = 0;
                Download.IsFlvSplit = false;
                CoreConfig.SetValue(CoreConfigClass.Key.IsFlvSplit, Download.IsFlvSplit.ToString(), CoreConfigClass.Group.Download);
                text = $"收到的Size为负数，自动设置为0，并关闭文件大小限制(自动分割)";
                
            }
            Download.FlvSplitSize = Size;
            CoreConfig.SetValue(CoreConfigClass.Key.FlvSplitSize, Download.FlvSplitSize.ToString(), CoreConfigClass.Group.Download);
            return Content(MessageBase.Success(nameof(Config_FileSplit), text), "application/json");
        }
    }
    /// <summary>
    /// 弹幕总开关接口
    /// </summary>
    public class Config_DanmuRec : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "Config_DanmuRec")]
        public ActionResult post([FromForm] bool state, [FromForm] string cmd)
        {
            Download.IsRecDanmu = state;
            CoreConfig.SetValue(CoreConfigClass.Key.IsRecDanmu, state.ToString(), CoreConfigClass.Group.Download);
            return Content(MessageBase.Success(nameof(Config_DanmuRec), (state ? "打开" : "关闭") + "弹幕录制(包括礼物、舰队、SC)  (每个房间自己在房间配置列表单独设置，这个只是是否启用弹幕录制功能的总共开关)"), "application/json");
        }
    }
    public class Config_GetFollow : ProcessingControllerBase.ApiControllerBase
    {
        [HttpPost(Name = "Config_GetFollow")]
        public ActionResult post([FromForm] string cmd)
        {
            if(!string.IsNullOrEmpty(BilibiliUserConfig.account.uid))
            {
                List<UserInfo.followClass> AddConut = UserInfo.follow(long.Parse(BilibiliUserConfig.account.uid));
                return Content(MessageBase.Success(nameof(Config_GetFollow), AddConut), "application/json");
            }
            else
            {
                return Content(MessageBase.Success(nameof(Config_GetFollow), $"未登录！"), "application/json");
            }
            
        }
    }
}
