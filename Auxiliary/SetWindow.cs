using System;
using System.Collections.Generic;
using System.Text;

namespace Auxiliary
{
    public class SetWindow
    {
        public class 窗口信息
        {
            public double X坐标 { set; get; } = -1;
            public double Y坐标 { set; get; } = -1;
            public double 宽度 { set; get; } = -1;
            public double 高度 { set; get; } = -1;
            public string 标题 { set; get; } = null;
            public string GUID { set; get; }
        }
    }
}
