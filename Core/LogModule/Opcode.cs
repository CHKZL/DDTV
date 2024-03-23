using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.LogModule
{
    public class Opcode
    {
        public enum Code
        { 
            /// <summary>
            /// 读取配置文件
            /// </summary>
            ReadingConfigurationFile=10101,
            /// <summary>
            /// 更新到配置文件
            /// </summary>
            UpdateToConfigurationFile=10102,
            /// <summary>
            /// 读取房间文件
            /// </summary>
            ReadingRoomFiles=10103,
            /// <summary>
            /// 更新到房间文件
            /// </summary>
            UpdateToRoomFile=10104,
            /// <summary>
            /// 修改配置
            /// </summary>
            ModifyConfiguration=10105,
        }
    }
}
