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
                string UURL = MMPU.TcpSend(30002, "{}", true);
                string URL = string.IsNullOrEmpty(UURL)?"https://hiyoko.sonoj.net/dy-st/30s/6c6cb639-1d2f-4151-81c7-fd877700cf98.json": UURL;
                JArray jo = (JArray)JsonConvert.DeserializeObject(MMPU.返回网页内容(URL));
                for(int i =0;i< jo.Count;i++)
                {
                    JToken item = jo[i];
                    try
                    {
                        直播数据.Add(new 直播信息());
                        #region 实际开始时间
                        try
                        {
                            直播数据[直播数据.Count - 1].实际开始时间 = long.Parse(item["actual_start_time"].ToString());
                        }
                        catch (Exception)
                        {
                            直播数据[直播数据.Count - 1].实际开始时间 = 0;
                        }
                        #endregion
                        #region 频道ID
                        try
                        {
                            直播数据[直播数据.Count - 1].频道ID = item["ch_id"].ToString();
                        }
                        catch (Exception)
                        {
                            直播数据[直播数据.Count - 1].频道ID = "0";
                        }
                        #endregion
                        #region 频道类型
                        try
                        {
                            直播数据[直播数据.Count - 1].频道类型 = int.Parse(item["ch_type"].ToString());
                        }
                        catch (Exception)
                        {
                            直播数据[直播数据.Count - 1].频道类型 = 0;
                        }
                        #endregion
                        #region 名称
                        try
                        {
                            直播数据[直播数据.Count - 1].名称 = item["name"].ToString();
                        }
                        catch (Exception)
                        {
                            直播数据[直播数据.Count - 1].名称 = "获取失败";
                        }
                        #endregion
                        #region 已直播时长_秒
                        try
                        {
                            直播数据[直播数据.Count - 1].已直播时长_秒 = int.Parse(item["seconds"].ToString());
                        }
                        catch (Exception)
                        {
                            直播数据[直播数据.Count - 1].已直播时长_秒 = 0;
                        }
                        #endregion
                        #region 已直播时长_日本时间
                        try
                        {
                            直播数据[直播数据.Count - 1].已直播时长_日本时间 = item["start_time_str"].ToString();
                        }
                        catch (Exception)
                        {
                            直播数据[直播数据.Count - 1].已直播时长_日本时间 = "null";
                        }
                        #endregion
                        #region 主播名称
                        try
                        {
                            直播数据[直播数据.Count - 1].主播名称 = item["streamer_name"].ToString();
                        }
                        catch (Exception)
                        {
                            直播数据[直播数据.Count - 1].主播名称 = "null";
                        }
                        #endregion
                        #region 标题
                        try
                        {
                            直播数据[直播数据.Count - 1].标题 = item["title"].ToString();
                        }
                        catch (Exception)
                        {
                            直播数据[直播数据.Count - 1].标题 = "null";
                        }
                        #endregion
                        #region 当前观众
                        try
                        {
                            直播数据[直播数据.Count - 1].当前观众 = item["viewers"].ToString();
                        }
                        catch (Exception)
                        {
                            直播数据[直播数据.Count - 1].当前观众 = "null";
                        }
                        #endregion
                        #region 频道相关信息
                        try
                        {
                            直播数据[直播数据.Count - 1].频道相关信息 = item["channel_misc"].ToString();
                        }
                        catch (Exception)
                        {
                            直播数据[直播数据.Count - 1].频道相关信息 = "null";
                        }
                        #endregion
                        #region 阿B房间号
                        try
                        {
                            直播数据[直播数据.Count - 1].阿B房间号 = item["desc"].ToString();
                        }
                        catch (Exception)
                        {
                            直播数据[直播数据.Count - 1].阿B房间号 = "null";
                        }
                        #endregion

                        直播数据[直播数据.Count() - 1].直播连接 = 根据频道类型返回直播地址(直播数据[直播数据.Count() - 1]);
                    }
                    catch (Exception ex)
                    {
                        InfoLog.InfoPrintf("外部API:正在直播的列表更新出现错误"+ ex.ToString(), InfoLog.InfoClass.系统错误信息);
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
