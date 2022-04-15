# Webhook

* 本Webhook会在触发特定事件的时候，会主动向配置文件中的地址发送HTTP / POST请求

## 注意事项
* 如果请求发送返回的结果不是http状态不是2xx，会判定为发送失败，失败状态最多会尝试三次，如果三次都失败，该请求会被抛弃  
* 因为本身自带网络延迟和代码处理延迟，并且每次失败都会有等待时间，所以http请求的内容时效性不能做到保证。

## 怎么启用WebHook
给配置文件中的`WebHookURL`添加上对应的地址后生效，当`WebHookURL`为空时WebHook功能关闭

::: tip  
`WebHookURL`参数为启动时加载，如果修改需要重启生效
:::

## 事件类型
```c#
        public enum HookType
        {
            /// <summary>
            /// 开播
            /// </summary>
            StartLive,
            /// <summary>
            /// 下播
            /// </summary>
            StopLive,
            /// <summary>
            /// 开始录制
            /// </summary>
            StartRec,
            /// <summary>
            /// 录制结束
            /// </summary>
            RecComplete,
            /// <summary>
            /// 录制被取消
            /// </summary>
            CancelRec,
            /// <summary>
            /// 完成转码
            /// </summary>
            TranscodingComplete,
            /// <summary>
            /// 保存弹幕文件完成
            /// </summary>
            SaveDanmuComplete,
            /// <summary>
            /// 保存SC文件完成
            /// </summary>
            SaveSCComplete,
            /// <summary>
            /// 保存礼物文件完成
            /// </summary>
            SaveGiftComplete,
            /// <summary>
            /// 保存大航海文件完成
            /// </summary>
            SaveGuardComplete,
            /// <summary>
            /// 执行Shell命令完成
            /// </summary>
            RunShellComplete,
        }
```

## 返回数据例子

```json
{
    "id":"3a3485e2-8e17-4446-ad49-8e423c16ee7f",
    "type":0,
    "uid":7855561,
    "hook_time":"2022-04-11T01:45:49.0457189+08:00",
    "user_info":{
        "name":"灬莱瓦汀",
        "face":"https://i1.hdslb.com/bfs/face/8f6e9c1a5c178b9437b8584292ef5881febe43a2.png",
        "uid":7855561,
        "sign":null,
        "attention":0
    },
    "room_Info":{
        "title":"【转播】GPU架构（试讲）",
        "description":"",
        "attention":0,
        "room_id":50443,
        "uid":7855561,
        "online":0,
        "live_time":1649612744,
        "live_status":1,
        "short_id":0,
        "area":6,
        "area_name":"生活娱乐",
        "area_v2_id":375,
        "area_v2_name":"科技科普",
        "area_v2_parent_name":"学习",
        "area_v2_parent_id":11,
        "uname":"灬莱瓦汀",
        "face":"https://i1.hdslb.com/bfs/face/8f6e9c1a5c178b9437b8584292ef5881febe43a2.png",
        "tag_name":"日常,学习,萌宠,厨艺,手机直播",
        "tags":"",
        "cover_from_user":"https://i0.hdslb.com/bfs/live/b739712a6923b96c48374567c1111a112129f3eb.jpg",
        "keyframe":"https://i0.hdslb.com/bfs/live-key-frame/keyframe012200350000000504434p17rf.jpg",
        "lock_till":"0000-00-00 00:00:00",
        "hidden_till":"0000-00-00 00:00:00",
        "broadcast_type":0,
        "need_p2p":0,
        "is_hidden":false,
        "is_locked":false,
        "is_portrait":false,
        "encrypted":false,
        "pwd_verified":false,
        "is_sp":0,
        "special_type":0,
        "roomStatus":0,
        "roundStatus":0,
        "url":"",
        "IsAutoRec":true,
        "IsRemind":true,
        "IsRecDanmu":false,
        "level":0,
        "sex":null,
        "sign":null,
        "DownloadedFileInfo":{
            "FlvFile":null,
            "Mp4File":null,
            "DanMuFile":null,
            "SCFile":null,
            "GuardFile":null,
            "GiftFile":null
        },
        "Shell":""
    }
}
```
## 返回数据说明

```C#  
        public class Message
        {
            /// <summary>
            /// WebHook请求的唯一标识
            /// </summary>
            public string id { get; set; }
            /// <summary>
            /// 类型
            /// </summary>
            public HookType type { set; get; }
            /// <summary>
            /// 用户UID
            /// </summary>
            public long uid { set; get; }
            /// <summary>
            /// 发起该WebHook请求的时间
            /// </summary>
            public DateTime hook_time { set; get; }
            /// <summary>
            /// 用户信息
            /// </summary>
            public user_info user_info { set; get; }
            /// <summary>
            /// 直播间信息
            /// </summary>
            public room_info room_Info { set; get; }
        }
        public class user_info
        {
            public string name { set; get; }
            public string face { set; get; }
            public long uid { get; set; }
            public string sign { set; get; }
            public int attention { set; get; }
        }
        public class room_info
        {
            /// <summary>
            /// 标题
            /// </summary>
            public string title { get; set; } = "";
            /// <summary>
            /// 主播简介
            /// </summary>
            public string description { get; set; } = "";
            /// <summary>
            /// 关注数
            /// </summary>
            public int attention { get; set; }
            /// <summary>
            /// 直播间房间号(直播间实际房间号)
            /// </summary>
            public int room_id { get; set; }
            /// <summary>
            /// 主播mid
            /// </summary>
            public long uid { get; set; }
            /// <summary>
            /// 直播间在线人数
            /// </summary>
            public int online { get; set; }
            /// <summary>
            /// 开播时间(未开播时为-62170012800,live_status为1时有效)
            /// </summary>
            public long live_time { get; set; }
            /// <summary>
            /// 直播状态(1为正在直播，2为轮播中)
            /// </summary>
            public int live_status { get; set; }
            /// <summary>
            /// 直播间房间号(直播间短房间号，常见于签约主播)
            /// </summary>
            public int short_id { get; set; }
            /// <summary>
            /// 直播间分区id
            /// </summary>
            public int area { get; set; }
            /// <summary>
            /// 直播间分区名
            /// </summary>
            public string area_name { get; set; } = "";
            /// <summary>
            /// 直播间新版分区id
            /// </summary>
            public int area_v2_id { get; set; }
            /// <summary>
            /// 直播间新版分区名
            /// </summary>
            public string area_v2_name { get; set; } = "";
            /// <summary>
            /// 直播间父分区名
            /// </summary>
            public string area_v2_parent_name { get; set; } = "";
            /// <summary>
            /// 直播间父分区id
            /// </summary>
            public int area_v2_parent_id { get; set; }
            /// <summary>
            /// 用户名
            /// </summary>
            public string uname { get; set; } = "";
            /// <summary>
            /// 主播头像url
            /// </summary>
            public string face { get; set; } = "";
            /// <summary>
            /// 系统tag列表(以逗号分割)
            /// </summary>
            public string tag_name { get; set; } = "";
            /// <summary>
            /// 用户自定义tag列表(以逗号分割)
            /// </summary>
            public string tags { get; set; } = "";
            /// <summary>
            /// 直播封面图
            /// </summary>
            public string cover_from_user { get; set; } = "";
            /// <summary>
            /// 直播关键帧图
            /// </summary>
            public string keyframe { get; set; } = "";
            /// <summary>
            /// 直播间封禁信息
            /// </summary>
            public string lock_till { get; set; } = "";
            /// <summary>
            /// 直播间隐藏信息
            /// </summary>
            public string hidden_till { get; set; } = "";
            /// <summary>
            /// 直播类型(0:普通直播，1：手机直播)
            /// </summary>
            public int broadcast_type { get; set; }
            /// <summary>
            /// 是否p2p
            /// </summary>
            public int need_p2p { set; get; }
            /// <summary>
            /// 是否隐藏
            /// </summary>
            public bool is_hidden { set; get; }
            /// <summary>
            /// 是否锁定
            /// </summary>
            public bool is_locked { set; get; }
            /// <summary>
            /// 是否竖屏
            /// </summary>
            public bool is_portrait { set; get; }
            /// <summary>
            /// 是否加密
            /// </summary>
            public bool encrypted { set; get; }
            /// <summary>
            /// 加密房间是否通过密码验证(encrypted=true时才有意义)
            /// </summary>
            public bool pwd_verified { set; get; }
            /// <summary>
            /// 是否为特殊直播间(0：普通直播间 1：付费直播间)
            /// </summary>
            public int is_sp { set; get; }
            /// <summary>
            /// 特殊直播间标志(0：普通直播间 1：付费直播间 2：拜年祭直播间)
            /// </summary>
            public int special_type { set; get; }
            /// <summary>
            /// 直播间状态(0:无房间 1:有房间)
            /// </summary>
            public int roomStatus { set; get; }
            /// <summary>
            /// 轮播状态(0：未轮播 1：轮播)
            /// </summary>
            public int roundStatus { set; get; }
            /// <summary>
            /// 直播间网页url
            /// </summary>
            public string url { set; get; } = "";
            /// <summary>
            /// 是否自动录制(Local值)
            /// </summary>
            public bool IsAutoRec { set; get; }
            /// <summary>
            /// 是否开播提醒(Local值)
            /// </summary>
            public bool IsRemind { set; get; }
            /// <summary>
            /// 是否录制弹幕(Local值)
            /// </summary>
            public bool IsRecDanmu { set; get; }
            /// <summary>
            /// 用户等级
            /// </summary>
            public int level { set; get; }
            /// <summary>
            /// 主播性别
            /// </summary>
            public string sex { set; get; }
            /// <summary>
            /// 主播简介
            /// </summary>
            public string sign { set; get; }
            /// <summary>
            /// 该房间最近一次完成的下载任务的文件信息
            /// </summary>
            public DownloadedFileInfo DownloadedFileInfo { set; get; } = new DownloadedFileInfo();
            /// <summary>
            /// 该房间录制完成后会执行的Shell命令
            /// </summary>
            public string Shell { set; get; } = "";
        }
```
