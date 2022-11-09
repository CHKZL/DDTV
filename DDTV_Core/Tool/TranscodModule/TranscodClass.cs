using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.Tool.TranscodModule
{
    public class TranscodClass
    {
        /// <summary>
        /// 转码任务状态，T为正在转码
        /// </summary>
        public bool IsTranscod { get; set; } = true;
        /// <summary>
        /// 转码后文件储存路径
        /// </summary>
        public string AfterFilePath { get; set; }
        /// <summary>
        /// 需要转码的文件路径
        /// </summary>
        public string BeforeFilePath { get; set; }
        /// <summary>
        /// 修改后的文件拓展名
        /// </summary>
        public string AfterFilenameExtension { get; set; }
        /// <summary>
        /// 转码进度
        /// </summary>
        public int Progress { set; get; }
        /// <summary>
        /// 自定义转码参数
        /// </summary>
        public string Parameters { get; set; }
        /// <summary>
        /// HLS调用转码需要提供的文件列表
        /// </summary>
        public List<string> HLS_Files { set; get; }
    }
}
