using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    public class 外部API
    {
        public class 正在直播数据
        {
            public class 直播信息
            {
                public long 实际开始时间 { set; get; }
                public string 频道ID { set; get; }
                public int 频道类型 { set; get; }
                public string 头像URL { set; get; }
                public int 最大观众 { set; get; }
                public string 名称 { set; get; }
                public int 已直播时长_秒 { set; get; }
                public string 已直播时长_日本时间 { set; get; }
                public string 主播名称 { set; get; }
                public string 封面URL { set; get; }
                public int 封面图高度 { set; get; }
                public int 封面图宽度 { set; get; }
                public string 标题 { set; get; }
                public string 当前观众 { set; get; }
                public string 频道相关信息 { set; get; }
                public string 阿B房间号 { set; get; }
                public string 直播连接 { set; get; }
            }
            public List<直播信息> 直播数据 = new List<直播信息>();
            public void 更新正在直播数据()
            {   
                JObject jo = (JObject)JsonConvert.DeserializeObject(MMPU.返回网页内容("https://hiyoko.sonoj.net/f/avtapi/live/fetch_current_v2"));
                foreach (var item in jo["current_live"])
                {
                    try
                    {
                        直播数据.Add(new 直播信息()
                        {
                            实际开始时间 = long.Parse(item["actual_start_time"].ToString()),
                            频道ID = item["ch_id"].ToString(),
                            频道类型 = int.Parse(item["ch_type"].ToString()),
                            头像URL = item["channel_thumbnail_url"].ToString(),
                            //最大观众 = int.Parse(item["max_viewers"].ToString()),
                            名称 = item["name"].ToString(),
                            已直播时长_秒 = int.Parse(item["seconds"].ToString()),
                            已直播时长_日本时间 = item["start_time_str"].ToString(),
                            主播名称 = item["streamer_name"].ToString(),
                            //封面URL = item["thumbnail_url"].ToString(),
                           // 封面图高度 = int.Parse(item["thumbnail_height"].ToString()),
                            //封面图宽度 = int.Parse(item["thumbnail_width"].ToString()),
                            标题 = item["title"].ToString(),
                            当前观众 = item["viewers"].ToString(),
                            频道相关信息=item["channel_misc"].ToString(),
                            阿B房间号= item["desc"].ToString()
                        });
                        直播数据[直播数据.Count() - 1].直播连接 = 根据频道类型返回直播地址(直播数据[直播数据.Count() - 1]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    
                    }
                }
            }
            public static string 根据频道类型返回直播地址(直播信息 A)
            {
                try
                {
                    string URL = string.Empty;
                    switch (A.频道类型)
                    {
                        case 1:
                            URL = "https://www.youtube.com/channel/" + A.频道ID + "/live";
                            break;
                        case 3:
                            URL = "https://www.showroom-live.com/" + A.频道ID.Split('_')[1];
                            break;
                        case 4:
                            URL = "https://live.bilibili.com/" + A.阿B房间号;
                            break;
                        case 7:
                            URL = "https://17.live/live/" + A.频道ID.Split('_')[1];
                            break;
                        case 8:
                            JObject J1 = (JObject)JsonConvert.DeserializeObject(A.频道相关信息);
                            URL = "https://twitcasting.tv/" + J1["screen_name"].ToString(); ;
                            break;
                    }
                    return URL;
                }
                catch (Exception)
                {

                    return null;
                }
            }
        }
    }
}
