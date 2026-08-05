# DDTV_Client（Windows）

:::warning 注意
DDTV_Client仅支持Windows平台运行，其他平台需要录制请使用DDTV_Server
:::

## 1.下载
从以下地方选一个下载DDTV最新版本  
QQ群共享(其实我推荐这个(这里的人超好的，还能直接对线(((  
DDTV功能反馈讨论群:`338182356`  
[GitHub](https://github.com/CHKZL/DDTV/releases/latest)   
DDTV聊天吹水群:`522865400`  

请下载`DDTV-Client-windows-latest-win-x64-release[版本号].zip`。

## 2.安装
DDTV_Client是免安装的，且为**自包含发布**（无需安装.NET运行时），把下载下来的压缩包解压到任意当前用户有**读写权限**的路径即可   

### WebView2运行时依赖
DDTV_Client的窗口通过WebView2承载WEBUI页面，需要系统中安装有**Microsoft Edge WebView2 Runtime**。  
Windows 11系统一般已自带；Windows 10或精简版系统如果没有，请到微软官方页面下载安装（下载`Evergreen Bootstrapper`在线安装即可）：  
[https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/](https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/)  

### 关于ffmpeg
Windows版的DDTV_Client压缩包中已经内置了ffmpeg（位于`bin/Plugins/ffmpeg/ffmpeg.exe`），无需另行安装。

## 3.启动准备
### 配置房间文件
默认房间文件`./Config/RoomListConfig.json`（相对于程序目录`bin/`）格式为json字符串，默认为空json     
可以直接使用其他版本DDTV的房间配置文件复制过来即可  
完整格式可参考配置文件说明中关于房间文件的说明  
房间配置文件格式为  
```json
{
            "name": "未来明-MiraiAkari",//昵称
            "Description": "",//备注
            "RoomId": 6792401,//房间号(长号)
            "UID": 238537745,//主播账号UID
            "IsAutoRec": false,//开播后是否自动录制
            "IsRemind": false,//开播后是否提醒
            "IsRecDanmu": false,//是否录制该房间弹幕(需要打开总弹幕录制开关)
            "Like": false,//特别标注
            "Shell": "",//该房间录制完成后执行的Shell命令，留空不执行
            "AppointmentRecord": false,//是否预约下一次录制
            "RoomCutAccordingToSize": 0,//只针对本房间生效的按文件大小切割(字节)，0为不生效
            "RoomCutAccordingToTime": 0//只针对本房间生效的按录制时长切割(秒)，0为不生效
},
```
多个这种格式的内容组成  

:::danger 警告 
手动编辑过后请检查JSON字符串的合法性，请保证确保符合参考文件的JSON文件格式！！！  
（注意：上面示例中的中文注释仅为说明用途，实际写入文件时请删除注释）  
::: 
### WEB端口设置
如果是部署在公网或者有需要从外部访问的需求，请在系统防火墙和可能存在的云平台安全组中打开DDTV的WEB服务所需端口(默认为**11419**)  

## 4.启动&初始化
1.使用压缩包最外层提供的`启动DDTV_Client.bat`脚本启动或者使用`./bin/Client.exe`直接启动。   
2.在弹出来的窗口中输入配置文件中的`AccessKeyId`以及`AccessKeySecret`后进入WEBUI（`AccessKeyId`默认为`ddtv`，`AccessKeySecret`为首次启动时随机生成并打印在控制台中，也可在`./bin/Config/DDTV_Config.ini`中查看，详见[WEBUI说明](./WEBUI.md)）

## 其他功能
如有任何疑问都可以加群338182356进行对线
