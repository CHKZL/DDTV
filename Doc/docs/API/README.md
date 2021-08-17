# DDTV API Doc



## 请求说明
### 流程图如下:
<img :src="$withBase('/API.png')" alt="mixureSecure">

### 例:/api/room_delete接口
>- Request:
>```text
>method: POST
>path: http://127.0.0.1:11419/api/room_delete
>```
>```json
>"form-data":
>{
>    "time":2345678,
>    "cmd":"room_delete",
>    "sig":"2B96810325EB0FE263A91FAA71592033377DF543",
>    "ver":2,
>    "RoomId":21706862
>}
>```
>- Response:
>```json
>{
>    "result": true,
>    "code":0,
>    "messge": "成功",
>    "queue": 1,
>    "Package": [
>        {
>            "result": true,
>            "messge": "删除完成"
>        }
>    ]
>}
>```
* Request内容解释：  
计算sig所使用的加密字符串是把所有请求参数拼接，并且加上token值过后，对key进行排序后拼接得到加密字符串  
【cmd=room_delete&RoomId=21706862&time=2345678&token=1145141919810A&ver=2】  
对该字符串进行SHA1加密全部转换成大写字母后便得到sig值  
【2B96810325EB0FE263A91FAA71592033377DF543】 
### 注意事项:
>加密字符串拼接时公共变量sig不参与  
在拼接加密字符串时应加上变量token，但应当注意，token不应当随请求一起提交，仅在本地参与  
加密字符串拼接，键值对之间用&分割(如："code=system_info&time=1234567")  
加密中编码以UTF-8为准  
加密得到的sig在发送时转换成大写  

## 返回内容格式

|键名|格式|是否会为Null|解释|
|:--:|:--:|:--:|--|
|result|int|否|鉴权的结果|
|code|int|否|请求返回的状态码(具体内容参照[返回结果状态码列表]()|
|messge|string|否|关于请求结果的说明文字内容|
|queue|int|否|Package的List长度|
|Package|List\<T>|是|返回的接口具体内容，如果鉴权失败或者本身列表为空则是Null|


## 公共变量

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|time|int|是|请求发出时的UTC时间戳(注意是UTC时间不需要+8小时)，单位为秒，和服务器时间300秒以内的为有效请求(如:1626508097)|
|cmd|string|是|请求的API接口的接口名称(如:system_info)|
|sig|string|是|其他变量排序后按照规则拼接过后使用SHA1散列后得到的签名|
|ver|int|是|接口的版本类型，当前只开放2。(1为DDTV服务端内部使用的WEB接口，规则不同)|


## 已实现的通用API列表
|方式|名称|返回内容|解释|私有变量数量|
|:--:|:--:|:--:|:--:|:--:|
|POST|system_info|JSON|[获取系统运行情况](./#post-api-system-info)|0|
|POST|system_config|JSON|[查看当前配置文件](./#post-api-system-config)|0|
|POST|system_resource_monitoring|JSON|[查看当前硬件资源使用情况](./#post-api-system-resource-monitoring)|0|
|POST|system_update|JSON|[检查更新](./##post-api-system-update)|0|
|POST|system_log|JSON|[获取系统运行日志](./#post-api-system-log)|0|
|POST|rec_processing_list|JSON|[获取当前录制中的队列简报](./#post-api-rec-processing-list)|0|
|POST|rec_all_list|JSON|[获取所有下载任务的队列简报](./#post-api-rec-all-list)|0|
|POST|rec_info|JSON|[根据录制任务GUID获取任务详情](./#post-api-rec-info)|1|
|POST|rec_cancel|JSON|[根据录制任务GUID取消相应任务](./#post-api-rec-cancel)|2|
|POST|room_add|JSON|[增加配置文件中监听的房间](./#post-api-room-add)|4|
|POST|room_delete|JSON|[删除配置文件中监听的房间](./#post-api-room-delete)|1|
|POST|room_status|JSON|[修改房间的自动录制开关配置](./#post-api-room-status)|2|
|POST|room_list|JSON|[获取当前房间配置列表总览](./#post-api-room-list)|0|
|POST|file_lists|JSON|[获取当前录制文件夹中的所有文件的列表](./#post-api-file-lists)|0|
|POST|file_delete|JSON|[删除某个录制完成的文件](./#post-api-file-delete)|3|
|POST|file_range|JSON|[根据房间号获得相关录制文件](./#post-api-file-range)|1|
|POST|upload_list|JSON|[获取全部历史上传任务信息列表](./#post-api-upload-list)|0|
|POST|upload_ing|JSON|[获取上传中的任务信息列表](./#post-api-upload-ing)|0|
|POST|config_auto_transcoding|JSON|[修改自动转码设置](./#post-api-config-auto-transcoding)|1|

## 已实现的特殊API接口
|方式|名称|返回内容|解释|私有变量数量|
|:--:|:--:|:--:|:--:|:--:|
|GET|file_steam|Flie|[获取播放文件](./#get-file-path)|2|

## 返回结果状态码列表
|值|含义|
|:--:|:--:|
|-1|请求错误|
|1001|请求成功|
|1002|鉴权失败|


## 通用接口详情

### `POST /api/system_info`

::: details 获取系统运行情况

- 私有变量  

无

- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/system_info
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"system_info",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 1,//请求返回的Package包含有多少个子项目
    "Package": [
        {
            "DDTVCore_Ver": "2.0.5.1d",//当前DDTV核心版本号
            "Room_Quantity": 1,//当前配置文件中的房间数量
            "ServerName":"DDTVServer",//配置文件中该服务器的名称(适用于多台服务器的人区分)
            "ServerAID":"A10D218F-79CD-94E5-0580-666CF6A179CC",//配置文件中该服务器的全局唯一编号(适用于多台服务器的人区分)
            "ServerGroup":"default",//配置文件中该服务器的分组(适用于多台服务器的人区分)
            "os_Info": {//
                "OS_Ver": "Linux 5.4.0-74-generic #83-Ubuntu SMP Sat May 8 02:35:39 UTC 2021",//系统版本
                "OS_Tpye": "X64",//系统架构
                "Memory_Usage": 155086848,//DDTV已使用的内存
                "Runtime_Ver": "X64",//DDTV运行时架构
                "UserInteractive": true,//是否为交互模式
                "Associated_Users": "ddtv",//关联执行的用户名
                "Current_Directory": "\\home\\ubuntu\\ddtv_S",//当前DDTV的运行目录
                "AppCore_Ver": ".NET 5.0.8",//.NET Runtime版本号
                "WebCore_Ver": "5.0.8"//WEB服务的ASP.NET 版本号
            },
            "Download_Info": {
                "Downloading": 0,//下载中的任务数量
                "Completed_Downloads": 1//下载完成的数量
            },
            "Ver_Info": {
                "IsNewVer": false,//是否有新版本
                "NewVer":null,//如果有新版本，那新版本是多少
                "Update_Log": null//如果有新版本，新版本的版本说明内容
            }
        }
    ]
}
```
:::

### `POST /api/system_config`
::: details 查看当前配置文件
- 私有变量  

无
- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/system_config
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"system_config",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 60,//请求返回的Package包含有多少个子项目
    "Package": [
        {
            "Key": "file",//键名
            "Value": "./tmp/"//值
        },
        {
            "Key": "Livefile",
            "Value": "./tmp/LiveCache/"
        },
        {
            "Key": "DANMU",
            "Value": "1"
        },
        {
            "Key": "PlayWindowHeight",
            "Value": "440"
        },
        {
            "Key": "PlayWindowWidth",
            "Value": "720"
        },
        {
            "Key": "YouTubeResolution",
            "Value": "640x360"
        },
        {
            "Key": "RoomConfiguration",
            "Value": "./RoomListConfig.json"
        },
        .....................
    ]
}
```
:::


### `POST /api/system_resource_monitoring`
::: details 查看当前硬件资源使用情况
- 私有变量  

无
- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/system_resource_monitoring
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"system_resource_monitoring",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 1,//请求返回的Package包含有多少个子项目
    "Package": [
        {
            "Platform":"Linux",//系统平台
            "DDTV_use_memory":184549376,//DDTV使用的内存量
            "CPU_usage":2.72,//CPU使用率(百分比)
            "Available_memory":18876381265,//剩余的物理内存大小
            "Total_memory":18876381265,//总共物理内存大小
            "HDDInfo":[//磁盘列表
                {
                    "FileSystem":"/dev/sda1",//物理路径
                    "Size":"7.8T",//总大小
                    "Used":"3.6T",//已使用大小
                    "Avail":"4.2T",//剩余大小
                    "Usage":"45%",//使用率
                    "MountPath":"/user/ddtv/test/tmp/"//挂载路径
                }
            ]
        }
    ]
}
```
:::


### `POST /api/system_update`
::: details 检查有无更新
- 私有变量  

无
- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/system_update
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"system_update",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 1,//请求返回的Package包含有多少个子项目
    "Package": [
       {
           "IsNewVer":false,//是否有新版本
           "NewVer":null,//如果有新版本，那新版本是多少
           "Update_Log":null//如果有新版本，新版本的版本说明内容
       }
    ]
}
```
:::

### `POST /api/system_log`
::: details 获取系统运行日志
- 私有变量  

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|count|int|否|获取最近的多少条日志，取值为1-int32，如果小于0则默认返回所有log|


- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/system_log
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"system_log",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2,
    "count":1
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 1,//请求返回的Package包含有多少个子项目
    "Package": [
       {
           "Time":2021-07-27 14:14:15,//日志产生的时间
           "Type":1,//日志类型枚举
           "LogMsg":"log测试信息1，这是一条系统错误信息信息"//日志内容
       },
       {
           "Time":2021-07-27 14:14:15,
           "Type":0,
           "LogMsg":"log测试信息2，这是一条DEbug消息"
       }
    ]
}
```
* logType枚举信息
```C#
public enum InfoClass
        {
            Debug = 0,
            系统错误信息 = 1,
            进程一般信息 = 2,
            下载系统信息 = 3,
            系统一般信息 = 4,
            上传系统信息 = 5,
            系统强制信息=6,
        }
```
:::

### `POST /api/rec_processing_list`
::: details 获取当前录制中的队列简报
- 私有变量  

无
- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/rec_processing_list
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"rec_processing_list",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 1,//请求返回的Package包含有多少个子项目
    "Package": [
        {
            "RoomId": "21706862",//房间号
            "Downloaded_bit": 1346472247,//已下载的字节数
            "Downloaded_str": "1.254G",//转换单位的已下载的数据大小
            "Name": "七咔拉CHikalar",//主播名称
            "StartTime": 1626783169,//开始时间(UTC时间戳)
            "EndTime": 0,//结束时间，如果未结束就是0
            "GUID": "fb7f502e-64ff-411b-9d55-d9f974751616",//任务ID
            "State":true,//下载状态
            "Remark":"自动录制中"//任务状态说明
        }
    ]
}
```
:::
### `POST /api/rec_all_list`
::: details 获取所有下载任务的队列简报
- 私有变量  

无
- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/rec_all_list
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"rec_all_list",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 1,//请求返回的Package包含有多少个子项目
    "Package": [
        {
            "RoomId": "21706862",//房间号
            "Downloaded_bit": 1346472247,//已下载的字节数
            "Downloaded_str": "1.254G",//转换单位的已下载的数据大小
            "Name": "七咔拉CHikalar",//主播名称
            "StartTime": 1626783169,//开始时间(UTC时间戳)
            "EndTime": 0,//结束时间，如果未结束就是0
            "GUID": "fb7f502e-64ff-411b-9d55-d9f974751616",//任务ID
            "State":true,//下载状态
            "Remark":"自动录制中"//任务状态说明
        }
    ]
}
```
:::

### `POST /api/rec_info`
::: details 根据录制任务GUID获取任务详情
- 私有变量  

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|GUID|string|是|任务的GUID值，可通过rec_processing_list和rec_all_list接口获得|

- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/rec_info
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"rec_info",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2,
    "GUID":"a38c4f1c-18a8-454c-ad22-dc1deb612c12"
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 1,//请求返回的Package包含有多少个子项目
    "Package": [
        {
            "阿B直播流对象": {
                "m_innerRts": null,
                "startIn": false,
                "wss_S": ""
            },
            "弹幕储存流": null,
            "礼物储存流": null,
            "弹幕录制基准时间": "2021-07-20T11:47:45.5530392+08:00",
            "WC": null,
            "下载状态": true,
            "最后连接时间": 1626781716,
            "网络超时": false,
            "已下载大小bit": 39291289.0,
            "已下载大小str": "37.47MB",
            "文件保存路径": "./tmp/bilibili_七咔拉CHikalar_21706862/【早安还债】土豆子还债中_20210720114745552.flv",
            "事件GUID": "a38c4f1c-18a8-454c-ad22-dc1deb612c12",
            "备注": "自动录制",
            "开始时间": 1626781671,
            "结束时间": 0,
            "房间_频道号": "21706862",
            "平台": "bilibili",
            "是否保存": true,
            "下载地址": "https://d1--cn-gotcha04.bilivideo.com/live-bvc/969914/live_484660274_78967043.flv?cdn=cn-gotcha04&expires=1626756463&len=0&oi=605583931&pt=web&qn=10000&trid=1000319df535301044089db508e00ea733ba&sigparams=cdn,expires,len,oi,pt,qn,trid&sign=e3dc89e76b8d8ff58db1fee4c7abd83a&ptype=0&src=9&sl=2&sk=417e709c171a500&order=1",
            "标题": "【早安还债】土豆子还债中",
            "播放状态": false,
            "是否是播放任务": false,
            "重连文件路径": null,
            "主播名称": "七咔拉CHikalar",
            "继承": {
                "是否为继承对象": false,
                "待合并文件列表": [],
                "继承的下载文件路径": null,
                "合并后的文件路径": null
            },
            "是否是固定视频": false
        }
    ]
}
```
:::
### `POST /api/rec_cancel`
::: details 根据录制任务GUID取消相应任务
- 私有变量  

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|GUID|string|是|任务的GUID值，可通过rec_processing_list和rec_all_list接口获得|

- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/rec_cancel
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"rec_cancel",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2,
    "GUID":"fb7f502e-64ff-411b-9d55-d9f974751616"
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 1,//请求返回的Package包含有多少个子项目
    "Package": [
        {
            "阿B直播流对象": {
                "m_innerRts": null,
                "startIn": false,
                "wss_S": ""
            },
            "弹幕储存流": null,
            "礼物储存流": null,
            "弹幕录制基准时间": "2021-07-20T12:12:48.9372528+08:00",
            "WC": null,
            "下载状态": false,
            "最后连接时间": 1626783195,
            "网络超时": false,
            "已下载大小bit": 8644969.0,
            "已下载大小str": "8.24MB",
            "文件保存路径": "./tmp/bilibili_七咔拉CHikalar_21706862/【早安还债】土豆子还债中_20210720121248923.flv",
            "事件GUID": "fb7f502e-64ff-411b-9d55-d9f974751616",
            "备注": "用户取消下载",
            "开始时间": 1626783175,
            "结束时间": 1626783195,
            "房间_频道号": "21706862",
            "平台": "bilibili",
            "是否保存": true,
            "下载地址": "https://d1--cn-gotcha04.bilivideo.com/live-bvc/479757/live_484660274_78967043.flv?cdn=cn-gotcha04&expires=1626757967&len=0&oi=605583931&pt=web&qn=10000&trid=1000b282f342323a46f7929767548ec2afa4&sigparams=cdn,expires,len,oi,pt,qn,trid&sign=955cc6c61a565b456c747880e031f33c&ptype=0&src=9&sl=2&sk=417e709c171a500&order=1",
            "标题": "【早安还债】土豆子还债中",
            "播放状态": false,
            "是否是播放任务": false,
            "重连文件路径": null,
            "主播名称": "七咔拉CHikalar",
            "继承": {
                "是否为继承对象": false,
                "待合并文件列表": [],
                "继承的下载文件路径": null,
                "合并后的文件路径": null
            },
            "是否是固定视频": false
        }
    ]
}
```
:::
### `POST /api/room_add`
::: details 增加配置文件中监听的房间
- 私有变量  

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|RoomId|int|是|房间号|
|Name|string|是|本地化(翻译)的名字|
|OfficialName|string|是|官方原文名称|
|RecStatus|bool|是|是否打开开播自动录制|

- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/room_add
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"room_add",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2,
    "RoomId":21706862,
    "Name":"七咔拉CHikalar",
    "OfficialName":"七咔拉CHikalar",
    "RecStatus":true,
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 1,//请求返回的Package包含有多少个子项目
    "Package": [
        {
            "result": true,//请求是否完成
            "messge": "七咔拉CHikalar[21706862]添加完成"//信息
        }
    ]
}
```
:::

### `POST /api/room_delete`
::: details 删除配置文件中监听的房间
- 私有变量  

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|RoomId|int|是|配置文件中的房间号|
- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/room_delete
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"room_delete",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2,
    "RoomId":21706862
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 1,//请求返回的Package包含有多少个子项目
    "Package": [
        {
            "result": true,//请求是否完成
            "messge": "删除完成"//信息
        }
    ]
}
```
:::

### `POST /api/room_status`
::: details 修改房间的自动录制开关配置
- 私有变量  

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|RoomId|int|是|房间号|
|RecStatus|bool|是|是否打开开播自动录制|
- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/room_status
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"room_status",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2,
    "RoomId":21706862,
    "RecStatus":false
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 1,//请求返回的Package包含有多少个子项目
    "Package": [
        {
            "result": true,//请求是否完成
            "messge": "修改设置完成"//信息
        }
    ]
}
```

:::
### `POST /api/room_list`
::: details 获取当前房间配置列表总览
- 私有变量  

无  

- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/room_list
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"room_list",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2,
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 1,//请求返回的Package包含有多少个子项目
    "Package": [
        {
            "名称": "七咔拉CHikalar",
            "原名": "七咔拉CHikalar",
            "直播状态": false,
            "平台": "bilibili",
            "是否提醒": false,
            "是否录制": false,
            "唯一码": "21706862"
        }
    ]
}
```
:::

### `POST /api/file_lists`
::: details 获取当前录制文件夹中的所有文件的列表
- 私有变量  

无  

- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/file_lists
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"file_lists",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2,
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 3,//请求返回的Package包含有多少个子项目
    "Package": [
        {
            "Size": 151728128,//文件大小
            "Name": "【早安还债】土豆子还债中_20210720111734878.flv",//文件名
            "Directory": "bilibili_七咔拉CHikalar_21706862",//文件所属目录
            "Path": "./tmp/bilibili_七咔拉CHikalar_21706862/【早安还债】土豆子还债中_20210720111734878.flv",//文件完整相对路径
            "ModifiedTime": "2021-07-20T11:17:40.0501024+08:00"//文件最后修改时间
        },
        {
            "Size": 13717504,
            "Name": "【早安还债】土豆子还债中_20210720112241897.flv",
            "Directory": "bilibili_七咔拉CHikalar_21706862",
            "Path": "./tmp/bilibili_七咔拉CHikalar_21706862/【早安还债】土豆子还债中_20210720112241897.flv",
            "ModifiedTime": "2021-07-20T11:22:47.150608+08:00"
        },
        {
            "Size": 6270976,
            "Name": "测试直播_20210720112652317.flv",
            "Directory": "bilibili_测试房间_123456",
            "Path": "./tmp/bilibili_测试房间_123456/测试直播_20210720112652317.flv",
            "ModifiedTime": "2021-07-20T11:26:57.770125+08:00"
        }
    ]
}
```
:::

### `POST /api/file_delete`
::: details 删除某个录制完成的文件
- 私有变量  

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|Directory|string|是|所在的文件夹名称|
|Name|string|是|要删除的文件名|
- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/file_delete
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"file_delete",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2,
    "Directory":"bilibili_七咔拉CHikalar_21706862",
    "Name":"【早安还债】土豆子还债中_20210720112652317.flv",
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 1,//请求返回的Package包含有多少个子项目
    "Package": [
        {
            "result": true,//请求是否完成
            "messge": "文件已提加入删除委托列表，等待文件锁解锁后自动删除"//信息
        }
    ]
}
```
:::

### `POST /api/file_range`
::: details 根据房间号获得相关录制文件
- 私有变量  

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|RoomId|int|是|房间号|

- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/file_range
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"file_range",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2,
    "RoomId":"21706862"
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 3,//请求返回的Package包含有多少个子项目
    "Package": [
        {
            "Size": 151728128,//文件大小
            "Name": "【早安还债】土豆子还债中_20210720111734878.flv",//文件名
            "Directory": "bilibili_七咔拉CHikalar_21706862",//文件所属目录
            "Path": "./tmp/bilibili_七咔拉CHikalar_21706862/【早安还债】土豆子还债中_20210720111734878.flv",//文件完整相对路径
            "ModifiedTime": "2021-07-20T11:17:40.0501024+08:00"//文件最后修改时间
        },
        {
            "Size": 13717504,
            "Name": "【早安还债】土豆子还债中_20210720112241897.flv",
            "Directory": "bilibili_七咔拉CHikalar_21706862",
            "Path": "./tmp/bilibili_七咔拉CHikalar_21706862/【早安还债】土豆子还债中_20210720112241897.flv",
            "ModifiedTime": "2021-07-20T11:22:47.150608+08:00"
        }
    ]
}
```
:::

### `POST /api/upload_list`
::: details 获取全部历史上传任务信息列表

- 私有变量  

无

- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/upload_list
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"upload_list",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2
}
```
- Response:*该内容仅供参考，可能有变动*
```json
{
    "result":true,
    "code":0,
    "messge":"成功",
    "queue":1,
    "Package":[
        {
            "streamerName":"蒂蒂媞薇",
            "streamTitle":"测试直播",
            "fileName":"这是文件名",
            "fileSize":46548135574,
            "srcFile":"这是待上传的文件路径",
            "remotePath":"这是上传目标的路径",
            "type":"这是上传的目标平台",
            "retries":0,
            "status":{
                "这是目标平台1":{
                    "startTime":1,
                    "endTime":2,
                    "statusCode":0,
                    "progress":1,//上传进度1-100，-1为获取失败
                    "comments":"其他信息，比如上传进度"
                },"这是目标平台2":{
                    "startTime":1,
                    "endTime":2,
                    "statusCode":0,
                    "progress":1,//上传进度1-100，-1为获取失败
                    "comments":"其他信息，比如上传进度"
                }
            }
        }
    ]
}
```
:::

::: details 上传任务的Package数据结构
```C#
/// <summary>
/// 每个文件的上传信息
/// </summary>
public class UploadInfo
{
    // <summary>
    /// 主播名
    /// </summary>
    public string streamerName { get; }
    /// <summary>
    /// 直播标题
    /// </summary>
    public string streamTitle { get; }
    /// <summary>
    /// 文件名
    /// </summary>
    public string fileName { get; }
    /// <summary>
    /// 文件大小
    /// </summary>
    public double fileSize { get; }

    /// <summary>
    /// 待上传文件路径
    /// </summary>
    public string srcFile { get; }
    /// <summary>
    /// 上传目标路径
    /// </summary>
    public string remotePath { get; }
    /// <summary>
    /// 上传目标
    /// </summary>
    public string type { set; get; }
    /// <summary>
    /// 已重试次数
    /// </summary>
    public int retries { set; get; }

    /// <summary>
    /// 存放每个上传任务的上传状态
    /// </summary>
    public Dictionary<string, UploadStatus> status { set; get; } = new Dictionary<string, UploadStatus>();

    /// <summary>
    /// 每个任务的上传状态
    /// </summary>
    public class UploadStatus
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public int startTime { set; get; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public int endTime { set; get; }
        /// <summary>
        /// 上传状态
        /// <para>-1：上传失败 0:上传成功 其他：上传次数</para>
        /// </summary>
        public int statusCode { set; get; }
        /// <summary>
        /// 上传进度
        /// <para>百分比，取值1-100%，返回-1则为失败</para>
        /// </summary>
        public int progress { set; get; }
        /// <summary>
        /// 其他信息
        /// <para>用于web端展示上传进度</para>
        /// </summary>
        public string comments { set; get; }
    }
}
```
:::

### `POST /api/upload_ing`
::: details 获取上传中的任务信息列表

- 私有变量  

无

- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/upload_ing
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"upload_ing",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2
}
```
- Response:*该内容仅供参考，可能有变动*
```json
{
    "result":true,
    "code":0,
    "messge":"成功",
    "queue":1,
    "Package":[
        {
            "streamerName":"蒂蒂媞薇",
            "streamTitle":"测试直播",
            "fileName":"这是文件名",
            "fileSize":46548135574,
            "srcFile":"这是待上传的文件路径",
            "remotePath":"这是上传目标的路径",
            "type":"这是上传的目标平台",
            "retries":0,
            "status":{
                "这是目标平台1":{
                    "startTime":1,
                    "endTime":2,
                    "statusCode":0,
                    "progress":1,//上传进度1-100，-1为获取失败
                    "comments":"其他信息，比如上传进度"
                },"这是目标平台2":{
                    "startTime":1,
                    "endTime":2,
                    "statusCode":0,
                    "progress":1,//上传进度1-100，-1为获取失败
                    "comments":"其他信息，比如上传进度"
                }
            }
        }
    ]
}
```
:::

### `POST /api/config_auto_transcoding`
::: details 修改自动转码设置
- 私有变量  

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|transcoding_status|bool|是|是否打开自动转码|
- Request:
```text
method: POST
path: http://127.0.0.1:11419/api/config_auto_transcoding
```
```json
"form-data":
{
    "time":2345678,
    "cmd":"config_auto_transcoding",
    "sig":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "ver":2,
    "transcoding_status":true
}
```
- Response:
```json
{
    "result": true,//鉴权是否通过
    "code":0,//请求状态码，请按返回结果状态码列表处理
    "messge": "成功",//请求返回的备注信息
    "queue": 1,//请求返回的Package包含有多少个子项目
    "Package": [
        {
            "result": true,
            "messge": "修改转码设置成功，当前转码使能为true"
        }
    ]
}
```
:::warning 自动转码依赖  
使用该接口续系统环境中存在对应的转码依赖程序，具体内容请参考[进阶功能说明-自动转码](../AdvancedFeatures/自动转码.html)里的说明  
:::

## 特殊接口详情

### `GET /$file_path$`
::: details 获取播放文件
- 私有变量  

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|Directory|string|是|录制文件夹名称|
|File|string|是|媒体文件名称|

- Request:
```text
method: GET
path: http://127.0.0.1:11419/tmp/bilibili_蒂蒂媞薇_21446992/测试直播.mp4?time=2345678&cmd=file_steam&sig=xxxxxxxxxxx&var=2&Directory=bilibili_蒂蒂媞薇_21446992&File=测试直播.mp4
```
- Response:
```C#
File
```
:::