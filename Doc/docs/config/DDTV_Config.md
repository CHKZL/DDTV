# DDTV_Config.ini 通用配置文件

`DDTV_Config.ini` 是 DDTV 的核心配置文件，保存除房间列表以外的全部设置项，在各个版本（Server / Desktop / Client / Docker）中都存在。

## 文件位置与格式

DDTV 启动后会强制把工作目录切换为**程序（exe）所在目录**，因此配置文件的实际位置为：

```
程序所在目录/Config/DDTV_Config.ini
```

- Windows 发布包：与 `DDTV.exe`（或 `DDTV_Server.exe` 等）同级的 `Config` 文件夹内。
- Docker 版：容器内程序目录下的 `./Config/DDTV_Config.ini`（一般已映射为数据卷）。

文件为纯文本，每行一个 `配置项=值`，例如：

```ini
Port=11419
AccessKeyId=ddtv
RecordingMode=1
```

::: warning 注意
- 配置值中**不能包含半角等号 `=`**，否则该项会解析失败被忽略。
- 配置项名（`=`左边的内容）以本文档表格中的名称为准，不要自行增删行。
:::

## 自动保存机制与手动编辑注意事项

- DDTV 运行期间会**每 3 秒检查一次**配置是否有变化，有变化就全量覆写 `DDTV_Config.ini`；房间配置 `RoomListConfig.json` 同理。
- 配置文件**只在启动时读取一次**。运行中直接改文件不仅不会生效，还会在几秒内被程序覆写回去。
- 因此：**如需手动编辑，请先完全关闭 DDTV，修改后再启动**；更推荐通过 WEBUI 设置页或 API 修改（这两种方式立即生效并自动落盘）。
- 名字中包含 `access`（不区分大小写）的配置项（如 `AccessKeySecret`）属于敏感项，**不会**被打印到调试日志中。

## 启动参数

以下参数以 `--参数=值` 的形式附加在启动命令后（如 `DDTV_Server.exe --StartMode=Server --no-update`），优先级高于配置文件：

| 参数 | 取值 | 说明 |
|---|---|---|
| `--StartMode` | `Core` / `Server` / `Docker` / `Client` / `Desktop` | 指定启动模式，用于判断运行环境。各发布版本已内置默认值，一般用户无需关心 |
| `--RecordingMode` | `auto` / `flv_only` / `hls_only` | 启动时指定录制模式，对应配置项 `RecordingMode` 的 1/2/3 |
| `--no-update` | 无值，携带即可 | 本次启动跳过自动更新检查 |
| `--conf` | 配置文件完整路径 | 指定使用其他的配置文件，替代默认的 `./Config/DDTV_Config.ini`（该路径本身不会写入配置文件，需每次启动都携带） |
| `--accept-eula` | 无值，携带即可 | 直接同意用户协议，终端不再弹出确认（适合无人值守/Docker 部署） |

## 网络与 WEB 服务

| 配置项 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Port` | 整数 | `11419` | API/WEBUI 服务监听的端口 |
| `IP` | 字符串 | `http://localhost` | WEB 服务对外地址。应为完整 URL，**必须带协议和端口号**，如 `http://127.0.0.1:11419` |
| `EnableWebServer` | 布尔 | `true` | 是否启用 WEB 服务和端口监听。关闭后 WEBUI 和 API 均不可用 |
| `AccessKeyId` | 字符串 | `ddtv` | API 鉴权使用的 AccessKeyId |
| `AccessKeySecret` | 字符串 | 首次启动随机生成 | API 鉴权使用的 AccessKeySecret。默认值为空，**首次启动时自动生成一个 16 位随机字符串并打印到控制台**（"初次使用，随机生成的AccessKeySecret为：..."），随后写入配置文件。忘记时可查看配置文件或控制台历史输出 |
| `AccessControlAllowOrigin` | 字符串 | `*` | WEB 跨域设置。为 `*` 或完整 URL（必须带协议和端口号，如 `http://127.0.0.1:11419`） |
| `AccessControlAllowCredentials` | 布尔 | `true` | WEB 跨域的 Credentials 设置 |
| `EnableSwagger` | 布尔 | `false` | 是否启用 Swagger API 文档页 |
| `LiveDomainName` | 字符串 | `https://api.live.bilibili.com` | 直播 API 域名，一般不要修改 |
| `MainDomainName` | 字符串 | `https://api.bilibili.com` | 主站 API 域名，一般不要修改 |

### 桌面版远程连接模式（Desktop 专用）

以下配置仅在 `DesktopRemoteServer` 为 `true` 时生效，用于让 Desktop 版连接另一台机器上运行的 DDTV Server：

| 配置项 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `DesktopRemoteServer` | 布尔 | `false` | 桌面版是否连接远程服务器模式 |
| `DesktopIP` | 字符串 | `http://localhost` | 远程服务器地址（完整 URL，带协议和端口） |
| `DesktopPort` | 整数 | `11419` | 远程服务器端口 |
| `DesktopAccessKeyId` | 字符串 | `ddtv` | 连接远程服务器使用的 AccessKeyId |
| `DesktopAccessKeySecret` | 字符串 | `ddtv` | 连接远程服务器使用的 AccessKeySecret（注意默认值为 `ddtv`，需改为远程服务器实际的密钥） |

## B 站账号与登录

| 配置项 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `ValidAccount` | 字符串 | `-1` | 当前生效的账号配置文件编号。扫码登录的凭据本身保存在同目录的加密文件（`BiliUser.ini` / `.Duser` 后缀文件）中，不写入本配置文件 |

## 录制

| 配置项 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `RecordingMode` | 整数 | `1` | 录制模式：`1`=Auto（自动选择）、`2`=FLV_Only（仅 FLV）、`3`=HLS_Only（仅 HLS） |
| `HlsWaitingTime` | 整数（秒） | `30` | 新录制任务等待 HLS 流就绪的最长时间 |
| `DetectIntervalTime` | 整数（毫秒） | `10000` | 直播间开播状态轮询间隔。调小会更及时但更频繁地请求 API，不建议过小 |
| `DefaultResolution` | 整数 | `10000` | 录制默认清晰度。可选值：流畅 `80`、高清 `150`、超清 `250`、蓝光 `400`、原画 `10000`、4K `20000`、杜比 `30000`。实际以直播间提供的清晰度为准，向下取最接近的可用档位 |
| `AutomaticRepair` | 布尔 | `true` | 录制完成后自动进行时间轴修复/封装转码（需要 ffmpeg）。开启时文件以 `_fix.mp4` 结尾，关闭时保留 `_original` 原始文件 |
| `AutomaticRepair_Arguments` | 字符串 | `-y -i "{before}" -c copy "{after}"` | 修复/转码时传给 ffmpeg 的参数模板，`{before}` 和 `{after}` 会被替换为输入输出文件路径 |
| `DeleteOriginalFileAfterRepair` | 布尔 | `true` | 修复成功后删除原始文件 |
| `ForceMerge` | 布尔 | `false` | 整场直播结束后，把自动切割（大小/时间/标题/分辨率/编码参数变化）产生的多个分片强制合并为一个视频文件。手动切割产生的分段不参与合并。**注意：分片间编码参数可能不同，合并必须整体重编码，CPU 开销很大，耗时约等于整场录像时长，低配机器慎用** |
| `ForceMerge_Arguments` | 字符串 | `-y -f concat -safe 0 -i "{list}" -c:v libx264 -preset veryfast -crf 20 -c:a aac -b:a 192k "{after}"` | 强制合并时传给 ffmpeg 的参数模板，`{list}` 会被替换为分片列表文件路径，`{after}` 为输出文件路径。**必须使用重编码参数，不能用 `-c copy` 流拷贝，否则分片编码参数不同会导致花屏**。合并成功后是否删除源分片由 `DeleteOriginalFileAfterRepair` 控制；合并失败时源分片全部保留 |
| `TranscodeFileDifference` | 浮点 | `0.05` | 修复后文件与最终文件可接受的最大体积误差比例（默认 5%）。误差超过该比例时保留原始文件，防止修复后内容丢失 |
| `DetectErroneousFilesFixThem` | 布尔 | `false` | 检测到修复后的文件大小不符合预期时，尝试再次修复 |
| `CutAccordingToSize` | 整数（字节） | `0` | 按文件大小切割视频，`0` 为不切割。例如 1GB 填 `1073741824`。房间列表中的 `RoomCutAccordingToSize` 可单独覆盖本项 |
| `CutAccordingToTime` | 整数（秒） | `0` | 按录制时长切割视频，`0` 为不切割。例如 1 小时填 `3600`。房间列表中的 `RoomCutAccordingToTime` 可单独覆盖本项 |
| `SplitOnTitleChange` | 布尔 | `false` | 直播间标题变更时自动切割视频，并以新标题保存后续分段 |
| `SaveCover` | 布尔 | `false` | 开始录制时保存当前直播间封面 |
| `AutomaticFileCleaningThreshold` | 整数（字节） | `8388608` | 自动清理过小录制文件的阈值（默认 8MB），小于该大小的文件会被视为无效录制自动删除。填 `0` 可关闭该行为 |
| `ReconnectAnchorReStream` | 布尔 | `false` | HLS 录制时检测到分片编号不连贯（主播推流重连等）是否切断重连，避免花屏/时间轴错乱 |
| `UsingCustomFFMPEG` | 字符串 | 空 | 自定义 ffmpeg 路径。为空时使用内置/自动下载的 ffmpeg；要使用系统 ffmpeg 时填 ffmpeg 可执行文件的绝对路径，如 `/usr/bin/ffmpeg` 或 `D:/tools/ffmpeg.exe` |

## 录制文件目录与命名

| 配置项 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `RecFileDirectory` | 字符串 | `./Rec/` | 录制文件保存根目录（相对程序目录，也可填绝对路径） |
| `DefaultLiverFolderName` | 字符串 | `{ROOMID}_{NAME}` | 一级主播文件夹命名格式 |
| `DefaultDataFolderName` | 字符串 | `{YYYY}_{MM}_{DD}` | 二级日期分类文件夹命名格式。置空则不按日期分文件夹 |
| `DefaultFileName` | 字符串 | `{DATE}_{TIME}_{TITLE}` | 录制文件名格式。文件名会根据 `AutomaticRepair` 状态固定以 `_original.mp4` 或 `_fix.mp4` 结尾 |

命名格式可用的关键字（区分大小写）：

| 关键字 | 含义 | 关键字 | 含义 |
|---|---|---|---|
| `{ROOMID}` | 房间号 | `{NAME}` | 主播昵称 |
| `{TITLE}` | 直播标题（自动去除非法文件名字符） | `{R}` | 4 位随机数 |
| `{DATE}` | 日期（`yyyy_MM_dd`） | `{TIME}` | 时间（`HH_mm_ss`） |
| `{YYYY}` / `{YY}` | 年（四位/两位） | `{MM}` / `{DD}` | 月 / 日 |
| `{HH}` / `{mm}` / `{SS}` / `{FFF}` | 时 / 分 / 秒 / 毫秒 | `{CWD}` | 当前录制任务的完整工作目录 |

## 弹幕

| 配置项 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `MaximumLengthDanmu` | 整数 | `40` | 弹幕最大长度限制（字符数），用于弹幕发送与录制处理 |
| `BlockBarrageList` | 字符串 | 空 | 弹幕屏蔽关键词列表，多个关键词用半角竖线 `|` 分隔。命中的弹幕不在播放窗口渲染 |

弹幕是否录制由每个房间的 `IsRecDanmu` 字段控制，详见 [房间配置文件](./RoomListConfig.json.md)。

## 播放窗口（Desktop 版）

| 配置项 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `DefaultPlayResolution` | 整数 | `10000` | 播放窗口默认清晰度，取值同 `DefaultResolution` |
| `PlayWindowDanmaSwitch` | 布尔 | `false` | 打开播放窗口时是否默认开启弹幕 |
| `PlayWindowDanmaSpeed` | 整数 | `8` | 弹幕滚动速度 |
| `PlayDanmaSpeed_Dynamically` | 布尔 | `false` | 弹幕速度是否跟随窗口大小动态变化 |
| `PlayWindowDanmaFontSize` | 整数 | `30` | 弹幕字号 |
| `PlayWindowDanmaColor` | 字符串 | `0xFF,0xFF,0xFF` | 弹幕颜色（R,G,B 十六进制分量） |
| `PlayWindowDanMuFontOpacity` | 浮点 | `0.8` | 弹幕不透明度（0~1） |
| `PlayWindowSubtitleFontSize` | 整数 | `30` | 字幕字号 |
| `PlayWindowSubtitleColor` | 字符串 | `0xFF,0xFF,0xFF` | 字幕颜色（R,G,B 十六进制分量） |
| `PlayWindowWatchHeartbeat` | 布尔 | `false` | 使用 VLC 播放窗口观看直播时，是否向 B 站发送观看心跳以记录观看时长（粉丝勋章亲密度结算依据）。需要已登录账号 |
| `CompatibilityModeDefaultsToOpeningPopupWindow` | 布尔 | `false` | 打开兼容模式播放器（WebView2）时，是否默认打开弹幕窗口方便查看和发送弹幕 |
| `CompatibilityWindowTop` | 布尔 | `false` | 兼容模式播放窗口是否强制置顶 |

## 通知

### WebHook

| 配置项 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `WebHookSwitch` | 布尔 | `false` | WebHook 推送总开关 |
| `WebHookAddress` | 字符串 | 空 | WebHook 推送目标地址，详见高级功能中的 WebHook 说明 |

### SMTP 邮件

| 配置项 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Email_EnableSmtp` | 布尔 | `false` | SMTP 邮件服务总开关 |
| `Email_SmtpServer` | 字符串 | 空 | SMTP 服务器地址，如 `smtp.qq.com` |
| `Email_SmtpPort` | 字符串 | `25` | SMTP 服务器端口（SSL 一般为 `465`） |
| `Email_SmtpSecurity` | 字符串 | `auto` | 连接安全模式：`auto` / `none` / `tls` / `starttls` / `opportunistic` |
| `Email_SmtpUserName` | 字符串 | 空 | SMTP 用户名（一般为邮箱地址） |
| `Email_SmtpPassword` | 字符串 | 空 | SMTP 密码或授权码 |
| `Email_SmtpFrom` | 字符串 | 空 | 发件人地址 |
| `Email_SmtpFromName` | 字符串 | `DDTV` | 发件人显示名 |
| `Email_SmtpTo` | 字符串 | 空 | 收件人地址 |
| `Email_LoginFailureReminder_Enable` | 布尔 | `false` | 登录态失效邮件提醒 |
| `Email_StartLive_Enable` | 布尔 | `false` | 开播邮件提醒（需房间开启开播提醒 `IsRemind`） |
| `Email_RecEnd_Enable` | 布尔 | `false` | 录制结束邮件提醒 |
| `Email_TranscodingFail_Enable` | 布尔 | `false` | 转码失败邮件提醒 |

## Shell（仅 Linux）

| 配置项 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Linux_Only_ShellSwitch` | 布尔 | `false` | 录制完成后执行 Shell 命令的总开关，仅 Linux 生效 |
| `Linux_Only_ShellCommand` | 字符串 | 空 | 全局 Shell 命令模板。房间未单独配置 `Shell` 字段时使用该模板 |

详细用法、关键字列表和注意事项见 [Shell 使用说明](./shell.md)。

## 日志与调试

| 配置项 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `DebugMode` | 布尔 | `false` | 调试模式开关，开启后输出更详细的排查信息 |
| `DurationLogStorage` | 整数（秒） | `2592000` | 日志保留时长（默认 30 天），超期日志自动清理 |

## 桌面版界面与系统（Desktop 专用）

| 配置项 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `ZoomOutMode` | 整数 | `0` | 缩小操作默认行为：`0`=缩小到任务栏、`1`=缩小到托盘后台 |
| `SystemCardReminder` | 布尔 | `true` | 开播时是否触发系统通知卡片提醒 |
| `DesktopWidth` | 整数 | `1250` | 桌面版主窗口宽度（退出时自动记录） |
| `DesktopHeight` | 整数 | `650` | 桌面版主窗口高度（退出时自动记录） |
| `PreventWindowsHibernation` | 布尔 | `false` | 阻止 Windows 休眠，避免录制中途系统睡眠断流 |

## 更新与其他

| 配置项 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `DevelopmentVersion` | 布尔 | `false` | 是否接收开发版（Dev）更新推送 |
| `UseAgree` | 布尔 | `false` | 用户协议同意状态。同意一次后自动置为 `true`，也可用 `--accept-eula` 启动参数跳过确认 |
| `LocalHTTPMode` | 布尔 | `false` | 本地模式是否使用 HTTP 请求（保留项，当前固定按 `false` 处理） |

## 其他高级配置（不建议修改）

以下配置项会自动持久化到配置文件中，但属于内部参数，没有特殊需求请不要改动：

| 配置项 | 默认值 | 说明 |
|---|---|---|
| `Key` | （内置值） | 本地数据 AES 加密密钥，修改后已加密的本地凭据将无法解密 |
| `IV` | （内置值） | AES 加密初始化向量，同上 |
| `HTTP_UA` | Chrome/Edge UA 字符串 | 请求 B 站接口时使用的 User-Agent |
| `DebugFileDirectory` | `./Debug/` | 排查文件（Debug 模式导出）保存目录 |

::: tip
如非特殊需要，请优先通过 WEBUI 设置页或 API 修改配置，避免手动编辑带来的格式问题。
:::
