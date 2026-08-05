# API 接口详情

本文档列出 DDTV5 全部 HTTP API 的详细信息。调用前请先阅读 [API 总览](./README.md)，了解[鉴权机制](./README.md#鉴权机制)、公共参数（`access_key_id` / `time` / `sig`）和[返回包结构](./README.md#返回包结构)。

::: tip 约定
- 下文的参数表**只列出业务参数**，公共参数每个接口都必须携带（免鉴权接口除外），不再重复列出。
- POST 接口的参数放在 form 中提交（`application/x-www-form-urlencoded` 或 `multipart/form-data`），GET 接口放在 query 中。
- 「UID 和房间号二选一」的接口：两个参数至少填一个；都填时优先使用 UID。
- 布尔参数提交 `true` / `false` 字符串。
- 返回示例中 `code` 为 `0` 表示成功，完整状态码见[状态码表](./README.md#状态码)。
:::

## system 系统信息

### get_core_version

获取当前 Core 的版本号。

- 方法/路由：`GET api/system/get_core_version`

参数：无（仅公共参数）

返回示例：

```json
{
    "cmd": "get_core_version",
    "code": 0,
    "data": "5.0.0.0",
    "message": "CoreVersion"
}
```

### get_webui_version

获取当前 WEBUI 版本信息（读取 `./static/version.ini`）。

- 方法/路由：`GET api/system/get_webui_version`

参数：无（仅公共参数）

返回示例：

```json
{
    "cmd": "get_webui_version",
    "code": 0,
    "data": "WEBUI 版本信息文本",
    "message": "WebUIVersion"
}
```

::: warning 注意
WEBUI 版本信息文件不存在时 `data` 为空字符串，`message` 为 `WEBUI版本信息文件不存在`。
:::

### get_system_resources

获取内存和录制路径储存空间使用情况。

- 方法/路由：`GET api/system/get_system_resources`

参数：无（仅公共参数）

返回 `data` 结构：

| 字段 | 类型 | 说明 |
|:--|:--|:--|
| `Platform` | string | 运行平台 |
| `Memory` | object | 内存信息，`Total`（总计字节）、`Available`（可用字节） |
| `HDDInfo` | array | 硬盘信息列表，每项含 `FileSystem`、`Size`、`Used`、`Avail`、`Usage`、`MountPath`（均为字符串） |

返回示例：

```json
{
    "cmd": "get_system_resources",
    "code": 0,
    "data": {
        "Platform": "Win32NT",
        "Memory": { "Total": 34225520640, "Available": 17179869184 },
        "HDDInfo": [
            { "FileSystem": "C:", "Size": "500G", "Used": "60%", "Avail": "200G", "Usage": "300G", "MountPath": "C:" }
        ]
    },
    "message": "SystemResource"
}
```

::: warning 注意
该接口单次执行时间在秒级以上且硬件开销较大，**不推荐频繁调用刷新**；如必须使用，调用间隔建议以分钟为单位。
:::

### generate_debug_file_snapshot

生成 debug 快照文件，用于反馈问题时附带诊断信息。

- 方法/路由：`GET api/system/generate_debug_file_snapshot`

参数：无（仅公共参数）

返回示例（`data` 为生成的快照文件路径）：

```json
{
    "cmd": "generate_debug_file_snapshot",
    "code": 0,
    "data": "./DebugReport/xxxx.zip",
    "message": "GenerateReportSnapshot"
}
```

### get_c

获取当前登录态的 Cookie 字符串（供桌面端播放器使用）。

- 方法/路由：`GET api/system/get_c`

参数：无（仅公共参数）

返回示例：

```json
{
    "cmd": "get_c",
    "code": 0,
    "data": "SESSDATA=xxxx;...",
    "message": ""
}
```

::: danger 警告
该接口返回的是 B 站账号的完整登录 Cookie，**等同于账号凭证**，请勿在不安全的网络环境中暴露。
:::

## config 配置管理

### reload_configuration

重新从配置文件加载配置到内存。

- 方法/路由：`POST api/config/reload_configuration`

参数：无（仅公共参数）

返回示例：

```json
{
    "cmd": "reload_configuration",
    "code": 0,
    "data": true,
    "message": "从配置文件重新加载配置..."
}
```

::: warning 注意
如果修改了路径相关配置，之后调用路径相关接口获取到的都是新配置，可能造成一场直播写到两个路径中的问题。
:::

### set_recording_path

设置录制文件储存路径。**该操作需要二次确认**。

- 方法/路由：`POST api/config/set_recording_path`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `path` | string | 是 | 录制文件储存路径 |
| `check` | string | 否 | 二次确认 key：第一次调用留空，接口返回一个 key；把该 key 作为 `check` 再次提交即生效 |

调用流程：

1. 第一次提交 `path`，返回的 `data` 是一个 GUID 形式的确认 key；
2. 把该 key 作为 `check` 参数连同原参数再次提交，`data` 返回 `true` 表示修改完成；key 不匹配返回 `false`。

::: warning 注意
二次确认提交后，`get_file_structure` 接口以及返回具体文件流的功能将会失效，直到下一次启动。
:::

### get_recording_path

获取录制文件储存路径。

- 方法/路由：`GET api/config/get_recording_path`

参数：无（仅公共参数）

返回示例：

```json
{
    "cmd": "get_recording_path",
    "code": 0,
    "data": "./Rec/",
    "message": "获取录制文件储存路径（字符串）"
}
```

### set_default_file_path_name_format

设置录制储存路径中的子路径和文件名格式。**该操作需要二次确认**（流程同 `set_recording_path`）。

- 方法/路由：`POST api/config/set_default_file_path_name_format`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `default_liver_folder_name` | string | 是 | 默认一级主播名文件夹格式 |
| `default_data_folder_name` | string | 是 | 默认二级主播名下日期分类文件夹格式（可为空） |
| `default_file_name` | string | 是 | 默认下载文件名格式 |
| `check` | string | 否 | 二次确认 key |

三个参数拼接后即为录制文件夹内的最终路径格式：`{default_liver_folder_name}/{default_data_folder_name}/{default_file_name}`。格式支持关键词替换（如 `{YEAR}`、`{MONTH}`、`{ROOMID}` 等），详见配置说明文档。

::: warning 注意
格式有误将导致录制失败或文件异常；二次确认提交后 `get_file_structure` 接口及文件流功能将失效或出现异常，直到下一次启动。
:::

### get_default_file_path_name_format

获取录制储存路径中的子路径和文件名格式。

- 方法/路由：`GET api/config/get_default_file_path_name_format`

参数：无（仅公共参数）

返回示例：

```json
{
    "cmd": "get_default_file_path_name_format",
    "code": 0,
    "data": {
        "Full": "{ROOMID}_{NAME}/{YEAR}-{MONTH}/{YEAR}-{MONTH}-{DATE}_{HOUR}-{MINUTE}",
        "default_liver_folder_name": "{ROOMID}_{NAME}",
        "default_data_folder_name": "{YEAR}-{MONTH}",
        "default_file_name": "{YEAR}-{MONTH}-{DATE}_{HOUR}-{MINUTE}"
    },
    "message": "录制储存路径中的子路径和格式"
}
```

### restore_all_settings_to_default

恢复所有设置为默认值。

- 方法/路由：`GET api/config/restore_all_settings_to_default`

参数：无（仅公共参数）

::: danger 警告
该操作会直接删除配置文件，**需要重新启动程序才生效**，重启后自动生成默认配置。请谨慎调用。
:::

### set_hls_waiting_time

修改 HLS 等待时间。

- 方法/路由：`POST api/config/set_hls_waiting_time`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `waiting_time` | int | 是 | 等待时间（单位：秒） |

### get_hls_waiting_time

获取 HLS 等待时间。

- 方法/路由：`GET api/config/get_hls_waiting_time`

参数：无（仅公共参数）。`data` 返回 int，单位秒。

### set_automatic_repair

设置自动修复状态（录制文件写入完成时是否自动进行时间轴修复）。

- 方法/路由：`POST api/config/set_automatic_repair`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `automatic_repair` | bool | 是 | 文件写入完成时是否自动修复 |

### get_automatic_repair

获取自动修复设置状态。

- 方法/路由：`GET api/config/get_automatic_repair`

参数：无（仅公共参数）。`data` 返回 bool。

### reinitialize

重新初始化：**清空所有现有配置文件（用户登录态、房间配置、主配置），3 秒后程序自动结束运行**。需要二次确认（流程同 `set_recording_path`）。

- 方法/路由：`POST api/config/reinitialize`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `check` | string | 否 | 二次确认 key |

::: danger 警告
该操作不可逆，执行后需自行重新启动进程并重新登录、重新配置。请谨慎调用。
:::

### set_cut_according_time

全局设置根据录制时长切割视频文件的时长。

- 方法/路由：`POST api/config/set_cut_according_time`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `cut_time` | long | 是 | 切割时长（单位：秒），0 表示不按时长切割 |

### set_cut_according_size

全局设置根据录制文件大小切割视频文件。

- 方法/路由：`POST api/config/set_cut_according_size`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `size` | long | 是 | 切割大小（单位：字节），0 表示不按大小切割 |

### get_cut_according_config

获取全局的切割配置。

- 方法/路由：`GET api/config/get_cut_according_config`

参数：无（仅公共参数）

返回示例：

```json
{
    "cmd": "get_cut_according_config",
    "code": 0,
    "data": { "size": 2147483648, "time": 3600 },
    "message": ""
}
```

### set_room_cut_according_to_size

房间维度设置根据录制文件大小切割视频文件（覆盖全局配置）。

- 方法/路由：`POST api/config/set_room_cut_according_to_size`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `size` | long | 是 | 切割大小（单位：字节），必须大于 0 |
| `uid` | long | 否 | 用户 UID（与 `roomid` 至少填一个） |
| `roomid` | long | 否 | 房间号 |

参数不合法时 `code` 返回 `7000`。

### set_room_cut_according_to_time

房间维度设置根据录制时长切割视频文件（覆盖全局配置）。

- 方法/路由：`POST api/config/set_room_cut_according_to_time`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `cut_time` | long | 是 | 切割时长（单位：秒），必须大于 0 |
| `uid` | long | 否 | 用户 UID（与 `roomid` 至少填一个） |
| `roomid` | long | 否 | 房间号 |

### get_room_cut_according_config

获取房间维度的切割配置。

- 方法/路由：`POST api/config/get_room_cut_according_config`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `uid` | long | 否 | 用户 UID（与 `roomid` 至少填一个） |
| `roomid` | long | 否 | 房间号 |

返回示例：

```json
{
    "cmd": "get_room_cut_according_config",
    "code": 0,
    "data": { "size": 2147483648, "time": 3600 },
    "message": ""
}
```

## rec_task 录制任务

### single_task

手动增加一个录制任务（UID 和房间号二选一）。如果直播还未开始，则预约下一场开始的直播（如果主播中途下播再上播则不会再次录制）。

- 方法/路由：`POST api/rec_task/single_task`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `uid` | long | 否 | 用户 UID |
| `room_id` | long | 否 | 房间号 |

返回示例：

```json
{
    "cmd": "single_task",
    "code": 0,
    "data": true,
    "message": "任务增加成功"
}
```

`data` 为 bool，表示操作是否成功；失败原因见 `message`。

### cancel_task

取消录制任务（UID 和房间号二选一）。

- 方法/路由：`POST api/rec_task/cancel_task`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `uid` | long | 否 | 用户 UID |
| `room_id` | long | 否 | 房间号 |

`data` 为 bool，表示操作是否成功。

### cut_task

手动切断并重连当前录制任务（UID 和房间号二选一）。

- 方法/路由：`POST api/rec_task/cut_task`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `uid` | long | 否 | 用户 UID |
| `room_id` | long | 否 | 房间号 |

::: warning 注意
网络状态差或硬盘较慢的环境下不推荐调用。
:::

### generate_snapshot

创建直播间快照，用于快速切片（剪辑）。

- 方法/路由：`POST api/rec_task/generate_snapshot`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `uid` | long | 否 | 用户 UID |

成功时 `code` 为 `0`、`message` 为「创建快照成功」；失败时 `code` 为 `7000`。

## get_rooms 房间查询

### room_statistics

获取当前房间监控列表的统计。

- 方法/路由：`POST api/get_rooms/room_statistics`

参数：无（仅公共参数）

返回示例：

```json
{
    "cmd": "room_statistics",
    "code": 0,
    "data": {
        "MonitoringCount": 10,
        "LiveCount": 3,
        "RecCount": 2
    },
    "message": ""
}
```

| data 字段 | 类型 | 说明 |
|:--|:--|:--|
| `MonitoringCount` | int | 监控中的房间总数 |
| `LiveCount` | int | 当前开播中的房间数 |
| `RecCount` | int | 当前录制中的房间数 |

### room_information

查询单个房间信息（UID 或房间号二选一）。

- 方法/路由：`POST api/get_rooms/room_information`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `uid` | long | 否 | 用户 UID |
| `room_id` | long | 否 | 房间号 |

`data` 返回该房间的完整卡片对象（RoomCardClass），包含昵称、UID、房间号、自动录制/开播提醒/弹幕录制开关、直播状态、标题、下载状态等。房间不存在时 `code` 为 `7000`、`data` 为 `false`。

### batch_complete_room_information

批量获取配置中房间的完整信息。

- 方法/路由：`POST api/get_rooms/batch_complete_room_information`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `quantity` | int | 否 | 分页后每页数量，默认或传 0 为全部 |
| `page` | int | 否 | 获取的页数（从 1 开始），当 `quantity` 不为 0 时有效 |
| `type` | int | 否 | 返回数据类型：0 全部 / 1 录制中 / 2 开播中 / 3 未开播 / 4 开播但未录制 / 5 提醒状态开 / 6 全部按原始字母顺序排列 |
| `screen_name` | string | 否 | 只返回昵称包含该字符串的房间 |

返回 `data` 结构：

| 字段 | 类型 | 说明 |
|:--|:--|:--|
| `total` | int | 筛选后房间总数 |
| `completeInfoList` | array | 房间完整信息列表 |

`completeInfoList` 每项结构：

| 字段 | 说明 |
|:--|:--|
| `uid` / `roomId` | 用户 UID / 房间号 |
| `userInfo` | 用户信息：`name`、`description`、`uid`、`isAutoRec`、`isRemind`、`isRecDanmu`、`sex`、`sign`、`appointmentRecord`（是否有预约录制）、`RoomCutAccordingToTime`、`RoomCutAccordingToSize` |
| `roomInfo` | 直播间信息：`roomId`、`title`、`attention`、`liveTime`、`liveStatus`、`shortId`、`areaName`、`face`、`tags`、`coverFromUser`、`keyFrame`、`url`、`specialType` |
| `taskStatus` | 任务状态：`isDownload`、`downloadSize`、`downloadRate`、`status`（下载状态枚举，见下）、`startTime`、`endTime`、`title`、`isDanma` |

### batch_basic_room_information

批量获取配置中房间的基本信息（轻量版，适合列表刷新）。

- 方法/路由：`POST api/get_rooms/batch_basic_room_information`

参数与 `batch_complete_room_information` 相同。

返回 `data` 结构：

| 字段 | 类型 | 说明 |
|:--|:--|:--|
| `total` | int | 筛选后房间总数 |
| `currentPage` | int | 当前页码 |
| `basicInfoList` | array | 房间基本信息列表 |

`basicInfoList` 每项结构：

| 字段 | 说明 |
|:--|:--|
| `uid` / `roomId` | 用户 UID / 房间号 |
| `userInfo` | `name`、`uid`、`appointmentRecord` |
| `roomInfo` | `roomId`、`title`、`liveStatus`、`specialType` |
| `taskStatus` | `isDownload`、`downloadSize`、`status` |

::: tip 下载状态枚举（`status`）
| 值 | 含义 |
|:--:|:--|
| 0 | 新任务 NewTask |
| 1 | 已准备 Standby |
| 2 | 下载中 Downloading |
| 3 | 下载结束 DownloadComplete |
| 4 | 取消下载中 Cancel |
| 5 | 特殊状态（大航海/门票等收费无权限） |
:::

## set_rooms 房间管理

### add_room

添加房间（UID 和房间号二选一）。

- 方法/路由：`POST api/set_rooms/add_room`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `auto_rec` | bool | 是 | 是否自动录制 |
| `remind` | bool | 是 | 是否开播提醒 |
| `rec_danmu` | bool | 是 | 是否录制弹幕 |
| `uid` | long | 否 | 用户 UID |
| `room_id` | long | 否 | 房间号 |

返回示例：

```json
{
    "cmd": "add_room",
    "code": 0,
    "data": true,
    "message": "新增房间配置成功"
}
```

`data` 为 bool，表示是否添加成功；失败原因见 `message`。

### batch_add_room

批量增加房间。

- 方法/路由：`POST api/set_rooms/batch_add_room`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `uids` | string | 是 | 使用英文半角逗号分隔的 UID 字符串，如 `672346917,672328094` |
| `auto_rec` | bool | 是 | 是否自动录制 |
| `remind` | bool | 是 | 是否开播提醒 |
| `rec_danmu` | bool | 是 | 是否录制弹幕 |

`data` 返回每个 UID 的添加结果。

### del_room

删除房间（UID 和房间号二选一）。

- 方法/路由：`POST api/set_rooms/del_room`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `uid` | long | 否 | 用户 UID |
| `room_id` | long | 否 | 房间号 |

`data` 为 bool，表示是否删除成功。

### batch_delete_rooms

批量删除房间。

- 方法/路由：`POST api/set_rooms/batch_delete_rooms`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `uids` | string | 是 | 使用英文半角逗号分隔的 UID 字符串 |

### modify_recording_settings

批量修改房间的自动录制设置。

- 方法/路由：`POST api/set_rooms/modify_recording_settings`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `uid` | long 列表 | 是 | 要修改的房间 UID 列表（重复提交多个同名 `uid` 参数，或用英文逗号拼接为一个值） |
| `state` | bool | 是 | 目标自动录制状态 |

`data` 返回成功修改的 UID 列表（long 数组）。

### modify_room_prompt_settings

批量修改房间的开播提醒设置。参数与返回值同 `modify_recording_settings`。

- 方法/路由：`POST api/set_rooms/modify_room_prompt_settings`

### modify_room_dm_settings

批量修改房间的弹幕录制设置。参数与返回值同 `modify_recording_settings`。

- 方法/路由：`POST api/set_rooms/modify_room_dm_settings`

### modify_room_settings

修改单个房间配置。

- 方法/路由：`POST api/set_rooms/modify_room_settings`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `uid` | long | 是 | 要修改的房间 UID |
| `AutoRec` | bool | 是 | 是否开播自动录像 |
| `Remind` | bool | 是 | 是否开播提醒 |
| `RecDanmu` | bool | 是 | 是否录制弹幕（打开录像才生效） |

`data` 为 bool，表示是否修改成功。

## file 文件

### get_file_structure

获取录制文件夹下的文件结构，以 JSON 树返回。

- 方法/路由：`POST api/file/get_file_structure`

参数：无（仅公共参数）

返回 `data` 结构（DirectoryNode 树）：

| 字段 | 类型 | 说明 |
|:--|:--|:--|
| `Name` | string | 文件/文件夹名 |
| `Type` | string | 类型（文件或文件夹） |
| `Size` | long / null | 文件大小（字节），文件夹为 null |
| `RelativePath` | string | 相对路径 |
| `Extension` | string | 扩展名 |
| `Children` | array | 子节点列表（DirectoryNode 递归） |

目标路径不存在时 `data` 为 `false`，`message` 为「目标路径不存在」。

::: tip 提示
录制目录同时被映射为 WEB 静态路径 `/rec_file`，拿到 `RelativePath` 后可直接通过 `http(s)://<IP>:<端口>/rec_file/<RelativePath>` 播放或下载文件。
:::

## login 登录

### get_login_qr（免鉴权）

获取登录二维码，返回 **PNG 图片流**（不是 JSON）。二维码文件未生成时会最多等待约 3 秒；超时返回 JSON 错误包。

- 方法/路由：`GET api/login/get_login_qr`

参数：无（免鉴权）

::: warning 注意
二维码有有效期，需先由程序处于待登录状态（首次启动未登录，或调用 `re_login` 后）才能获取到。
:::

### get_login_url（免鉴权）

获取用于生成登录二维码的 URL 字符串，可自行生成二维码展示。

- 方法/路由：`GET api/login/get_login_url`

参数：无（免鉴权）

返回示例：

```json
{
    "cmd": "get_login_url",
    "code": 0,
    "data": "https://passport.bilibili.com/h5-app/passport/login/scan?navhide=1&qrcode_key=xxxx&from=",
    "message": "获取用于生成登陆二维码的URL字符串"
}
```

### re_login

重新登录（触发后会覆盖当前登录态，覆盖前当前登录态依旧有效）。

- 方法/路由：`POST api/login/re_login`

参数：无（仅公共参数）

调用后请在 1 分钟内通过 `get_login_qr` 或 `get_login_url` 获取二维码并扫码登录。

### get_login_status

获取本地登录态（AccountInformation）的有效状态。

- 方法/路由：`POST api/login/get_login_status`

参数：无（仅公共参数）。`data` 返回 bool。

### get_nav

获取登录用户基本信息（B站 nav 接口数据）。

- 方法/路由：`GET api/login/get_nav`

参数：无（仅公共参数）

返回示例：

```json
{
    "cmd": "get_nav",
    "code": 0,
    "data": { "mid": 672346917 },
    "message": ""
}
```

### use_agree

同意用户协议（使用须知）。

- 方法/路由：`POST api/login/use_agree`

| 参数 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `check` | string | 否 | 传 `y` 表示同意；其他值（默认 `n`）表示不同意 |

同意时 `data` 为 `true`；不同意时 `code` 为 `6000`、`data` 为 `false`。

### use_agree_state

获取用户初始化授权（用户协议同意）状态。

- 方法/路由：`POST api/login/use_agree_state`

参数：无（仅公共参数）。`data` 返回 bool。

## 其他

### dokidoki

请求当前运行心跳信息。

- 方法/路由：`GET api/dokidoki` 或 `POST api/dokidoki`

参数：无（仅公共参数）

返回 `data` 结构：

| 字段 | 类型 | 说明 |
|:--|:--|:--|
| `Total` | int | 监控房间总数 |
| `Downloading` | int | 录制中任务数 |
| `UsingMemory` | long | 当前使用内存（字节） |
| `UsingMemoryStr` | string | 当前使用内存（可读字符串） |
| `InitType` | string | 初始化类型 |
| `Ver` | string | 版本号 |
| `CompiledVersion` | string | 编译时间 |
| `CompilationMode` | string | 编译模式 |
| `StartMode` | int | 启动模式枚举 |

### init_inspect（免鉴权）

用于检测 WEB 服务是否初始化完成。

- 方法/路由：`GET api/init_inspect`

参数：无（免鉴权）

返回示例：

```json
{
    "cmd": "init_inspect",
    "code": 0,
    "data": "OK",
    "message": "OK"
}
```

### unauthorized（免鉴权）

鉴权失败时的跳转地址，返回文本 `HTTP 401`。一般不需要直接调用。

- 方法/路由：`GET api/unauthorized`

### not_found（免鉴权）

404 页面地址。存在 `./static/404.html` 时返回该页面，否则返回内置 404 图片。一般不需要直接调用。

- 方法/路由：`GET api/not_found`
