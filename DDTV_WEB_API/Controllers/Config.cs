using DDTV_Core.SystemAssembly.ConfigModule;
using DDTV_Core.SystemAssembly.DownloadModule;
using DDTV_Core.SystemAssembly.NetworkRequestModule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DDTV_WEB_API.Controllers
{
    public class Transcod : ProcessingControllerBase.ApiControllerBase
    {
        [HttpGet(Name = "Transcod")]
        public string get(bool state)
        {
            DDTV_Core.Tool.TranscodModule.Transcod.IsAutoTranscod = state;
            CoreConfig.SetValue(CoreConfigClass.Key.IsAutoTranscod, state.ToString(), CoreConfigClass.Group.Core);
            return MessageBase.Success(nameof(Transcod), (state ? "打开" : "关闭") + "自动转码成功");
        }
    }
    public class FileSplit : ProcessingControllerBase.ApiControllerBase
    {
        [HttpGet(Name = "FileSplit")]
        public string get(long Size)
        {
            if(Size>0)
            {
                Download.IsFlvSplit = true;
                CoreConfig.SetValue(CoreConfigClass.Key.IsFlvSplit, Download.IsFlvSplit.ToString(), CoreConfigClass.Group.Download);
                return MessageBase.Success(nameof(Transcod), $"启用录制文件大小限制(自动分割)，设置大小为:{NetClass.ConversionSize(Size)}");
            }
            else
            {
                Download.IsFlvSplit = false;
                CoreConfig.SetValue(CoreConfigClass.Key.IsFlvSplit, Download.IsFlvSplit.ToString(), CoreConfigClass.Group.Download);
                return MessageBase.Success(nameof(Transcod), $"已关闭文件大小限制(自动分割)");
            }
        }
    }
    public class DanmuRec : ProcessingControllerBase.ApiControllerBase
    {
        [HttpGet(Name = "DanmuRec")]
        public string get(bool state)
        {
            Download.IsRecDanmu = state;
            CoreConfig.SetValue(CoreConfigClass.Key.IsRecDanmu, state.ToString(), CoreConfigClass.Group.Download);
            return MessageBase.Success(nameof(Transcod), (state ? "打开" : "关闭") + "弹幕录制(包括礼物、舰队、SC)  (每个房间自己在房间配置列表单独设置，这个只是是否启用弹幕录制功能的总共开关)");        
        }
    }
    public class GetFollow : ProcessingControllerBase.ApiControllerBase
    {
        [HttpGet(Name = "GetFollow")]
        public string get()
        {
            if(!string.IsNullOrEmpty(BilibiliUserConfig.account.uid))
            {
                int AddConut = DDTV_Core.SystemAssembly.BilibiliModule.API.UserInfo.follow(long.Parse(BilibiliUserConfig.account.uid));
                return MessageBase.Success(nameof(Transcod), $"成功导入{AddConut}个关注列表中的到配置");
            }
            else
            {
                return MessageBase.Success(nameof(Transcod), $"未登录！");
            }
            
        }
    }
}
