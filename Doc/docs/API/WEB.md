# WebSocket 推送

DDTV5 的 WEB 服务在同一个端口上提供 WebSocket 推送通道，用于向客户端实时推送运行状态变化（开播、开始录制、录制结束、登录态变化、配置变更等）。WEBUI 的实时刷新就是基于这个通道实现的，你也可以在自己的程序中订阅。

## 连接方式

| 项目 | 说明 |
|:--|:--|
| 地址 | `ws://<IP>:<端口>/ws`（与 HTTP API 同一端口，默认 `ws://127.0.0.1:11419/ws`） |
| 鉴权 | **无鉴权**，也无需携带公共参数，直接建立 WebSocket 连接即可 |
| 独立端口 | 无独立端口，跟随 WEB 服务（`EnableWebServer` 开启时可用） |
| 消息格式 | 文本帧（UTF-8 JSON） |

::: tip 提示
客户端向服务端发送的消息**只会被原样回显（echo）**，服务端目前不处理任何客户端指令——这个通道是单向的「服务端 → 客户端」推送通道，所有操作请走 [HTTP API](./API.md)。
:::

## 推送消息格式

服务端推送的每条消息都是如下 JSON 结构：

```json
{
    "cmd": "StartRecording",
    "code": 40104,
    "data": { "Name": "某某主播", "UID": 672346917, "...": "..." },
    "message": "开始录制"
}
```

| 字段 | 类型 | 说明 |
|:--|:--|:--|
| `cmd` | string | 事件名称（枚举名），与 `code` 一一对应 |
| `code` | int | 事件代码，见下方各分类表 |
| `data` | object / null | 关联的房间卡片对象（RoomCardClass）；与具体房间无关的事件为 `null` |
| `message` | string | 事件描述文本 |

### data 结构（RoomCardClass）

`data` 为 `null` 或一个房间卡片对象，常用字段如下（完整字段以实际推送为准）：

| 字段 | 类型 | 说明 |
|:--|:--|:--|
| `Name` | string | 主播昵称 |
| `UID` | long | 主播 UID |
| `RoomId` | long | 房间号 |
| `Description` | string | 主播简介 |
| `IsAutoRec` | bool | 是否自动录制 |
| `IsRemind` | bool | 是否开播提醒 |
| `IsRecDanmu` | bool | 是否录制弹幕 |
| `AppointmentRecord` | bool | 是否有预约录制 |
| `RoomCutAccordingToSize` / `RoomCutAccordingToTime` | long | 房间维度的切割配置（字节 / 秒） |
| `Title` | object | 直播标题（含 `Value` 字段） |
| `live_status` | object | 直播状态（`Value` 为 1 表示开播） |
| `live_time` | object | 开播时间（Unix 秒） |
| `DownInfo` | object | 下载任务信息（下载大小、速度、状态等） |

### 消息合并机制

服务端推送采用批量合并：同一批消息中**相同 `cmd` 且相同 UID** 的重复推送只会保留最新一条后广播。因此客户端收到的每个事件都是最新状态，但也意味着**不要把该通道当作完整事件日志**，中间态可能被合并丢弃。

另外推送队列容量为 2000 条，队列满时会丢弃最旧的消息；长时间不消费的慢客户端可能错过事件，关键状态请结合 HTTP API 查询兜底。

## 推送事件一览

### Config 配置相关（10101-10106）

| code | cmd | 含义 |
|:--:|:--|:--|
| 10101 | ReadingConfigurationFile | 读取配置文件 |
| 10102 | UpdateToConfigurationFile | 更新到配置文件 |
| 10103 | ReadingRoomFiles | 读取房间文件 |
| 10104 | UpdateToRoomFile | 更新到房间文件 |
| 10105 | ModifyConfiguration | 修改配置 |
| 10106 | UpdateDetect | 检测到 DDTV 新版本 |

### Room 房间相关（20101-20114）

| code | cmd | 含义 |
|:--:|:--|:--|
| 20101 | SuccessfullyAddedRoom | 新增房间配置成功 |
| 20102 | FailedToAddRoomConfiguration | 新增房间配置失败 |
| 20103 | ModifyRoomRecordingConfiguration | 修改房间录制配置 |
| 20104 | ModifyRoomBulletScreenConfiguration | 修改房间弹幕配置 |
| 20105 | ModifyRoomPromptConfiguration | 修改房间提示配置 |
| 20106 | ManuallyTriggeringRecordingTasks | 手动触发录制任务 |
| 20107 | SuccessfullyDeletedRoom | 删除房间成功 |
| 20108 | FailedToDeleteRoom | 删除房间失败 |
| 20109 | CancelRecordingSuccessful | 取消录制成功 |
| 20110 | CancelRecordingFail | 取消录制失败 |
| 20111 | SuccessfullyTriggeredQuickCut | 触发快剪成功 |
| 20112 | TriggerQuickCutFail | 触发快剪失败 |
| 20113 | SuccessfullyAddedRecordingTask | 新增录制任务成功 |
| 20114 | FailedToAddRecordingTask | 新增录制任务失败 |

### Account 账号相关（30101-30110）

| code | cmd | 含义 |
|:--:|:--|:--|
| 30101 | UserConsentAgreement | 用户同意协议 |
| 30102 | UserDoesNotAgreeToAgreement | 用户未同意协议 |
| 30103 | TriggerLoginAgain | 触发重新登录 |
| 30104 | LoginSuccessful | 登录成功 |
| 30105 | UpdateLoginStateCache | 更新登录态缓存 |
| 30106 | InvalidLoginStatus | 登录态失效 |
| 30107 | ScanCodeConfirmation | 扫码登录确认 |
| 30108 | QrCodeWaitingForScann | 二维码等待扫码 |
| 30109 | ScannedCodeWaitingForConfirmation | 已扫码等待确认 |
| 30110 | QrCodeExpir | 二维码已过期 |

### Download 下载相关（40101-40110）

| code | cmd | 含义 |
|:--:|:--|:--|
| 40101 | SaveBulletScreenFile | 保存弹幕相关文件 |
| 40102 | StartLiveEvent | 触发开播事件 |
| 40103 | StartBroadcastingReminder | 开播提醒 |
| 40104 | StartRecording | 开始录制 |
| 40105 | RecordingEnd | 录制结束 |
| 40106 | StopLiveEvent | 停止直播（下播）事件 |
| 40107 | Reconnect | 录制触发重新连接 |
| 40108 | HlsTaskStart | HLS 任务成功开始 |
| 40109 | FlvTaskStart | FLV 任务成功开始 |
| 40110 | EndBroadcastingReminder | 下播提醒 |

## 客户端示例

### JavaScript（浏览器）

```javascript
const ws = new WebSocket("ws://127.0.0.1:11419/ws");

ws.onmessage = (event) => {
    const pack = JSON.parse(event.data);
    console.log(`[${pack.code}] ${pack.cmd}: ${pack.message}`);
    if (pack.code === 40104) {
        console.log(`${pack.data.Name} 开始录制了！`);
    }
};

ws.onopen = () => console.log("已连接 DDTV WebSocket");
ws.onclose = () => console.log("连接已断开");
```

### Python

```python
import json
import websocket  # pip install websocket-client

def on_message(ws, message):
    pack = json.loads(message)
    print(f"[{pack['code']}] {pack['cmd']}: {pack['message']}")

ws = websocket.WebSocketApp(
    "ws://127.0.0.1:11419/ws",
    on_message=on_message,
)
ws.run_forever()
```

## 与 WebHook 的关系

同一份推送消息也会通过 WebHook 以 HTTP POST（`application/json`）发送到配置项 `WebHookAddress` 指定的地址（需 `WebHookSwitch` 开启）。如果你的程序不方便维持 WebSocket 长连接，可以改用 WebHook 被动接收，详见 [WebHook](../AdvancedFeatures/WebHook.md)。
