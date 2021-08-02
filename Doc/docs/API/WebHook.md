# Webhook
* 本Webhook会在触发特定事件的时候，会主动向配置文件中的地址发送HTTP / POST请求
## 注意事项
* 如果请求发送返回的结果不是http状态不是2xx，会判定为发送失败，失败状态最多会尝试三次，如果三次都失败，该请求会被抛弃  
* 因为本身自带网络延迟和代码处理延迟，并且每次失败都会等待5秒，所以http请求的内容时效性不能做到保证。
## 现有功能
|cmd|解释|
|:--:|:--:|
|LIVE|开始录制|
|PREPARING|结束录制|
## 功能详情
### LIVE
::: details 获取系统运行情况
>- Request:
>```text
>POST 配置文件中设置的地址
>method: POST
>ContentType: application/json; charset=UTF-8
>UserAgent: DDTVCore/2.0.6.1Dev
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
:::