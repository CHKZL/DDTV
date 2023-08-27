# DDTV_WEB_Server 接口文档

## 接口说明
当前DDTV的接口分为两种，并且只对DDTV_WEB_Server有效，**其他版本是不能使用WEB请求和API操作接口的**
|接口类型|请求是否需要携带cookie|请求是否需要携带sig|说明|
|:--:|:--:|:--:|:--:|
|WEB请求|✅|❌|一般用于前端请求，需要先调用login接口进行登录，之后的操作每次携带登录返回的cookie即可|
|API操作|❌|✅|一般用于程序调用，每个请求都可以直接调用，每次操作都需要携带根据key计算的sig|

## 两种接口的调用差异
请务必先请查看该路径下WEB请求和API操作相关页面的说明
请务必先请查看该路径下WEB请求和API操作相关页面的说明
请务必先请查看该路径下WEB请求和API操作相关页面的说明

## swagger
DDTV_WEB_Server自带swagger方便进行调试，请使用`http(s)://[IP地址]:11419/swagger/index.html`进行访问调试

## 已实现的通用API列表
|方式|名称|返回内容|解释|
|:--:|:--:|:--:|:--:|
|POST|System_Info|JSON|[获取系统运行情况](./#post-api-system-info)|
|POST|System_Config|JSON|[获取系统配置文件信息](./#post-api-system-config)|
|POST|System_Resources|JSON|[获取系统硬件资源使用情况](./#post-api-system-resources)|
|POST|System_Log|JSON|[获取历史日志](./#post-api-system-log)|
|POST|System_LatestLog|JSON|[获取最新日志](./#post-api-system-latestlog)|
|POST|System_QueryWebFirstStart|JSON|[返回一个可以自行设定的初始化状态值](./#post-api-system-querywebfirststart)|
|POST|System_SetWebFirstStart|JSON|[设置初始化状态值](./#post-api-system-setsebfirststart)|
|POST|System_QueryUserState|JSON|[查询阿B接口返回数据判断用户登陆状态是否有效](./#post-api-system-queryuserstate)|
|POST|Config_Transcod|JSON|[设置自动转码总开关](./#post-api-config-Transcod)|
|POST|Config_FileSplit|JSON|[根据文件大小自动切片](./#post-api-config-filesplit)|
|POST|Config_DanmuRec|JSON|[弹幕录制总共开关(包括礼物、舰队、SC)](./#post-api-config-danmurec)|
|POST|Config_GetFollow|JSON|[导入关注列表中的V](./#post-api-config-getfollow)|
|POST|File_GetAllFileList|JSON|[获取已录制的文件列表](./#post-api-file-getallfilelist)|
|POST|File_GetTypeFileList|JSON|[分类获取已录制的文件总列表](./#post-api-file-gettypefilelist)|
|POST|File_GetFilePathList|JSON|[根据文件树结构返回已录制的文件总列表](./#post-api-file-getfilepathlist)|
|GET|File_GetFile|FileStram|[下载对应的文件](./#post-api-file-getfile)|
|POST|Login|JSON|[WEB登陆](./#post-api-login)|
|GET|loginqr|PNG|[在提示登陆的情况下获取用于的登陆二维码](./#get-api-loginqr)|
|POST|Login_Reset|JSON|[重新登陆哔哩哔哩账号](./#post-api-login-reset)|
|POST|Login_State|JSON|[查询内部登陆状态](./#post-api-login-state)|
|POST|Rec_RecordingInfo|JSON|[获取下载中的任务情况详细情况](./#post-api-rec-recordinginfo)|
|POST|Rec_RecordingInfo_Lite|JSON|[获取下载中的任务情况简略情况](./#post-api-rec-recordinginfo-lite)|
|POST|Rec_RecordCompleteInfon|JSON|[获取已经完成的任务详细情况](./#post-api-rec-recordcompleteinfon)|
|POST|Rec_RecordCompleteInfon_Lite|JSON|[获取已经完成的任务简略情况](./#post-api-rec-recordcompleteinfon-lite)|
|POST|Rec_CancelDownload|JSON|[取消某个下载任务](./#post-api-rec-canceldownload)|
|POST|Room_AllInfo|JSON|[获取房间详细配置信息](./#post-api-room-allinfo)|
|POST|Room_SummaryInfo|JSON|[获取房间简要配置信息](./#post-api-room-summaryinfo)|
|POST|Room_Add|JSON|[增一个加房间配置](./#post-api-room-add)|
|POST|Room_Del|JSON|[删除一个房间配置](./#post-api-room-del)|
|POST|Room_AutoRec|JSON|[修改房间自动录制配置信息](./#post-api-room-autorec)|
|POST|Room_DanmuRec|JSON|[修改房间弹幕录制配置信息](./#post-api-room-danmurec)|
|POST|User_Search|JSON|[通过阿B搜索搜索直播用户](./#post-api-user-search)|

## 返回数据内容格式

```csharp
        public class pack<T>
        {
            /// <summary>
            /// 状态码
            /// </summary>
            public code code { get; set; }
            /// <summary>
            /// 接口名称
            /// </summary>
            public string cmd { get; set; }
            /// <summary>
            /// 信息
            /// </summary>
            public string massage { get; set; }
            /// <summary>
            /// 对应的接口数据
            /// </summary>
            public T data { get; set; }
        }
```

## 返回结果状态码列表

|值|含义|
|:--:|:--:|
|0|成功|
|-2|UID不存在|
|6000|登陆信息失效|
|6001|登陆验证失败|
|6002|APIsig计算校验失败|
|7000|操作失败|

## 接口详细说明
### `POST /api/System_Info`
::: details 获取系统运行情况
- 私有变量

无

- 返回数据说明
```CSharp
        public class Info
        {
            /// <summary>
            /// 当前DDTV版本号
            /// </summary>
            public string DDTVCore_Ver { get; set; }
            /// <summary>
            /// 监控房间数量
            /// </summary>
            public int Room_Quantity { get; set; }
            /// <summary>
            /// 设置的服务器名称
            /// </summary>
            public string ServerName { get; set; }
            /// <summary>
            /// 服务器的唯一资源编号
            /// </summary>
            public string ServerAID { get; set; }
            /// <summary>
            /// 操作系统相关信息
            /// </summary>
            public OS_Info os_Info { get; set; }
            /// <summary>
            /// 下载任务基础信息
            /// </summary>
            public Download_Info download_Info { get; set; }
            public class OS_Info
            {
                /// <summary>
                /// 系统版本
                /// </summary>
                public string OS_Ver { get; set; }
                /// <summary>
                /// 系统类型
                /// </summary>
                public string OS_Tpye { get; set; }
                /// <summary>
                /// 使用内存量，单位bit
                /// </summary>
                public long Memory_Usage { get; set; }
                /// <summary>
                /// 运行时版本
                /// </summary>
                public string Runtime_Ver { get; set; }
                /// <summary>
                /// 是否在交互模式下
                /// </summary>
                public bool UserInteractive { get; set; }
                /// <summary>
                /// 关联的用户
                /// </summary>
                public string Associated_Users { get; set; }
                /// <summary>
                /// 工作目录
                /// </summary>
                public string Current_Directory { get; set; }
                /// <summary>
                /// Core程序核心框架版本
                /// </summary>
                public string AppCore_Ver { set; get; }
                /// <summary>
                /// Web程序核心框架版本
                /// </summary>
                public string WebCore_Ver { set; get; }
            }
            public class Download_Info
            {
                /// <summary>
                /// 下载中的任务数
                /// </summary>
                public int Downloading { get; set; }
                /// <summary>
                /// 下载结束的任务数
                /// </summary>
                public int Completed_Downloads { get; set; }
            }
        }
```
:::

### `POST /api/System_Config`
::: details 获取系统配置文件信息
- 私有变量

无

- 返回数据说明
```CSharp
        public class Config
        {
            public List<Data> datas = new();
            public class Data
            {
                /// <summary>
                /// 配置键
                /// </summary>
                public Key Key { set; get; }
                /// <summary>
                /// 配置键名称
                /// </summary>
                public string KeyName { set; get; }
                /// <summary>
                /// 配置分组
                /// </summary>
                public Group Group { set; get; } = Group.Default;
                /// <summary>
                /// 配置值
                /// </summary>
                public string Value { set; get; } = "";
                /// <summary>
                /// 是否有效
                /// </summary>
                public bool Enabled { set; get; } = false;

            }
        }
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
            /// 转码默认参数 (应该是带{After}{Before}的ffmpeg参数字符串，如:-y -i {Before} -c copy {After})
            /// 组：Core      默认值：-y -i {Before} -c copy {After}
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
            /// 是否需要初始化
            /// 组：  默认值：
            /// </summary>
            InitializationStatus,
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

        }
```
:::

### `POST /api/System_Resources`
::: details 获取系统硬件资源使用情况
- 私有变量

无

- 注意事项
该接口消耗的系统硬件资源较高，请勿频繁调用！！！！！

- 返回数据说明
```CSharp
    public class SystemResourceClass
    {
        /// <summary>
        /// 平台
        /// </summary>
        public string Platform { set; get; }
        /// <summary>
        /// CPU使用率
        /// </summary>
        public double CPU_usage { set; get; }
        /// <summary>
        /// 内存
        /// </summary>
        public MemInfo Memory { set; get; }
        /// <summary>
        /// 硬盘信息
        /// </summary>
        public List<HDDInfo> HDDInfo { set; get; }
    }

        public class MemInfo
        {
            /// <summary>
            /// 总计内存大小
            /// </summary>
            public long Total { get; set; }
            /// <summary>
            /// 可用内存大小
            /// </summary>
            public long Available { get; set; }
        }

    public class HDDInfo
    {
        /// <summary>
        /// 注册路径
        /// </summary>
        public string FileSystem { set; get; }
        /// <summary>
        /// 硬盘大小
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// 已使用大小
        /// </summary>
        public string Used { get; set; }

        /// <summary>
        /// 可用大小
        /// </summary>
        public string Avail { get; set; }

        /// <summary>
        /// 使用率
        /// </summary>
        public string Usage { get; set; }
        /// <summary>
        /// 挂载路径
        /// </summary>
        public string MountPath { set; get; }
    }
```
:::

### `POST /api/System_Log`
::: details 获取历史日志
- 私有变量

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|page|int|是|第几页|
|Quantity|int|是|每页多少条|

- 返回数据说明
```CSharp
        public class Log
        {
            /// <summary>
            /// 总日志条数
            /// </summary>
            public long TotalLogs { get; set; }
            /// <summary>
            /// 查询量的日志信息
            /// </summary>
            public List<LogClass> Logs { get; set; } = new List<LogClass>();
        }

    public class LogClass
    {
        public enum LogType
        {
            /// <summary>
            /// 会造成整个DDTV无法运行的严重错误
            /// </summary>
            Error = 10,
            /// <summary>
            /// 虽然现在还没发生问题，但是不管这个问题之后肯定会导致严重错误
            /// </summary>
            Error_IsAboutToHappen = 11,
            /// <summary>
            /// 会造成错误，但是不影响运行的警告
            /// </summary>
            Warn = 20,
            /// <summary>
            /// 房间巡逻系统错误日志
            /// </summary>
            Warn_RoomPatrol = 23,
            /// <summary>
            /// 系统一般消息
            /// </summary>
            Info = 30,
            /// <summary>
            /// 转码消息
            /// </summary>
            Info_Transcod=31,
            /// <summary>
            /// API消息
            /// </summary>
            Info_API = 32,
            /// <summary>
            /// IP协议版本消息
            /// </summary>
            Info_IP_Ver = 33,
            /// <summary>
            /// 调试信息
            /// </summary>
            Debug = 40,
            /// <summary>
            /// 调试信息
            /// </summary>
            Debug_Request = 41,
            /// <summary>
            /// DDcenter请求
            /// </summary>
            Debug_DDcenter = 42,
            /// <summary>
            /// 调试信息
            /// </summary>
            Debug_Request_Error = 43,
            /// <summary>
            /// 一些追踪数据
            /// </summary>
            Trace = 50,
            Trace_Web=51,
            TmpInfo=99,
            /// <summary>
            /// 打开所有日志
            /// </summary>
            All = int.MaxValue,
        }
        /// <summary>
        /// 日志内容
        /// </summary>
        public string? Message { set; get; }
        /// <summary>
        /// 日志类型
        /// </summary>
        public LogType Type { set; get; }
        /// <summary>
        /// 系统时间
        /// </summary>
        public DateTime Time { set; get; }
        /// <summary>
        /// 软件的运行时间
        /// </summary>
        public long RunningTime { set; get; }
        /// <summary>
        /// 来源
        /// </summary>
        public string? Source { set; get; }
        /// <summary>
        /// 时候是需要写入txt记录的错误
        /// </summary>
        public bool IsError { set; get; }
        /// <summary>
        /// IsError为真时有效，记录错误详细信息
        /// </summary>
        public Exception exception { set; get; }
        /// <summary>
        /// 是否应该打印到终端
        /// </summary>
        public bool IsDisplay { set; get; } =false;
    }
```
:::

### `POST /api/System_LatestLog`
::: details 获取最新日志
- 私有变量

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|Quantity|int|最新的多少条|

- 返回数据说明
```CSharp
        public class Log
        {
            /// <summary>
            /// 查询量的日志信息
            /// </summary>
            public List<LogClass> Logs { get; set; } = new List<LogClass>();
        }

    public class LogClass
    {
        public enum LogType
        {
            /// <summary>
            /// 会造成整个DDTV无法运行的严重错误
            /// </summary>
            Error = 10,
            /// <summary>
            /// 虽然现在还没发生问题，但是不管这个问题之后肯定会导致严重错误
            /// </summary>
            Error_IsAboutToHappen = 11,
            /// <summary>
            /// 会造成错误，但是不影响运行的警告
            /// </summary>
            Warn = 20,
            /// <summary>
            /// 房间巡逻系统错误日志
            /// </summary>
            Warn_RoomPatrol = 23,
            /// <summary>
            /// 系统一般消息
            /// </summary>
            Info = 30,
            /// <summary>
            /// 转码消息
            /// </summary>
            Info_Transcod=31,
            /// <summary>
            /// API消息
            /// </summary>
            Info_API = 32,
            /// <summary>
            /// IP协议版本消息
            /// </summary>
            Info_IP_Ver = 33,
            /// <summary>
            /// 调试信息
            /// </summary>
            Debug = 40,
            /// <summary>
            /// 调试信息
            /// </summary>
            Debug_Request = 41,
            /// <summary>
            /// DDcenter请求
            /// </summary>
            Debug_DDcenter = 42,
            /// <summary>
            /// 调试信息
            /// </summary>
            Debug_Request_Error = 43,
            /// <summary>
            /// 一些追踪数据
            /// </summary>
            Trace = 50,
            Trace_Web=51,
            TmpInfo=99,
            /// <summary>
            /// 打开所有日志
            /// </summary>
            All = int.MaxValue,
        }
        /// <summary>
        /// 日志内容
        /// </summary>
        public string? Message { set; get; }
        /// <summary>
        /// 日志类型
        /// </summary>
        public LogType Type { set; get; }
        /// <summary>
        /// 系统时间
        /// </summary>
        public DateTime Time { set; get; }
        /// <summary>
        /// 软件的运行时间
        /// </summary>
        public long RunningTime { set; get; }
        /// <summary>
        /// 来源
        /// </summary>
        public string? Source { set; get; }
        /// <summary>
        /// 时候是需要写入txt记录的错误
        /// </summary>
        public bool IsError { set; get; }
        /// <summary>
        /// IsError为真时有效，记录错误详细信息
        /// </summary>
        public Exception exception { set; get; }
        /// <summary>
        /// 是否应该打印到终端
        /// </summary>
        public bool IsDisplay { set; get; } =false;
    }
```
:::



### `POST /api/System_QueryWebFirstStart`
::: details 返回一个可以自行设定的初始化状态值(用于前端自行判断)
- 私有变量

无

- 注意事项
该接口用于前端自行判断，启动后默认值都为真，不能作为DDTV是否正在运行的参考

- 返回数据说明
```CSharp
return bool;//直接指示当前的WEB_FirstStart值为多少

```
:::

### `POST /api/System_SetWebFirstStart`
::: details 设置初始化状态值
- 私有变量

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|state|bool|是|设置初始化状态值|

- 注意事项
用于设置初始化状态值(WEB_FirstStart)；该值无实际的逻辑处理，用于前端自行判断使用。

- 返回数据说明
```CSharp
return MessageBase.Success(nameof(System_Config), state, $"设置初始化标志位为:{state}");
```
:::

### `POST /api/System_QueryUserState`
::: details 用于判断用户登陆状态是否有效
- 私有变量
无

- 注意事项
该接口应该是用于登陆状态是否有效的检测，检测到登陆状态失效就应该停止调用本接口，直到登陆状态恢复
检测登陆中时是否登陆成功，应该使用`/api/LoingState`进行查询

- 返回数据说明
```CSharp
return bool;//直接指示当前的登陆状态

```
:::


### `POST /api/Config_Transcod`
::: details 设置自动转码总开关
- 私有变量

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|state|bool|是|打开或者关闭自动转码开关调用|

- 注意事项
该接口需要依赖ffmpeg，请根据`进阶功能说明`中的`自动转码`页面的内容进行检查是否已经安装ffmpeg

- 返回数据说明
```CSharp
MessageBase.Success(nameof(Config_Transcod), (state ? "打开" : "关闭") + "自动转码成功");
```
:::

### `POST /api/Config_FileSplit`
::: details 根据文件大小自动切片
- 私有变量

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|Size|long|是|设置自动分割文件大小(单位为byte)，为0时为关闭该功能|

- 注意事项
请勿输入1-10485760(1MB)的数值，在某些清晰度较高的直播间中，初始数据包会大于这个数值，这种情况下会报错

- 返回数据说明
```CSharp
MessageBase.Success(nameof(Config_Transcod), (state ? "打开" : "关闭") + "根据文件大小自动切片成功");
```
:::


### `POST /api/Config_DanmuRec`
::: details 弹幕录制总共开关(包括礼物、舰队、SC)
- 私有变量

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|state|bool|是|打开或关闭弹幕录制总开关|

- 注意事项
该弹幕录制接口总共开关包括礼物、舰队、SC的录制开关，并且个房间自己在房间配置列表单独设置，这个只是是否启用弹幕录制功能的总共开关，要录制某个房间除了打开这个设置还需要房间配置启动打开录制

- 返回数据说明
```CSharp
MessageBase.Success(nameof(Config_Transcod), (state ? "打开" : "关闭") + "弹幕录制总共开关成功(注:该弹幕录制接口总共开关包括礼物、舰队、SC的录制开关，并且个房间自己在房间配置列表单独设置，这个只是是否启用弹幕录制功能的总共开关，要录制某个房间除了打开这个设置还需要房间配置启动打开录制)");
```
:::


### `POST /api/Config_GetFollow`
::: details 导入关注列表中的V
- 私有变量

无

- 注意事项
该接口需要依赖哔哩哔哩账号登陆，使用前请确认已经扫码登陆

- 返回数据说明
```CSharp
List<followClass>;

  public class followClass
        {
            public long mid;
            public int roomid;
            public string name;
        }
```
:::

### `POST /api/File_GetAllFileList`
::: details 获取已录制的文件列表
- 私有变量
无

- 返回数据说明
```CSharp
List<string> FileList;
```
:::

### `GET /api/File_GetFile`
::: details 下载对应的文件
- 私有变量

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|FileName|string|是|根据提交的文件路径和文件名下载该文件|


- 返回数据说明
```CSharp
return File();
```
:::

### `POST /api/File_GetFilePathList`
::: details 根据文件树结构返回已录制的文件总列表
- 私有变量

无

- 返回数据说明
```CSharp
        return List<FileNames>;

        public class FileNames
        {
            /// <summary>
            /// 文件名
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 文件类型
            /// </summary>
            public string FileType { get; set; }
            /// <summary>
            /// 文件大小(如果类型是文件夹则为0)
            /// </summary>
            public long Size { get; set; }
            /// <summary>
            /// 文件创建时间
            /// </summary>
            public DateTime DateTime { get; set; }
            /// <summary>
            /// 子文件夹
            /// </summary>
            public List<FileNames> children { get; set; }
        }
```
:::

### `POST /api/File_GetTypeFileList`
::: details 分类获取已录制的文件总列表
- 私有变量
无

- 返回数据说明
```CSharp
    public class TypeFileList
    {
        public List<FileList> fileLists =new List<FileList>();
        public class FileList
        {
            public string Type { set; get; }
            public List<string> files = new List<string>();
        }
    }
```
:::

### `POST /api/Login`
::: details WEB登陆
- 私有变量

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|UserName|string|是|用于登陆的用户名，默认设置为ami，在配置文件中进行设置|
|Password|string|是|用于登陆的密码，默认设置为ddtv，在配置文件中进行设置|

- 返回数据说明
```CSharp
 private class LoginOK
        {
            public string Cookie { get; set; }
        }
```
:::

### `GET /api/loginqr`
::: details 在提示登陆的情况下获取用于的登陆二维码
- 私有变量
无

- 返回数据说明
```CSharp
return File(ms.ToArray(), "image/png");
```
:::

### `POST /api/Login_Reset`
::: details 重新登陆哔哩哔哩账号
- 私有变量

无

- 返回数据说明

直接返回操作结果说明的字符串

:::

### `POST /api/Login_State`
::: details 查询内部登陆状态
- 私有变量

无

- 返回数据说明

```CSharp

        internal class LoginC
        {
            internal LoginStatus LoginState { get; set; }
        }

        public enum LoginStatus
        {
            /// <summary>
            /// 未登录
            /// </summary>
            NotLoggedIn = 0,
            /// <summary>
            /// 已登陆
            /// </summary>
            LoggedIn = 1,
            /// <summary>
            /// 登陆失效
            /// </summary>
            LoginFailure = 2,
            /// <summary>
            /// 登陆中
            /// </summary>
            LoggingIn = 3
        }

```


:::

### `POST /api/Rec_RecordingInfo`
::: details 获取下载中的任务情况详细情况
- 私有变量
无

- 返回数据说明
```CSharp
return List<Downloads>;

        public class Downloads
        {
            public string Token { get; set; }=Guid.NewGuid().ToString("N");
            /// <summary>
            /// 房间号
            /// </summary>
            public string RoomId { get; set; }
            /// <summary>
            /// 用户UID
            /// </summary>
            public long Uid { set; get; }
            /// <summary>
            /// 昵称
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 标题
            /// </summary>
            public string Title { get; set; }
            /// <summary>
            /// FLV大小限制使能
            /// </summary>
            public bool FlvSplit { get; set; } = false;
            /// <summary>
            /// FLV切割大小单位为byte
            /// </summary>
            public long FlvSplitSize { set; get; }
            /// <summary>
            /// 是否下载中
            /// </summary>
            public bool IsDownloading { get; set; }
            /// <summary>
            /// 下载地址
            /// </summary>
            public string Url { get; set; }
            /// <summary>
            /// 下载的完整文件路径
            /// </summary>
            public string FileName { set; get; }
            /// <summary>
            /// 文件夹路径
            /// </summary>
            public string FilePath { set; get; }
            /// <summary>
            /// 开始时间
            /// </summary>
            public DateTime StartTime { set; get; } = DateTime.Now;
            /// <summary>
            /// 结束时间
            /// </summary>
            public DateTime EndTime { set; get; }
            public Tool.FlvModule.FlvClass.FlvTimes flvTimes { set; get; } = new();
            /// <summary>
            /// FLV文件头
            /// </summary>
            public Tool.FlvModule.FlvClass.FlvHeader FlvHeader { set; get; } = new();
            /// <summary>
            /// FLV头脚本数据
            /// </summary>
            public Tool.FlvModule.FlvClass.FlvTag FlvScriptTag { set; get; } = new();
            /// <summary>
            /// WebRequest类的HTTP的实现
            /// </summary>
            public HttpWebRequest HttpWebRequest { get; set; }
            /// <summary>
            /// 当前已下载字节数
            /// </summary>
            public long DownloadCount { get; set; }
            /// <summary>
            /// 该任务下所有任务的总下载字节数
            /// </summary>
            public long TotalDownloadCount { get; set; }
            /// <summary>
            /// 下载状态
            /// </summary>
            public DownloadStatus Status { get; set; } = DownloadStatus.NewTask;
            public enum DownloadStatus
            {
                /// <summary>
                /// 新任务
                /// </summary>
                NewTask,
                /// <summary>
                /// 已准备
                /// </summary>
                Standby,
                /// <summary>
                /// 下载中
                /// </summary>
                Downloading,
                /// <summary>
                /// 下载结束
                /// </summary>
                DownloadComplete,
                /// <summary>
                /// 取消下载
                /// </summary>
                Cancel,
            }
        }
```
:::

### `POST /api/Rec_RecordingInfo_Lite`
::: details 获取下载中的任务情况简略情况
- 私有变量
无

- 返回数据说明
```CSharp
return List<LiteDownloads>;

    public class LiteDownloads
    {
        public string Token { get; set; }
        /// <summary>
        /// 房间号
        /// </summary>
        public string RoomId { get; set; }
        /// <summary>
        /// 用户UID
        /// </summary>
        public long Uid { set; get; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 文件夹路径
        /// </summary>
        public string FilePath { set; get; }
        /// <summary>
        /// 开始时间(秒，Unix时间戳)
        /// </summary>
        public long StartTime { set; get; }
        /// <summary>
        /// 结束时间(秒，Unix时间戳)
        /// </summary>
        public long EndTime { set; get; }
        /// <summary>
        /// 该任务下所有子任务的总下载字节数
        /// </summary>
        public long TotalDownloadCount { get; set; }
    }
```
:::

### `POST /api/Rec_RecordCompleteInfon`
::: details 获取已经完成的任务详细情况
- 私有变量
无

- 返回数据说明
```CSharp
return List<Downloads>;

        public class Downloads
        {
            public string Token { get; set; }=Guid.NewGuid().ToString("N");
            /// <summary>
            /// 房间号
            /// </summary>
            public string RoomId { get; set; }
            /// <summary>
            /// 用户UID
            /// </summary>
            public long Uid { set; get; }
            /// <summary>
            /// 昵称
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 标题
            /// </summary>
            public string Title { get; set; }
            /// <summary>
            /// FLV大小限制使能
            /// </summary>
            public bool FlvSplit { get; set; } = false;
            /// <summary>
            /// FLV切割大小单位为byte
            /// </summary>
            public long FlvSplitSize { set; get; }
            /// <summary>
            /// 是否下载中
            /// </summary>
            public bool IsDownloading { get; set; }
            /// <summary>
            /// 下载地址
            /// </summary>
            public string Url { get; set; }
            /// <summary>
            /// 下载的完整文件路径
            /// </summary>
            public string FileName { set; get; }
            /// <summary>
            /// 文件夹路径
            /// </summary>
            public string FilePath { set; get; }
            /// <summary>
            /// 开始时间
            /// </summary>
            public DateTime StartTime { set; get; } = DateTime.Now;
            /// <summary>
            /// 结束时间
            /// </summary>
            public DateTime EndTime { set; get; }
            public Tool.FlvModule.FlvClass.FlvTimes flvTimes { set; get; } = new();
            /// <summary>
            /// FLV文件头
            /// </summary>
            public Tool.FlvModule.FlvClass.FlvHeader FlvHeader { set; get; } = new();
            /// <summary>
            /// FLV头脚本数据
            /// </summary>
            public Tool.FlvModule.FlvClass.FlvTag FlvScriptTag { set; get; } = new();
            /// <summary>
            /// WebRequest类的HTTP的实现
            /// </summary>
            public HttpWebRequest HttpWebRequest { get; set; }
            /// <summary>
            /// 当前已下载字节数
            /// </summary>
            public long DownloadCount { get; set; }
            /// <summary>
            /// 该任务下所有任务的总下载字节数
            /// </summary>
            public long TotalDownloadCount { get; set; }
            /// <summary>
            /// 下载状态
            /// </summary>
            public DownloadStatus Status { get; set; } = DownloadStatus.NewTask;
            public enum DownloadStatus
            {
                /// <summary>
                /// 新任务
                /// </summary>
                NewTask,
                /// <summary>
                /// 已准备
                /// </summary>
                Standby,
                /// <summary>
                /// 下载中
                /// </summary>
                Downloading,
                /// <summary>
                /// 下载结束
                /// </summary>
                DownloadComplete,
                /// <summary>
                /// 取消下载
                /// </summary>
                Cancel,
            }
        }
```
:::

### `POST /api/Rec_RecordCompleteInfon_Lite`
::: details 获取已经完成的任务简略情况
- 私有变量
无

- 返回数据说明
```CSharp
return List<LiteDownloads>;

    public class LiteDownloads
    {
        public string Token { get; set; }
        /// <summary>
        /// 房间号
        /// </summary>
        public string RoomId { get; set; }
        /// <summary>
        /// 用户UID
        /// </summary>
        public long Uid { set; get; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 文件夹路径
        /// </summary>
        public string FilePath { set; get; }
        /// <summary>
        /// 开始时间(秒，Unix时间戳)
        /// </summary>
        public long StartTime { set; get; }
        /// <summary>
        /// 结束时间(秒，Unix时间戳)
        /// </summary>
        public long EndTime { set; get; }
        /// <summary>
        /// 该任务下所有子任务的总下载字节数
        /// </summary>
        public long TotalDownloadCount { get; set; }
    }
```
:::

### `POST /api/Rec_CancelDownload`
::: details 取消某个下载任务
- 私有变量

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|UID|long|是|要取消任务的账号UID|

- 注意事项
注意！是UID！是UID！

- 返回数据说明
```CSharp
return string;
```
:::

### `POST /api/Room_AllInfo`
::: details 获取房间详细配置信息
- 私有变量
无

- 注意事项
该接口根据服务器上房间配置的多少决定，数据量可能会较多；在启动成功前30秒最好不要调用，该阶段属于API请求更新数据阶段，可能为空的数据较多。

- 返回数据说明
```CSharp
    return List<RoomInfoClass.RoomInfo>;

        public class RoomInfo
        {
            /// <summary>
            /// 标题
            /// </summary>
            public string title { get; set; } = "";
            /// <summary>
            /// 主播简介
            /// </summary>
            public string description { get; set; } = "";
            /// <summary>
            /// 关注数
            /// </summary>
            public int attention { get; set; }
            /// <summary>
            /// 直播间房间号(直播间实际房间号)
            /// </summary>
            public int room_id { get; set; }
            /// <summary>
            /// 主播mid
            /// </summary>
            public long uid { get; set; }
            /// <summary>
            /// 直播间在线人数
            /// </summary>
            public int online { get; set; }
            /// <summary>
            /// 开播时间(未开播时为-62170012800,live_status为1时有效)
            /// </summary>
            public long live_time { get; set; }
            /// <summary>
            /// 直播状态(1为正在直播，2为轮播中)
            /// </summary>
            public int live_status { get; set; }
            /// <summary>
            /// 直播间房间号(直播间短房间号，常见于签约主播)
            /// </summary>
            public int short_id { get; set; }
            /// <summary>
            /// 直播间分区id
            /// </summary>
            public int area { get; set; }
            /// <summary>
            /// 直播间分区名
            /// </summary>
            public string area_name { get; set; } = "";
            /// <summary>
            /// 直播间新版分区id
            /// </summary>
            public int area_v2_id { get; set; }
            /// <summary>
            /// 直播间新版分区名
            /// </summary>
            public string area_v2_name { get; set; } = "";
            /// <summary>
            /// 直播间父分区名
            /// </summary>
            public string area_v2_parent_name { get; set; } = "";
            /// <summary>
            /// 直播间父分区id
            /// </summary>
            public int area_v2_parent_id { get; set; }
            /// <summary>
            /// 用户名
            /// </summary>
            public string uname { get; set; } = "";
            /// <summary>
            /// 主播头像url
            /// </summary>
            public string face { get; set; } = "";
            /// <summary>
            /// 系统tag列表(以逗号分割)
            /// </summary>
            public string tag_name { get; set; } = "";
            /// <summary>
            /// 用户自定义tag列表(以逗号分割)
            /// </summary>
            public string tags { get; set; } = "";
            /// <summary>
            /// 直播封面图
            /// </summary>
            public string cover_from_user { get; set; } = "";
            /// <summary>
            /// 直播关键帧图
            /// </summary>
            public string keyframe { get; set; } = "";
            /// <summary>
            /// 直播间封禁信息
            /// </summary>
            public string lock_till { get; set; } = "";
            /// <summary>
            /// 直播间隐藏信息
            /// </summary>
            public string hidden_till { get; set; } = "";
            /// <summary>
            /// 直播类型(0:普通直播，1：手机直播)
            /// </summary>
            public int broadcast_type { get; set; }
            /// <summary>
            /// 是否p2p
            /// </summary>
            public int need_p2p { set; get; }
            /// <summary>
            /// 是否隐藏
            /// </summary>
            public bool is_hidden { set; get; }
            /// <summary>
            /// 是否锁定
            /// </summary>
            public bool is_locked { set; get; }
            /// <summary>
            /// 是否竖屏
            /// </summary>
            public bool is_portrait { set; get; }
            /// <summary>
            /// 是否加密
            /// </summary>
            public bool encrypted { set; get; }
            /// <summary>
            /// 加密房间是否通过密码验证(encrypted=true时才有意义)
            /// </summary>
            public bool pwd_verified { set; get; }
            /// <summary>
            /// 未知
            /// </summary>
            public int room_shield { set; get; }
            /// <summary>
            /// 是否为特殊直播间(0：普通直播间 1：付费直播间)
            /// </summary>
            public int is_sp { set; get; }
            /// <summary>
            /// 特殊直播间标志(0：普通直播间 1：付费直播间 2：拜年祭直播间)
            /// </summary>
            public int special_type { set; get; }
            /// <summary>
            /// 是否是临时播放项目
            /// </summary>
            public bool IsTemporaryPlay { set; get; } = false;
            /// <summary>
            /// 直播间状态(0:无房间 1:有房间)
            /// </summary>
            public int roomStatus { set; get; }
            /// <summary>
            /// (废弃：请使用live_status)(该值为getRoomInfoOld接口冗余值)直播状态(1为正在直播，2为轮播中)
            /// </summary>
            internal int liveStatus { set; get; }
            /// <summary>
            /// (废弃：请使用cover_from_user(该值为getRoomInfoOld接口冗余值)直播封面图
            /// </summary>
            internal string user_cover { get; set; } = "";
            /// <summary>
            /// 轮播状态(0：未轮播 1：轮播)
            /// </summary>
            public int roundStatus { set; get; }
            /// <summary>
            /// 直播间网页url
            /// </summary>
            public string url { set; get; } = "";
            /// <summary>
            /// 描述(Local值)
            /// </summary>
            public string Description { get; set; } = "";
            /// <summary>
            /// 是否自动录制(Local值)
            /// </summary>
            public bool IsAutoRec { set; get; }
            /// <summary>
            /// 是否开播提醒(Local值)
            /// </summary>
            public bool IsRemind { set; get; }
            /// <summary>
            /// 是否录制弹幕(Local值)
            /// </summary>
            public bool IsRecDanmu { set; get; }
            /// <summary>
            /// 特殊标记(Local值)
            /// </summary>
            public bool Like { set; get; }
            /// <summary>
            /// 用户等级
            /// </summary>
            public int level { set; get; }
            /// <summary>
            /// 主播性别
            /// </summary>
            public string sex { set; get; }
            /// <summary>
            /// 主播简介
            /// </summary>
            public string sign { set; get; }
            /// <summary>
            /// 房间的WS连接对象类
            /// </summary>
            public RoomWebSocket roomWebSocket { set; get; }= new RoomWebSocket();
            /// <summary>
            /// 下载标识符
            /// </summary>
            public bool IsDownload { set; get; } = false;
            /// <summary>
            /// 房间当前下载任务记录
            /// </summary>
            public List<Downloads> DownloadingList { set; get; } = new List<Downloads>();
            /// <summary>
            /// 是否被用户取消操作
            /// </summary>
            public bool IsUserCancel { set; get; }=false;
            /// <summary>
            /// 房间历史下载记录
            /// </summary>
            public List<Downloads> DownloadedLog { set; get; } = new List<Downloads>();
            /// <summary>
            /// 弹幕录制对象
            /// </summary>
            public API.DanMu.DanMuClass.DanmuMessage DanmuFile { set; get; } = new API.DanMu.DanMuClass.DanmuMessage();
            /// <summary>
            /// 是否正在被编辑
            /// </summary>
            public bool IsCliping { set; get; } = false;
            /// <summary>
            /// 该房间当前的任务时间
            /// </summary>
            public DateTime CreationTime { set; get; } = DateTime.Now;
            /// <summary>
            /// 该房间最近一次完成的下载任务的文件信息
            /// </summary>
            public DownloadedFileInfo DownloadedFileInfo { set; get; }=new DownloadedFileInfo();
            /// <summary>
            /// 该房间录制完成后会执行的Shell命令
            /// </summary>
            public string Shell { set; get; } = "";
            /// <summary>
            /// 用于房间监控系统，记录的是监控系统检测到开始直播的时间
            /// </summary>
            public DateTime MonitoringSystem_Airtime = DateTime.Now;
            /// <summary>
            ///  用于房间监控系统，记录开播时的关注数
            /// </summary>
            public int MonitoringSystem_Attention = 0;
            /// <summary>
            /// 当前Host地址
            /// </summary>
            public string Host { set; get; } = "";
            /// <summary>
            /// 当前模式（0:FLV 1:HLS）
            /// </summary>
            public int CurrentMode { set; get; } = 0;
            /// <summary>
            /// 下载的文件记录
            /// </summary>
            public List<DownloadedFiles> Files { set; get; } = new List<DownloadedFiles>();
            public class DownloadedFiles
            {
                public string FilePath { set; get; }
                public bool IsTranscod { set; get; } = false;
            }
        }
        public class RoomWebSocket
        {
            /// <summary>
            /// 是否已连接
            /// </summary>
            public bool IsConnect { set; get; }
            public long dokiTime { set; get; }
            /// <summary>
            /// WbdScket服务器信息
            /// </summary>
            public API.LiveChatScript.LiveChatListener LiveChatListener { set; get; } = new API.LiveChatScript.LiveChatListener();
        }
        public class DownloadedFileInfo
        {
            /// <summary>
            /// 修复后的文件完整路径List
            /// </summary>
            public List<FileInfo> AfterRepairFiles { set; get; } = new List<FileInfo>();
            /// <summary>
            /// 修复前的文件完整路径List
            /// </summary>
            public List<FileInfo> BeforeRepairFiles { set; get; } = new List<FileInfo>();
            /// <summary>
            /// 录制的弹幕文件
            /// </summary>
            public FileInfo DanMuFile { set; get; }
            /// <summary>
            /// 录制的SC记录文件
            /// </summary>
            public FileInfo SCFile { set; get; }
            /// <summary>
            /// 录制的大航海记录文件
            /// </summary>
            public FileInfo GuardFile { set; get; }
            /// <summary>
            /// 录制的礼物记录文件
            /// </summary>
            public FileInfo GiftFile { set; get; }
        }
```
:::


### `POST /api/Room_SummaryInfo`
::: details 获取房间简要配置信息
- 私有变量
无

- 注意事项
该接口根据服务器上房间配置的多少决定，数据量可能会较多；在启动成功前30秒最好不要调用，该阶段属于API请求更新数据阶段，可能为空的数据较多。

- 返回数据说明
```CSharp
return List<RoomInfoClass.RoomInfo>;

        public class RoomInfo
        {
            /// <summary>
            /// 直播间房间号(直播间实际房间号)
            /// </summary>
            public int room_id { get; set; }
            /// <summary>
            /// 主播mid
            /// </summary>
            public long uid { get; set; }
            /// <summary>
            /// 用户名
            /// </summary>
            public string uname { get; set; } = "";
            /// <summary>
            /// 是否自动录制(Local值)
            /// </summary>
            public bool IsAutoRec { set; get; }
            /// <summary>
            /// 是否开播提醒(Local值)
            /// </summary>
            public bool IsRemind { set; get; }
            /// <summary>
            /// 是否录制弹幕(Local值)
            /// </summary>
            public bool IsRecDanmu { set; get; }
            /// <summary>
            /// 特殊标记(Local值)
            /// </summary>
            public bool Like { set; get; }
            /// <summary>
            /// 下载标识符
            /// </summary>
            public bool IsDownload { set; get; } = false;
        }
```
:::

### `POST /api/Room_Add`
::: details 增一个加房间配置
- 私有变量

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|UID|long|是|要增加到房间配置中的账号UID|

- 注意事项
该接口的调用频率不能超过3秒/次，该接口后面封装的阿B原生API较为复杂，如果请求过多，可能会造成频率过高导致412鉴权错误导致IP被黑名单半小时左右。


- 返回数据说明
```CSharp
return string;
```
:::

### `POST /api/Room_Del`
::: details 删除一个房间配置
- 私有变量

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|UID|long|是|要从房间配置中删除的账号UID|

- 返回数据说明
```CSharp
return string;
```
:::

### `POST /api/Room_AutoRec`
::: details 修改房间自动录制配置信息
- 私有变量

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|UID|long|是|要修改自动录制配置的账号UID|
|IsAutoRec|bool|是|打开\关闭开播自动录制|

- 返回数据说明
```CSharp
return string;
```
:::


### `POST /api/Room_DanmuRec`
::: details 修改房间弹幕录制配置信息
- 私有变量

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|UID|long|是|要修改自动录制配置的账号UID|
|IsRecDanmu|bool|是|打开\关闭该房间的弹幕录制功能|

- 注意事项
该功能收到总弹幕录制配置的限制，如要打开该房间的弹幕录制功能，请确认总开关已经启动

- 返回数据说明
```CSharp
return string;
```
:::


### `POST /api/User_Search`
::: details 通过B阿B搜索搜索直播用户
- 私有变量

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|keyword|string|是|需要搜索的关键词|

- 返回数据说明

| 字段        | 类型  | 内容           | 备注                                                   |
| ----------- | ----- | -------------- | ------------------------------------------------------ |
| type        | str   | 结果类型       | 固定为live_user                                        |
| rank_offset | num   | 搜索结果排名值 |                                                        |
| uid         | num   | 主播mid        |                                                        |
| tas         | str   | 直播间TAG      | 多个用`,`分隔                                          |
| live_time   | str   | 开播时间       | YYYY-MM-DD HH:MM:SS<br />如未开播为0000-00-00 00:00:00 |
| hit_columns | array | 关键字匹配类型 |                                                        |
| live_status | num   | 是否开播       | 0：未开播<br />1：已开播                               |
| area        | num   | 1              | **作用尚不明确**                                       |
| is_live     | bool  | 是否开播       | false：未开播<br />true：已开播                        |
| uname       | str   | 主播昵称       | 关键字用xml标签`<em class="keyword">`标注              |
| uface       | str   | 主播头像url    |                                                        |
| rank_index  | num   | 0              | **作用尚不明确**                                       |
| rank_score  | num   | 结果排序量化值 |                                                        |
| attentions  | num   | 主播粉丝数     |                                                        |

:::
