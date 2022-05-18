# DDTV Core通用配置文件
## Core配置文件说明
配置文件`DDTV_Config.ini`为DDTV核心初始化配置文件，在DDTV_GUI和DDTV_WEB_Server中都存在   
配置文件所属的分组必须正确，如果放错了分组，是不会被读取的  

## 完整的配置文件示例
:::tip
如没特殊需要，请勿手动修改，请使用GUI或者API接口进行程序化修改
:::
```ini
[Core]
RoomListConfig=./RoomListConfig.json
ClientAID=9cc4fd9c-1116-4a92-8428-24a4b83c608d
GUI_FirstStart=false
WEB_FirstStart=false
IsAutoTranscod=True
TranscodParmetrs=-i {Before} -vcodec copy -acodec copy {After}
InstanceAID=956050d3
TranscodingCompleteAutoDeleteFiles=false
DDcenterSwitch=false
WebHookUrl=
[WEB_API]
WEB_API_SSL=false
AccessControlAllowOrigin=http://127.0.0.1:5500
AccessControlAllowCredentials=true
WebUserName=ami
WebPassword=ddtv
pfxFileName=pfxFileName
pfxPasswordFileName=pfxPasswordFileName
AccessKeyId=3fce1282b6104887ba494eaf4f714147
AccessKeySecret=2f9fda70a48c4b2eb3d60ff07ddc67ab
ServerAID=e3545f6d-63d9-4898-81dd-9be9c6439d6f
ServerName=DDTV_Server
CookieDomain=
[Download]
DoNotSleepWhileDownloading=true
DownloadPath=.\Rec
DownloadDirectoryName={ROOMID}_{NAME}
DownloadFileName={DATE}_{TIME}_{TITLE}
TmpPath=.\Tmp
RecQuality=10000
IsRecDanmu=true
IsRecGift=true
IsRecGuard=true
IsRecSC=true
IsFlvSplit=False
FlvSplitSize=1073741824
Shell=false
ForceCDNResolution=False
[Play]
DefaultVolume=100
PlayQuality=10000
[GUI]
HideIconState=false


```
### 配置说明
```csharp
        /// <summary>
        /// 配置分组(每个值对应的组是固定的，请勿随意填写)
        /// </summary>
        public enum Group
        {
            /// <summary>
            /// 缺省配置组(按道理应该给每个配置都设置组，不应该在缺省组里)
            /// </summary>
            Default,
            /// <summary>
            /// DDTV_Core运行相关的配置
            /// </summary>
            Core,
            /// <summary>
            /// 下载系统运行相关的配置
            /// </summary>
            Download,
            /// <summary>
            /// WEBAPI相关的配置
            /// </summary>
            WEB_API,
            /// <summary>
            /// 播放器相关设置
            /// </summary>
            Play,
            /// <summary>
            /// GUI相关设置
            /// </summary>
            GUI,
        }
        /// <summary>
        /// 配置键
        /// </summary>
        public enum Key
        {
            /// <summary>
            /// 房间配置文件路径 (应该是一个绝对\相对路径文件地址)
            /// 组：Core      默认值：./RoomListConfig.json
            /// </summary>
            RoomListConfig,
            /// <summary>
            /// 默认下载总文件夹路径 (应该是一个绝对\相对路径目录)
            /// 组：Download  默认值：./Rec/
            /// </summary>
            DownloadPath,
            /// <summary>
            /// 临时文件存放文件夹路径 (应该是一个绝对\相对路径文件地址)
            /// 组：Download  默认值：./tmp/
            /// </summary>
            TmpPath,
            /// <summary>
            /// 默认下载文件夹名字格式 (应该为关键字组合，如:{KEY}_{KEY})
            /// 组：Download  默认值：{ROOMID}_{NAME}        可选值：ROOMID|NAME|DATE|TIME|TITLE|R
            /// </summary>
            DownloadDirectoryName,
            /// <summary>
            /// 默认下载文件名格式 (应该为关键字组合，如:{KEY}_{KEY})
            /// 组：Download  默认值：{DATE}_{TIME}_{TITLE}  可选值：ROOMID|NAME|DATE|TIME|TITLE|R
            /// </summary>
            DownloadFileName,
            /// <summary>
            /// 转码默认参数 (应该是带{After}{Before}的ffmpeg参数字符串，如:-i {Before} -vcodec copy -acodec copy {After})
            /// 组：Core      默认值：-i {Before} -vcodec copy -acodec copy {After}
            /// </summary>
            TranscodParmetrs,
            /// <summary>
            /// 自动转码 (自动转码的使能配置，为布尔值false或ture)
            /// 组：Core      默认值：false
            /// </summary>
            IsAutoTranscod,
            /// <summary>
            /// 是否启用WEB_API加密证书 (应该为布尔值)
            /// 组：WEB_API   默认值：false
            /// </summary>
            WEB_API_SSL,
            /// <summary>
            /// WEB_API启用HTTPS后调用的pfx证书文件路径 (应该是一个绝对\相对路径文件地址)
            /// 组：WEB_API   默认值：
            /// </summary>
            pfxFileName,
            /// <summary>
            /// WEB_API启用后HTTPS调用的pfx证书秘钥文件路径 (应该是一个绝对\相对路径文件地址)
            /// 组：WEB_API   默认值：
            /// </summary>
            pfxPasswordFileName,
            /// <summary>
            /// 播放器默认音量 (应该是一个uint值)
            /// 组：Play      默认值：50      可选值：0-100
            /// </summary>
            DefaultVolume,
            /// <summary>
            /// GUI首次启动标志位 (应该是一个布尔值第一次启动为真)
            /// 组：Core      默认值：true
            /// </summary>
            GUI_FirstStart,
            /// <summary>
            /// WEB首次启动标志位 (应该是一个布尔值第一次启动为真)
            /// 组：Core      默认值：true
            /// </summary>
            WEB_FirstStart,
            /// <summary>
            /// 录制分辨率 (应该为有限的int值)
            /// 组：Download  默认值：10000  可选值：流畅:80  高清:150  超清:250  蓝光:400  原画:10000
            /// </summary>
            RecQuality,
            /// <summary>
            /// 默认在线观看的分辨率 (应该为有限的int值)
            /// 组：Play      默认值：250    可选值：流畅:80  高清:150  超清:250  蓝光:400  原画:10000
            /// </summary>
            PlayQuality,
            /// <summary>
            /// 全局弹幕录制开关 (布尔值，每个房间自己在房间配置列表单独设置，这个是是否启用弹幕录制功能的总共开关)
            /// 组：Download  默认值：true
            /// </summary>
            IsRecDanmu,
            /// <summary>
            /// 全局礼物录制开关 (布尔值)
            /// 组：Download  默认值：true
            /// </summary>
            IsRecGift,
            /// <summary>
            /// 全局上舰录制开关 (布尔值)
            /// 组：Download  默认值：true
            /// </summary>
            IsRecGuard,
            /// <summary>
            /// 全局SC录制开关 (布尔值)
            /// 组：Download  默认值：true
            /// </summary>
            IsRecSC,
            /// <summary>
            /// 全局FLV文件按大小切分开关 (布尔值)
            /// 组：Download  默认值：false
            /// </summary>
            IsFlvSplit,
            /// <summary>
            /// 当IsFlvSplit为真时使能，FLV文件切分的大小 (应该为long值，切割值应该以byte计算)
            /// 组：Download  默认值：1073741824
            /// </summary>
            FlvSplitSize,
            /// <summary>
            /// WEB登陆使用的用户名 (string)
            /// 组：WEB_API   默认值：ami
            /// </summary>
            WebUserName,
            /// <summary>
            /// WEB登陆使用的密码 (string)
            /// 组：WEB_API   默认值：ddtv
            /// </summary>
            WebPassword,
            /// <summary>
            /// WEBAPI使用的KeyId (string)
            /// 组：WEB_API   默认值：(随机字符串)
            /// </summary>
            AccessKeyId,
            /// <summary>
            /// WEBAPI使用的KeySecret (string)
            /// 组：WEB_API   默认值：(随机字符串)
            /// </summary>
            AccessKeySecret,
            /// <summary>
            /// 用于标记服务器资源ID编号 (string)
            /// 组：WEB_API   默认值：(随机字符串)
            /// </summary>
            ServerAID,
            /// <summary>
            /// 用于标记服务器名称 (string)
            /// 组：WEB_API   默认值：DDTV_Server
            /// </summary>
            ServerName,
            /// <summary>
            /// 客户端唯一标识 (string)
            /// 组：Core      默认值：(随机字符串)
            /// </summary>
            ClientAID,
            /// <summary>
            /// DDTVGUI缩放是否隐藏到托盘
            /// 组：GUI       默认值：false
            /// </summary>
            HideIconState,
            /// <summary>
            /// DDTV_WEB跨域设置路径（应为前端网址，必须带协议和端口号，如：http://127.0.0.1:5500）
            /// 组：WEB_API   默认值：*
            /// </summary>
            AccessControlAllowOrigin,
            /// <summary>
            /// DDTV_WEB的Credentials设置 (布尔值)
            /// 组：WEB_API   默认值：true
            /// </summary>
            AccessControlAllowCredentials,
            /// <summary>
            /// 用于控制下载时时候阻止系统休眠 (布尔值)
            /// 组：Download   默认值：true
            /// </summary>
            DoNotSleepWhileDownloading,
            /// <summary>
            /// 用于控制cookie作用域（字符串）
            /// 组：WEB_API   默认值：string.Empty
            /// </summary>
            CookieDomain,
            /// <summary>
            /// 用于控制下载完成后是否执行对应房间的Shell命令（布尔值）
            /// 组：Download   默认值：false
            /// </summary>
            Shell,
            /// <summary>
            /// WebHook的目标地址（字符串）
            /// 组：Core   默认值：string.Empty
            /// </summary>
            WebHookUrl,
            /// <summary>
            /// 实例AID用于在联网情况下区分客户端（字符串）
            /// 组：Core   默认值：随机字符串
            /// </summary>
            InstanceAID,
            /// <summary>
            /// DDC的采集开关 (布尔值)
            /// 组：Core   默认值：false
            /// </summary>
            DDcenterSwitch,
            /// <summary>
            /// 转码完成后自动删除文件开关 (布尔值)
            /// 组：Core   默认值：false
            /// </summary>
            TranscodingCompleteAutoDeleteFiles,
            /// <summary>
            /// 是否强使用主CDN下载地址 (布尔值)
            /// 组：Download   默认值：false
            /// </summary>
            ForceCDNResolution,
        }
```