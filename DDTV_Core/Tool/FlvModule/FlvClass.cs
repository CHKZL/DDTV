using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.Tool.FlvModule
{
    public class FlvClass
    {
        public class FlvTimes
        {
            public uint FlvTotalTagCount { set; get; } = 0;
            public uint FlvAudioTagCount { set; get; } = 0;
            public uint FlvVideoTagCount { set; get; } = 0;
            public uint PerfectAudioTimes { get; set; } = 0;
            public uint PerfectVideoTimes { get; set; } = 0;
            public bool IsTagHeader { set; get; } = true;
            public uint ErrorAudioTimes { get; set; } = 0;
            public uint ErrorVideoTimes { get; set; } = 0;
            public byte TagType = 0x00;
        }
        public class FlvHeader
        {
            public byte[] Header { set; get; }
            /// <summary>
            /// FLV文件标识签名
            /// </summary>
            public byte[] Signature { get; set; } = new byte[3];
            /// <summary>
            /// FLV版本
            /// </summary>
            public byte Version { get; set; }
            /// <summary>
            /// 5+3数据位（根据FLV文件头计算，表示是否有视频和音频）
            /// </summary>
            public byte Type { get; set; }
            /// <summary>
            /// 是否存在音频
            /// </summary>
            public bool TypeFlagsAudio { get; set; }
            /// <summary>
            /// 是否存在视频
            /// </summary>
            public bool TypeFlagsVideo { get; set; }
            /// <summary>
            /// 文件头长度
            /// </summary>
            public byte[] FlvHeaderOffset { get; set; } = new byte[4];
        }
        public class FlvTag
        {
            public byte[] FistVbody { set; get; }
            public byte[] FistAbody { set; get; }
            public byte[] tag { set; get; }
            /// <summary>
            /// 前一个Tag长度
            /// </summary>
            public byte[] PreTagsize { set; get; } =new byte[4];
            /// <summary>
            /// Tag类型(0x08:音频 0x09:视频 0x12:ScriptData)
            /// </summary>
            public byte TagType { set; get; }
            /// <summary>
            /// TagData的数据长度
            /// </summary>
            public byte[] TagDataSize { set; get; } = new byte[3];
            /// <summary>
            /// Tag时间戳
            /// </summary>
            public byte[] Timestamp { set; get; } = new byte[4];
            /// <summary>
            /// Tag有效数据
            /// </summary>
            public byte[] TagaData { set; get; }
        }

    }
}
