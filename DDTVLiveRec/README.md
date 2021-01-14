# DDTVLiveRec

一个低消耗的windows/linux/MacOS全平台通用的B站直播录制工具

***
(代码中大量 **中文** 变量\函数 **易#** 警告，美术生出身，请各位大佬轻拍 
***

# 功能完成情况
* 支持linux，可以挂在路由器或树莓派等linux嵌入式设备上运行
* 开播自动录制
* 录制完成后自动合并文件
* 录制完成后自动转码为MP4并修复直播流flv时间轴错误的问题 (在Linux/MacOS上需要自行安装ffmpeg)
* 多路异步下载
* 在网页直接查看运行状态\日志\下载文件列表
* 登陆买票后可以录制付费直播内容
* 在录制的同时储存弹幕信息为ass文件
  
有任何问题和需要增加的功能欢迎加群：307156949直接说

# 使用说明
    
DDTVLiveRec的releases只提供依赖框架的可移植版本，请确保环境已经安装以下两个环境依赖
  
* ASP.NET5运行时(ASP.NET Core Runtime 5.0.0) 
* .NET5运行时(.NET Runtime)
如未安装不能启动，下列方法二选一：  
1.请到参考[微软文档](https://docs.microsoft.com/zh-cn/dotnet/core/install/)进行环境的安装  
2.下载[下载ASP.NET5.0运行时](https://download.visualstudio.microsoft.com/download/pr/48dd125b-b9ca-4fc7-b26c-558bff5bee13/214be31c3239444d4a9cfdf0574f3cd8/aspnetcore-runtime-5.0.1-win-x64.exe)和[下载NET5.0运行时](https://download.visualstudio.microsoft.com/download/pr/93095e51-be33-4b28-99c8-5ae0ebba753d/501f77f4b95d2e9c3481246a3eff9956/dotnet-runtime-5.0.1-win-x64.exe)2个文件进行安装，然后运行DDTVLiveRec

启动准备:  
　　1.因为DDTVLiveRec是根据DDTV2部分功能移植而来，所以需要依赖DDTV的配置文件，在使用前请先保证有一个可以正常使用的DDTV最新版本，并且已经登录。  
　　2.把DDTV目录里登陆并配置好的【RoomListConfig.json】和【BiliUser.ini】复制到对应的DDTVLive文件夹中，该文件是房间配置文件和bilibili登陆验证文件  
　　3.Linux系统使用[dotnet ./DDTVLiveRec.dll]命令直接启动或者是用类似[nohup dotnet ./DDTVLiveRec.dll &]之类的命令后台启动。  windows系统直接使用[DDTVLiveRec.exe]启动  
　　4.(DDTVLive会监听11419端口，如果防火墙阻止请允许，该端口用于信息反馈的本地web服务端)

* 录制的视频文件在文件夹中的["tmp"]文件夹内


web服务端:  
启动DDTVLiveRec后会自动启动内置的web服务端,提供了三个页面：  
[http://IP:11419/log]：DDTVLiveRec日志信息  
[http://IP:11419/file]：DDTVLiveRec录制的文件列表  
[http://IP:11419/list]：DDTVLiveRec下载列表状态查看  
[http://IP:11419/config]：DDTVLiveRec的配置修改命令列表  
[http://IP:11419/systeminfo]：DDTVLiveRec的系统总览页面

### 如果使用Docker构建:

1. 构建Docker镜像：

```bash
docker build -f DDTVLiveRec/Dockerfile -t ddtv:latest .
```

2. 运行Docker：

```bash
docker run -d \
    --restart always \
    -p 11419:11419 \
    -v ${CONFIG_DIR}/BiliUser.ini:/DDTVLiveRec/BiliUser.ini \
    -v ${CONFIG_DIR}/DDTVLiveRec.dll.config:/DDTVLiveRec/DDTVLiveRec.dll.config \
    -v ${CONFIG_DIR}/RoomListConfig.json:/DDTVLiveRec/RoomListConfig.json \
    -v ${DOWNLOAD_DIR}:/DDTVLiveRec/tmp \
    --name ddtv \
    ddtv:latest
```

# 录制配置：RoomListConfig.json说明：
　　格式和解析方式和DDTV一样，格式为json字符串，releases发布的压缩包里附带了一个参考的文件。  
　　【重要】默认只支持[VTBS](https://vtbs.moe/)有记录的房间监控，如果有未记录的V，到vtbs提交新的V即可。如需监控非V的房间，需要打开配置文件中的NV选项(NotVTBStatus)，切连接的非V房间推荐不要超过5个，因为阿B的服务器限制，可能造成未知错误。 
房间配置文件格式为
```json
{
            "Name": "绯赤艾莉欧",
            "OfficialName": "緋赤エリオ",
            "RoomNumber": "21396545",
            "Types": "bilibili",
            "status": false,
            "VideoStatus": false,
            "RemindStatus": false,
            "LiveStatus": false
}
```
多个这种格式的内容组成，其中
* ["Name"]为翻译名称
* ["OfficialName"]为官方名称
* ["RoomNumber"]为BiliLive房间号
* ["Types"]为DDTV区分直播平台预留，默认并必须为"bilibili"
* ["status"]为DDTV需要的配置文件，在DDTVLive中无用，默认为false
* ["VideoStatus"]为是否开播自动录制的标识，false为检测到开播后不录制，true为检测到开播后自动录制
* ["RemindStatus"]为DDTV开播弹窗提醒标识，在DDTVLive中无用，默认为false
* ["LiveStatus"]为DDTV直播状态标识，在DDTVLive中无用，默认为false


**也就是说["VideoStatus"]为false的项不会自动录制，请注意**   
使用DDTVLiveRec需要注意的为["Name"]["OfficialName"]["RoomNumber"]["VideoStatus"]  

### 一定要确保符合参考文件的JSON文件格式！！！

# 运行界面
主界面   
![运行界面](./软件图/主界面.png)    

系统概况预览  
![系统概况预览](./软件图/系统详情.png)   

WEB下载状况查看界面   
![WEB下载状况查看界面](./软件图/下载信息界面.png)

WEB文件列表界面   
![WEB文件列表界面](./软件图/下载文件信息列表.png)   




## 写给不会使用gayhub的：怎么下载？？？怎么下载？？？怎么下载？？？
↓↓↓↓↓↓↓↓↓↓↓↓↓↓点击跳转下载页面↓↓↓↓↓↓↓↓↓↓↓↓↓↓  
[点击跳转到releases下载页面](https://github.com/CHKZL/DDTV2/releases/latest)  
↑↑↑↑↑↑↑↑↑↑↑↑↑↑点击跳转下载页面↑↑↑↑↑↑↑↑↑↑↑↑↑↑   
===如果实在是下载不动也可以加群在群共享中下载,群：307156949===


# 关于监控列表
兼容DDTVLiveRec，DDTV1.0，DDTV2.0通用。    

### 在更新软件的时候请备份好RoomListConfig.json文件，该文件是监控房间配置文件
### 在更新软件的时候请备份好RoomListConfig.json文件，该文件是监控房间配置文件
### 在更新软件的时候请备份好RoomListConfig.json文件，该文件是监控房间配置文件


# 使用到的第三方组件
* [BiliAccount](https://github.com/LeoChen98/BiliAccount)
* [FFmpeg](https://github.com/FFmpeg/FFmpeg)
* [vtbs.moe](https://github.com/dd-center/vtbs.moe)
