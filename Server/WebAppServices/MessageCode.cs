using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.WebAppServices
{
    public class MessageCode
    {
        /// <summary>
        /// 返回的状态码
        /// </summary>
        public enum code
        {
            /// <summary>
            /// 请求成功
            /// </summary>
            ok = 0,
            /// <summary>
            /// 参数有误
            /// </summary>
            ParameterError = 5000,
            /// <summary>
            /// 登陆信息失效
            /// </summary>
            LoginInfoFailure = 6000,
            /// <summary>
            /// 操作失败
            /// </summary>
            OperationFailed = 7000,
            /// <summary>
            /// 读取配置文件完成
            /// </summary>
            ReadingConfigurationFileComplet=10101,

        }
    }
}
