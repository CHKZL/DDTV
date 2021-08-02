# DDTV Core通用配置文件
## Core配置文件说明
Core配置文件为DDTV核心初始化配置文件，在DDTV2.0和DDTVLiveRec中都存在  
  
其中在DDTV2.0中叫`DDTV_NEW.exe.config`、在DDTVLiveRec中叫`DDTVLiveRec.exe.config`或`DDTVLiveRec.dll.config`
:::tip
其中在Windows下为exe，在linux和MacOS下为dll 
:::

## 完整的配置文件示例
:::tip
如果你的配置文件中有在这之外的内容，如没特殊需要，请勿修改
:::
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <appSettings>
    <add key="file" value="./tmp/" /><!-- 下载文件默认目录，如果没有特殊需求请勿修改 -->
    <add key="Livefile" value="./tmp/LiveCache/" /><!-- DDTV2.0在线播放时产生的缓存文件，会自己定期清理 -->
    <add key="RoomConfiguration" value="./RoomListConfig.json" /><!-- 房间配置文件的路径和名称，如果没有特殊需求请勿修改 -->
    <add key="RoomTime" value="20" /><!-- Core对于直播状态的查询间隔，推荐默认值即可，过快的间隔可能会导致被http 412黑名单半小时 -->
    <add key="DefaultVolume" value="100" /><!-- DDTV2.0中的默认音量 -->
    <add key="Zoom" value="1" /><!-- 配置DDTV2.0的缩小默认值，1为缩小到开始菜单，0为缩小到后台托盘 -->
    <add key="PlayNum" value="5" /><!-- DDTV2.0的最大直播并行数量 -->
    <add key="DanMuColor" value="0xFF,0x00,0x00,0x00" /><!-- DDTV2.0的默认弹幕颜色 -->
    <add key="ZiMuColor" value="0xFF,0x00,0x00,0x00" /><!-- DDTV2.0的默认字幕颜色 -->
    <add key="DanMuSize" value="20" /><!-- DDTV2.0的默认弹幕字体大小 -->
    <add key="ZiMuSize" value="24" /><!-- DDTV2.0的默认字幕字体大小 -->
    <add key="LiveListTime" value="5" /><!-- DDTV2.0的直播列表UI刷新间隔，如果没有特殊需求请勿修改 -->
    <add key="PlayWindowH" value="450" /><!-- DDTV2.0的播放器默认高度 -->
    <add key="PlayWindowW" value="800" /><!-- DDTV2.0的播放器默认宽度 -->
    <add key="BufferDuration" value="3" /><!-- DDTV2.0播放前缓冲多少秒 -->
    <add key="AutoTranscoding" value="0" /><!-- 是否启动自动转码功能，详细信息请参考'高级功能配置'中关于自动转码文档 -->
    <add key="DataSource" value="0" /><!-- DDTV默认的数据源，0和1为B站原生API接口不同配置，2为Vtbs辅助数据源，默认0 -->
    <add key="IsFirstTimeUsing" value="1" /><!-- DDTV2.0引导初始化，如果为1，启动时会启动初始化引导，初始化完成后会自动改为0 -->
    <add key="LiveRecWebServerDefaultIP" value="0.0.0.0" /><!-- DDTVLiveRec的WEB服务监听来源IP，0.0.0.0为监听全部地址 -->
    <add key="Port" value="11419" /><!-- DTVLiveRec的WEB服务监听端口 -->
    <add key="RecordDanmu" value="0" /><!-- 是否启动弹幕录制，0为关闭，1为启动 -->
    <add key="DebugFile" value="1" /><!-- 是否把Debug日志输出到日志文件 -->
    <add key="DebugCmd" value="1" /><!-- 是否把Debug信息现在到控制台界面 -->
    <add key="DebugMod" value="1" /><!-- 是否启动Debug模式 -->
    <add key="DevelopmentModel" value="0" /><!-- 开发模式开关，默认0关闭，如无特殊需求或指导请勿打开 -->
    <add key="DokiDoki" value="300" /><!-- 控制台每多少秒输出一次心跳信息，该心跳信息会通过WebHook同步发送 -->
    <add key="sslName" value="" /><!-- DDTVLiveRec的WEB服务启动SSL连接后pfx证书文件的路径和名称 -->
    <add key="sslPssword" value="" /><!-- pfx证书文件密码 -->
    <add key="EnableUpload" value="0" /><!-- 自动上传功能总开关，默认0关闭，1启动 -->
    <add key="DeleteAfterUpload" value="1" /><!-- 上传完成后是否自动删除文件 -->
    <add key="EnableOneDrive" value="0" /><!-- 是否启动OneDrive自动上传 -->
    <add key="OneDriveConfig" value="" /><!-- OneDrive配置文件 -->
    <add key="OneDrivePath" value="" /><!-- OneDrive根目录 -->
    <add key="EnableCos" value="0" /><!-- 是否启动腾讯云对象储存自动上传 -->
    <add key="CosSecretId" value="" /><!-- 腾讯云对象储存SecretId -->
    <add key="CosSecretKey" value="" /><!-- 腾讯云对象储存SecretKey -->
    <add key="CosRegion" value="" /><!-- 腾讯云对象储存的存储桶地域 -->
    <add key="CosBucket" value="" /><!--腾讯云对象储存的存储桶名称  -->
    <add key="CosPath" value="" /><!-- 上传到腾讯云对象储存的路径 -->
    <add key="AutoTranscodingDelFile" value="0" /><!-- 转码后自动删除文件开关 -->
    <add key="ApiToken" value="1145141919810A" /><!-- DDTVLiveRec的API服务的Token秘钥 -->
    <add key="WebUserName" value="ami" /><!-- DDTVLiveRec的WEB信息页登陆用户名 -->
    <add key="WebPassword" value="ddtv" /><!-- DDTVLiveRec的WEB信息页登陆密码 -->
    <add key="WebhookUrl" value="" /><!-- DDTVLiveRec的Webhook目标地址 -->
    <add key="WebhookEnable" value="0" /><!-- DDTVLiveRec的Webhook开关 -->
  </appSettings>
  <system.net>
    <connectionManagement>
      <add address="*" maxconnection="512" /><!-- 系统配置。默认最高并行下载数量，如没特殊需求请去修改 -->
    </connectionManagement>
  </system.net>
</configuration>
```