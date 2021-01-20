using Auxiliary;
using DDTV_New.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Auxiliary.RoomInit;

namespace DDTV_New
{
    public class AddList
    {
        public static void 导入VTBVUP(Action<string> callback, System.Windows.Window W,bool 是否初始化)
        {
            NewThreadTask.Run(runOnLocalThread =>
            {
                try
                {
                    if (string.IsNullOrEmpty(MMPU.Cookie))
                    {
                        callback("未登录，请先登录");
                        return;
                    }
                    MMPU.加载网络房间方法.更新网络房间缓存();
                    MMPU.加载网络房间方法.是否正在缓存 = true;
                    while (MMPU.加载网络房间方法.是否正在缓存)
                    {
                        Thread.Sleep(500);
                    }
                    if(是否初始化)
                    {
                        RoomInit.RoomConfigFile = MMPU.读取exe默认配置文件("RoomConfiguration", "./RoomListConfig.json");
                        RoomInit.InitializeRoomConfigFile();
                        RoomInit.InitializeRoomList(0, false, false);
                    }
                    int 增加的数量 = 0;
                    int 已经存在的数量 = 0;
                   
                    RoomBox rlc = JsonConvert.DeserializeObject<RoomBox>(ReadConfigFile(RoomConfigFile));
                    RoomBox RB = new RoomBox
                    {
                        data = new List<RoomCadr>()
                    };
                    if (rlc.data != null)
                    {
                        foreach (var item in rlc.data)
                        {
                            RB.data.Add(item);
                        }
                    }
                    List<MMPU.加载网络房间方法.选中的网络房间> 符合条件的房间 = new List<MMPU.加载网络房间方法.选中的网络房间>();
                    JObject BB = bilibili.根据UID获取关注列表(MMPU.UID);
                    foreach (var 账号关注数据 in BB["data"])
                    {
                        foreach (var 网络房间数据 in MMPU.加载网络房间方法.列表缓存1)
                        {
                            if (账号关注数据["UID"].ToString() == 网络房间数据.UID)
                            {
                                符合条件的房间.Add(new MMPU.加载网络房间方法.选中的网络房间()
                                {
                                    UID = 网络房间数据.UID,
                                    名称 = 网络房间数据.名称,
                                    官方名称 = 网络房间数据.官方名称,
                                    平台 = 网络房间数据.平台,
                                    房间号 = 网络房间数据.roomId,
                                    编号 = 0
                                });
                                break;
                            }
                        }
                    }
                    foreach (var 符合条件的 in 符合条件的房间)
                    {
                        bool BF = false;
                        if (!string.IsNullOrEmpty(符合条件的.UID))
                        {
                            string 房间号 = string.Empty;
                            if (string.IsNullOrEmpty(符合条件的.房间号))
                            {
                                BF = true;
                                房间号 = bilibili.通过UID获取房间号(符合条件的.UID);

                                符合条件的.房间号 = 房间号;
                            }
                            else
                            {
                               房间号 = 符合条件的.房间号 ;
                            }
                            
                            bool 是否已经存在 = false;
                            foreach (var item in bilibili.RoomList)
                            {
                                if (item.房间号 == 房间号)
                                {
                                    是否已经存在 = true;
                                    break;
                                }
                            }
                            if (!是否已经存在 && !string.IsNullOrEmpty(房间号.Trim('0')))
                            {
                                增加的数量++;
                                long UIDD = 0;
                                try
                                {
                                    UIDD = long.Parse(符合条件的.UID);
                                }
                                catch (Exception){}
                                RB.data.Add(new RoomCadr { Name = 符合条件的.名称, RoomNumber = 符合条件的.房间号, Types = 符合条件的.平台, RemindStatus = false, status = false, VideoStatus = false, OfficialName = 符合条件的.官方名称, LiveStatus = false,UID= UIDD });
                            }
                            else
                            {
                                已经存在的数量++;
                            }
                        }
                        if(BF)
                        {
                            Thread.Sleep(200);
                        }
                        
                    }
                    string JOO = JsonConvert.SerializeObject(RB);
                    MMPU.储存文本(JOO, RoomConfigFile);
                    InitializeRoomList(0, false, false);

                    runOnLocalThread(() =>
                    {
                        callback("导入成功！原有:"+ 已经存在的数量 + "个，新增VTB/VUP数："+ 增加的数量);
                    });
                }
                catch (Exception E)
                {
                    ;
                }
            }, W);
        }
    }
}
