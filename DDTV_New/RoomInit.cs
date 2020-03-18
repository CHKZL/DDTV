using Auxiliary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_New
{
    public static class RoomInit
    {
        public static string RoomConfigFile = MMPU.getFiles("RoomConfiguration");
        
       
        public static List<RL> 房间主表 = new List<RL>();
        public static List<RL> 之前的房间主表状态 = new List<RL>();
        public static List<RL> 现在的房间主表状态 = new List<RL>();
        public static bool 首次启动 = true;
        public static int B站更新刷新次数 = 0;
        public static bool 首次更新提醒列表 = true;
        public static int 房间主表长度 = 0;
        public static int 房间信息更新次数 = 0;
        public class RL
        {
            public string 名称 { set; get; }
            public string 原名 { set; get; }
            public bool 直播状态 { set; get; }
            public string 平台 { set; get; }
            public bool 是否提醒 { set; get; }
            public bool 是否录制 { set; get; }
            public string 唯一码 { set; get; }
        }
        public static bool 根据唯一码获取直播状态(string GUID)
        {
            foreach (RL item in 房间主表)
            {
                try
                {
                    if (item.唯一码 == GUID)
                    {
                        if (item.直播状态)
                            return true;
                        else
                            return false;
                    }
                }
                catch (Exception)
                {
                }
              
            }
            return false;
        }
        public static void start()
        {
            InitializeRoomConfigFile();
            InitializeRoomList();


            Auxiliary.bilibili.start();

            Task.Run(async () =>
            {
                while (true)
                {
                    刷新B站房间列表();
                    房间信息更新次数++;
                    await Task.Delay(5 * 1000).ConfigureAwait(false);
                }
            });
        }
        private static void 刷新B站房间列表()
        {
            if (!MMPU.是否能连接阿B)
                return;
            之前的房间主表状态.Clear();
            List<RL> 临时主表 = new List<RL>();
            foreach (var item in 房间主表)
            {
                之前的房间主表状态.Add(item);
                临时主表.Add(item);
            }
            int A = 临时主表.Count();
            for (int i = 0; i < A; i++)
            {
                if (临时主表[i].平台 == "bilibili")
                {
                    临时主表.Remove(临时主表[i]);
                    i--;
                }
                A--;
            }
            foreach (var 最新的状态 in bilibili.RoomList)
            {
                foreach (var 之前的状态 in 之前的房间主表状态)
                {
                    if (B站更新刷新次数>5)
                    {
                        if (之前的状态.唯一码 == 最新的状态.房间号)
                        {
                            if (之前的状态.直播状态 == false && 最新的状态.直播状态 == true && 之前的状态.是否提醒)
                            {
                                MMPU.弹窗.Add(3000, "直播提醒", 最新的状态.名称 + "的直播状态发生了变化");
                            }

                            if (之前的状态.直播状态 == false && 最新的状态.直播状态 == true && 之前的状态.是否录制 == true)
                            {
                                MMPU.弹窗.Add(3000, "自动录制", 最新的状态.名称 + "开始直播了，开始自动录制");
                                Console.WriteLine(最新的状态.名称);
                                 Downloader.新建下载对象(之前的状态.平台, 之前的状态.唯一码, bilibili.根据房间号获取房间信息.获取标题(之前的状态.唯一码), Guid.NewGuid().ToString(), bilibili.根据房间号获取房间信息.下载地址(之前的状态.唯一码), "自动录制", true).DownIofo.备注="自动录制下载中";
                            }
                            break;
                        }
                    }
                }
                int B = 之前的房间主表状态.Count();
                临时主表.Add(new RL { 名称 = 最新的状态.名称, 唯一码 = 最新的状态.房间号, 平台 = "bilibili", 是否录制 = 最新的状态.是否录制视频, 是否提醒 = 最新的状态.是否提醒, 直播状态 = 最新的状态.直播状态, 原名 = 最新的状态.原名 });
            }
            房间主表.Clear();
            foreach (var item in 临时主表)
            {
                房间主表.Add(item);
                
            }
            房间主表长度 = 房间主表.Count();
            B站更新刷新次数++;
        }
        /// <summary>
        /// 初始化房间配置文件
        /// </summary>
        private static void InitializeRoomConfigFile()
        {
            try
            {
                ReadConfigFile(RoomConfigFile);
            }
            catch (Exception)
            {

                File.WriteAllText(RoomConfigFile, "{}");

            }
        }
        /// <summary>
        /// 读取房间配置文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string ReadConfigFile(string file)
        {
            if (!File.Exists(file))
                File.Move("./RoomListConfig.ini", file);
            return File.ReadAllText(file);
        }
        ///// <summary>
        ///// 更新房间列表
        ///// </summary>
        ///// <param name="P"></param>
        ///// <param name="GUID"></param>
        ///// <param name="A">1增加，0删除，2修改</param>
        //public static void 更新房间列表(string P, string GUID, int A)
        //{
        //    var rlc = JsonConvert.DeserializeObject<RoomBox>(ReadConfigFile(RoomConfigFile));
        //    switch (P)
        //    {
        //        case "bilibili":
        //            {

        //                switch (A)
        //                {
        //                    case 0:
        //                        {
        //                            foreach (var item in Auxiliary.bilibili.RoomList)
        //                            {
        //                                if (item.房间号 == GUID)
        //                                {
        //                                    Auxiliary.bilibili.RoomList.Remove(item);
        //                                    break;
        //                                }

        //                            }
        //                            break;
        //                        }
        //                    case 1:
        //                        {
        //                            foreach (var item in rlc.data)
        //                            {
        //                                if (item.RoomNumber == GUID)
        //                                {
        //                                    Auxiliary.bilibili.RoomList.Add(new Auxiliary.bilibili.RoomInfo
        //                                    {
        //                                        房间号 = item.RoomNumber,
        //                                        标题 = "",
        //                                        是否录制弹幕 = item.VideoStatus,
        //                                        是否录制视频 = item.VideoStatus,
        //                                        UID = "",
        //                                        直播开始时间 = "",
        //                                        名称 = item.Name,
        //                                        直播状态 = item.LiveStatus,
        //                                        原名 = item.OfficialName

        //                                    });
        //                                }                 
        //                            }
        //                            break;
        //                        }
        //                    case 2:
        //                        {
        //                            foreach (var item in Auxiliary.bilibili.RoomList)
        //                            {
        //                                if (item.房间号 == GUID)
        //                                {
        //                                    Auxiliary.bilibili.RoomList.Remove(item);
        //                                    break;
        //                                }       
        //                            }
        //                            foreach (var item in rlc.data)
        //                            {
        //                                if (item.RoomNumber == GUID)
        //                                {
        //                                    Auxiliary.bilibili.RoomList.Add(new Auxiliary.bilibili.RoomInfo
        //                                    {
        //                                        房间号 = item.RoomNumber,
        //                                        标题 = "",
        //                                        是否录制弹幕 = item.VideoStatus,
        //                                        是否录制视频 = item.VideoStatus,
        //                                        UID = "",
        //                                        直播开始时间 = "",
        //                                        名称 = item.Name,
        //                                        直播状态 = false,
        //                                        原名 = item.OfficialName

        //                                    });
        //                                }
        //                            }
        //                            break;
        //                        }


        //                }

        //                break;
        //            }
        //    }
        //}

        public static bool 初始化储存房间储存一次 = true;
        /// <summary>
        /// 初始化房间列表
        /// </summary>
        public static void InitializeRoomList()
        {
            var rlc = new RoomBox();
            rlc = JsonConvert.DeserializeObject<RoomBox>(ReadConfigFile(RoomConfigFile));
            List<RoomCadr> RoomConfigList = new List<RoomCadr>();//房间信息1List
            RoomConfigList = rlc?.data;
            if (RoomConfigList == null)
                RoomConfigList = new List<RoomCadr>();
            bilibili.RoomList.Clear();
            if(初始化储存房间储存一次)
            {
                string JOO = JsonConvert.SerializeObject(rlc);
                MMPU.储存文本(JOO, RoomConfigFile);
                初始化储存房间储存一次 = false;
            }

            foreach (var item in RoomConfigList)
            {
                if (item.Types == "bilibili")
                {

                    bilibili.RoomList.Add(new bilibili.RoomInfo
                    {
                        房间号 = item.RoomNumber,
                        标题 = "",
                        是否录制弹幕 = item.VideoStatus,
                        是否录制视频 = item.VideoStatus,
                        UID = "",
                        直播开始时间 = "",
                        名称 = item.Name,
                        直播状态 = item.LiveStatus,
                        原名 = item.OfficialName,
                        是否提醒=item.RemindStatus
                    });
                    if (首次启动)
                    {
                        bilibili.RoomList[bilibili.RoomList.Count - 1].直播状态 = false;
                    }
                }
            }
            首次启动 = false;
        }
        public class RoomBox
        {
            public List<RoomCadr> data { get; set; }
        }

        public class RoomCadr
        {
            public string Name { get; set; }
            public string OfficialName { set; get; } = "";
            public string RoomNumber { get; set; }
            public string Types { get; set; } = "NU";
            public bool status { get; set; } = false;
            public bool VideoStatus { get; set; } = false;
            public bool RemindStatus { get; set; } = true;
            public bool LiveStatus { get; set; } = false;

        }
    }
}
