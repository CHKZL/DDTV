# 房间配置文件

## 房间配置文件的格式介绍

默认房间文件`RoomListConfig.json`格式为json字符串，默认为空json，并且`DDTV_GUI`和`DDTV_WEB_Server`通用  
`DDTV_GUI`和`DDTV_WEB_Server`的房间配置文件可以互相复制通用  
完整的房间配置文件是由多个房间配置组合成的，完整的文件格式如下  
房间配置文件完整格式为
```json
{
    "data": [
        {
            "name": "未来明-MiraiAkari",
            "Description": "",
            "RoomId": 6792401,
            "UID": 238537745,
            "IsAutoRec": false,
            "IsRemind": false,
            "IsRecDanmu": false,
            "Like": false,
            "Shell": ""
        },
        {
            "name": "AIChannel官方",
            "Description": "",
            "RoomId": 1485080,
            "UID": 1473830,
            "IsAutoRec": false,
            "IsRemind": false,
            "IsRecDanmu": false,
            "Like": false,
            "Shell": ""
        }
    ]
}
```
### 房间配置文件中，单个房间配置信息说明 
```json
{
            "name": "未来明-MiraiAkari",//昵称
            "Description": "",//备注
            "RoomId": 6792401,//房间号
            "UID": 238537745,//账号UID
            "IsAutoRec": false,//开播后是否自动录制
            "IsRemind": false,//开播后是否提醒(DDTV_GUI特有，在WEB中无效)
            "IsRecDanmu": false,//是否录制该房间弹幕(需要打开总弹幕录制开关)
            "Like": false,///特别标注(功能未开发完成，当前无效)
            "Shell": ""//该房间录制完成后会执行的Shell命令，详细内容请查看高级功能中详细说明(高级功能，如果不了解请勿随意填写！)
}
```

:::danger 警告 
手动编辑过后请检查JSON字符串的合法性，请保证确保符合参考文件的JSON文件格式！！！
::: 