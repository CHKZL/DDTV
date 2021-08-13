# WebSocket服务器

## 怎么启用WebSocket
在``DDTVLiveRec.xxx.config``文件中，配置以下几个项目:  
```xml
<add key="WebSocketEnable" value="1" /><!-- 是否使能WebSocket服务器 -->
<add key="WebSocketPort" value="11451" /><!-- WebSocket服务器初始化时使用的端口号 -->
<add key="WebSocketUserName" value="username" /><!-- 用于获取Token的WebSocket服务器账号 -->
<add key="WebSocketPassword" value="password" /><!-- 用于获取Token的WebSocket服务器密码 -->
```
## WebSocket的数据包封装格式
服务器对于往返的数据包都套用相同的外壳，如客户端发送给服务器的数据格式均为:  
```json
{
    "code":1001,//命令码
    "Token":"xxxxxxxxxxxxxx",//用于验证身份的Token
    "messge":""//具体的命令交互内容
}
```
::: tip 注意  
其中Messge的内容应该为Json字符串，而不是Json对象  
如:请求Token的完整命令应为:  
```json
{
    "code":2001,
    "Token":null,
    "messge":"{\"UserName\":\"defaultUserName\",\"Password\":\"defaultPassword\"}"
}
``` 
:::  

:::danger 警告   
此文档中的所有命令都只标注messge内容，外壳请自行添加  
:::  

服务器发送给客户端的外壳数据封装格式和API请求返回的信息格式一致，具体内容请参照[API请求](../API)  

## 目前会响应的请求命令码

|功能|命令码|
|--|:--:|
|请求WebSocketWToken|2001|
|获取系统运行情况|2002|
|查看当前配置文件|2003|
|检查更新|2004|
|获取系统运行日志|2005|
|获取当前录制中的队列简报 | 2006|
|获取所有下载任务的队列简报 | 2007|
|根据录制任务GUID获取任务详情 | 2008|
|根据录制任务GUID取消相应任务 | 2009|
|增加配置文件中监听的房间 | 2010|
|删除配置文件中监听的房间 | 2011|
|修改房间的自动录制开关配置 | 2012|
|获取当前房间配置列表总览 | 2013|
|获取当前录制文件夹中的所有文件的列表 | 2014|
|删除某个录制完成的文件 | 2015|
|根据房间号获得相关录制文件 | 2016|
|获取上传任务信息列表 | 2017|
|获取上传中的任务信息列表 | 2018|

## WebSocket服务器的工作流程
WebSocket功能在使用具体命令前，需向DDTV发送2001命令，以获取Token  
```json
{
    "UserName":"username",
    "Password":"password"
}
```
::: tip 提示  
其他所有的操作均和[API请求](../API)一致，区别只是WebSocket服务器公共接口为code、Token和messge，其中messge为私有属性，无其他值，私有属性定义和API一致  
:::  