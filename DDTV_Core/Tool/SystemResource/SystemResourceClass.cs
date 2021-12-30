using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DDTV_Core.Tool.SystemResource.GetMemInfo;

namespace DDTV_Core.Tool.SystemResource
{
    public class SystemResourceClass
    {
        /// <summary>
        /// 平台
        /// </summary>
        public string Platform { set; get; }
        /// <summary>
        /// CPU使用率
        /// </summary>
        public double CPU_usage { set; get; }
        /// <summary>
        /// 内存
        /// </summary>
        public MemInfo Memory { set; get; }  
        /// <summary>
        /// 硬盘信息
        /// </summary>
        public List<HDDInfo> HDDInfo { set; get; }
    }
}
