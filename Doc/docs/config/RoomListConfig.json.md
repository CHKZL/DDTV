# 房间配置文件

## 房间配置文件的格式介绍

默认房间文件`RoomListConfig.json`格式为json字符串，默认为空json，并且`DDTVLiveRec`和`DDTV`通用  
`DDTVLiveRec`和`DDTV`的房间配置文件可以互相复制通用  
如果是`DDTVLiveRec`，在下载的压缩包里附带了一个参考的文件，也可以参考那个文件进行手动编写  
完整的房间配置文件是由多个房间配置组合成的，完整的文件格式如下  
房间配置文件完整格式为
```
{
    "data": [
        {
            "Name": "绯赤艾莉欧",
            "OfficialName": "緋赤エリオ",
            "RoomNumber": "21396545",
            "Types": "bilibili",
            "status": false,
            "VideoStatus": false,
            "RemindStatus": false,
            "LiveStatus": false
        },
        {
            "Name": "奥斯曼人谢谢你",
            "OfficialName": "夢乃栞",
            "RoomNumber": "14052636",
            "Types": "bilibili",
            "status": false,
            "VideoStatus": false,
            "RemindStatus": false,
            "LiveStatus": false
        }
    ]
}
```
### 房间配置文件中，单个房间配置信息说明 
```json
{
    "Name": "奥斯曼人谢谢你",//为翻译名称
    "OfficialName": "夢乃栞",//为官方名称
    "RoomNumber": "14052636",//为B站直播的房间号
    "Types": "bilibili",//为多平台支持预留，现在默认并必须为"bilibili"
    "status": false,//为是否开播自动录制的标识，false为检测到开播后不录制，true为检测到开播后自动录制
    "VideoStatus": false,//为DDTV开播弹窗提醒标识，false为检测到开播后系统气泡提醒，true为检测到开播后不提醒
    "RemindStatus": false,//为房间状态缓存信息，默认为false，不用手动修改
    "LiveStatus": false//为房间状态缓存信息，默认为false，不用手动修改
}
```

:::danger 警告 
手动编辑过后请检查JSON字符串的合法性，请保证确保符合参考文件的JSON文件格式！！！
::: 