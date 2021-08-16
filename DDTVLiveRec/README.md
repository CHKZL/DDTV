# DDTVLiveRec

一个低消耗的windows/linux/MacOS全平台通用的B站直播录制工具

***
(代码中大量 **中文** 变量\函数 **易#** 警告，美术生出身，请各位大佬轻拍 
***
<br/>

# 功能完成情况
* 支持linux，可以挂在路由器或树莓派等linux嵌入式设备上运行
* 开播自动录制
* 录制完成后自动合并文件
* 录制完成后自动转码为MP4并修复直播流flv时间轴错误的问题 (在Linux/MacOS上需要自行安装ffmpeg)
* 多路异步下载
* 在网页直接查看运行状态\日志\下载文件列表
* 登陆买票后可以录制付费直播内容
* 在录制的同时储存弹幕信息为ass文件
* API操作
  
有任何问题和需要增加的功能欢迎加群：307156949直接说  
<br/>

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
　　4.在将`\static\config.js`中的`apiUrl`改成你服务器对应的信息  
　　3.Linux系统使用[dotnet ./DDTVLiveRec.dll]命令直接启动或者是用类似[nohup dotnet ./DDTVLiveRec.dll &]之类的命令后台启动。  windows系统直接使用[DDTVLiveRec.exe]启动  
　　4.(DDTVLive会监听11419端口，如果防火墙阻止请允许，该端口用于信息反馈的本地web服务端)
　　5.访问访问[http://IP:11419]查看()  
* 录制的视频文件在文件夹中的["tmp"]文件夹内


web服务端注意事项:  
首先需要将`\static\config.js`中的`apiUrl`改成你服务器对应的信息   
启动DDTVLiveRec后会自动启动内置的web服务端,请访问[http://IP:11419]查看    
WEB服务端信息页的默认账号密码为在配置文件`DDTVLiveRec.config`中的`WebUserName`和`WebPassword`     
    
<br/>

# API 说明文档

* [点击查看API文档](https://github.com/CHKZL/DDTV2/blob/master/Doc/docs/API/README.md)

# Webhook 说明文档
* [点击查看Webhook文档](https://github.com/CHKZL/DDTV2/blob/master/Doc/docs/API/WebHook.md)


# 如果使用Docker构建:

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
<br/>

# 录制配置：RoomListConfig.json说明：
格式和解析方式和DDTV一样，格式为json字符串，releases发布的压缩包里附带了一个参考的文件。  
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
<br/>

# 启用自动转码
在**DDTVLiveRec.dll.config**文件中，将[AutoTranscoding]的值改为1，也就是
```ini
<add key="AutoTranscoding" value="1" />
```   
（注:如果没有DDTVLiveRec.dll.config文件，请先运行一次DDTVLiveRec，会自动生成该文件）  
<br/>
请注意！自动转码会**消耗大量CPU资源**，CPU资源不够可能会**导致不断重连、断播、网络连接失败等故障！！**  
请注意！自动转码会**消耗大量CPU资源**，CPU资源不够可能会**导致不断重连、断播、网络连接失败等故障！！**  
请注意！自动转码会**消耗大量CPU资源**，CPU资源不够可能会**导致不断重连、断播、网络连接失败等故障！！**  


## windows加载ffmpeg
>在根目录中建立**libffmpeg**文件夹，将DDTV本地中对应的[ffmpeg.exe]文件给复制到该目录  
(也就是和DDTVLiveRec执行文件的相对路径为：[./libffmpeg/ffmpeg.exe])
## Linux\MacOS加载ffmpeg
>根据自己的系统安装ffmpeg，并将ffmpeg加入系统环境变量中  


<br/>

# 运行界面
主界面   
![运行界面](./软件图/主界面.png)    

系统概况预览  
![系统概况预览](./软件图/系统详情.png)   

WEB下载状况查看界面   
![WEB下载状况查看界面](./软件图/下载信息界面.png)

WEB文件列表界面   
![WEB文件列表界面](./软件图/下载文件信息列表.png)   


# DDTVLiveRec上传模块使用说明

## 0 基本设置  
配置文件：DDTVLiveRec.dll.config中需要配置的参数  
- DeleteAfterUpload  
  上传后删除源文件：1删除 2保留  
  只有当最后一个上传任务成功后 才会执行删除操作

- Enablexxx  
  开启xxx上传并设置顺序  
  当为0时不上传至此目标，其他数字为上传的次序

- xxxPath  
  上传至xxx的目录，默认为$/$根目标  
  该目录下会根据直播间进行分类，每个文件夹下根据直播时间再分类
## 1 OneDrive
1. 软件安装 安装OneDriveUploader  
https://github.com/MoeClub/OneList/tree/master/OneDriveUploader

    - Windows用户下载i386(32位)/amd64(64位)中的可执行文件到DDTVLiveRec.exe的同级目录下  
    - Linux用户
        ```sh
        wget https://raw.githubusercontent.com/MoeClub/OneList/master/OneDriveUploader/amd64/linux/OneDriveUploader -P /usr/local/bin/
        chmod +x /usr/local/bin/OneDriveUploader
        ```
2. 配置  
   根据[安装OneDriveUploader]在github页上的说明，先初始化他的配置文件  
   再在DDTVLiveRec.dll.config 中将初始化后生成的auth.json位置填入DDTVLiveRec配置文件的$OneDriveConfig$  

## 2 腾讯Cos对象存储
1. 申请腾讯云API
2. DDTVLiveRec配置文件中填写相关信息(如果配置文件中没有对应的值，则启动一次DDTVLiveRec即可(包括登陆的整个流程走一次))  
   - CosSecretId API中的ID
   - CosSecretKey API中的Key
   - CosRegion 存储桶所在区域，格式为 xx-xxxx
   - CosBucket 存储桶名称，格式为 bucketname-appid

没有你的上传目标？试试自己添加吧！
[相关文档](./Upload.md)  
<br/>
<br/>

# 写给不会使用gayhub的：怎么下载？？？怎么下载？？？怎么下载？？？
↓↓↓↓↓↓↓↓↓↓↓↓↓↓点击跳转下载页面↓↓↓↓↓↓↓↓↓↓↓↓↓↓  
[点击跳转到releases下载页面](https://github.com/CHKZL/DDTV2/releases/latest)  
↑↑↑↑↑↑↑↑↑↑↑↑↑↑点击跳转下载页面↑↑↑↑↑↑↑↑↑↑↑↑↑↑   
===如果实在是下载不动也可以加群在群共享中下载,群：307156949===

<br/>

# 关于监控列表
兼容DDTVLiveRec，DDTV1.0，DDTV2.0通用。    

### 在更新软件的时候请备份好RoomListConfig.json文件，该文件是监控房间配置文件
### 在更新软件的时候请备份好RoomListConfig.json文件，该文件是监控房间配置文件
### 在更新软件的时候请备份好RoomListConfig.json文件，该文件是监控房间配置文件
<br/>


# 使用到的第三方组件
* [BiliAccount](https://github.com/LeoChen98/BiliAccount)
* [FFmpeg](https://github.com/FFmpeg/FFmpeg)
* [vtbs.moe](https://github.com/dd-center/vtbs.moe)
