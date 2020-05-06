using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    public class BiliVideoInfo
    {
        public class VideoInfo
        {
			public static List<Root> Info = new List<Root>();
            public class Root
            {
                public int code { get; set; }

                public string message { get; set; }

				public int ttl { get; set; }
				public string title { set; get; }
				public string BV { set; get; }

				public List<data> data { get; set; }
			}

			public class data
			{
				/// <summary>
				/// 视频CIA，最小单位
				/// </summary>
				public int cid { get; set; }
				/// <summary>
				/// 分P
				/// </summary>
				public int page { get; set; }
				/// <summary>
				/// 来源类别
				/// </summary>
				public string from { get; set; }
				/// <summary>
				/// 分P名称
				/// </summary>
				public string part { get; set; }
				/// <summary>
				/// 长度,秒
				/// </summary>
				public int duration { get; set; }
				/// <summary>
				/// vid
				/// </summary>
				public string vid { get; set; }
				/// <summary>
				/// weblink
				/// </summary>
				public string weblink { get; set; }
				/// <summary>
				/// 分辨率
				/// </summary>
				public dimension dimension { get; set; }
			}

			public class dimension
			{
				/// <summary>
				/// 视频宽度
				/// </summary>
				public int width { get; set; }
				/// <summary>
				/// 视频高度
				/// </summary>
				public int height { get; set; }
				/// <summary>
				/// 视频旋转
				/// </summary>
				public int rotate { get; set; }
			}

		}
    }
}
