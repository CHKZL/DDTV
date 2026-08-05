# 房间配置文件 RoomListConfig.json

`RoomListConfig.json` 保存监控房间列表及每个房间的独立设置（是否自动录制、是否录弹幕、房间级切割、Shell 命令等）。

## 文件位置

```
程序所在目录/Config/RoomListConfig.json
```

（即与 `DDTV_Config.ini` 同目录。DDTV 启动时会把工作目录强制切换为程序所在目录，因此就是 exe 同级的 `Config` 文件夹。）

- 文件不存在时，DDTV 启动会自动创建并写入空 JSON `{}`。
- 如果文件内容不是合法的 JSON，DDTV 启动时会抛出解析异常并可能无法启动，**请务必先备份再手动编辑**。

## 保存机制与手动编辑注意事项

- DDTV 运行期间**每 3 秒检查一次**房间配置是否有变化，有变化就全量覆写该文件（格式化输出、中文不转义）。
- 文件**只在启动时读取一次**。运行中手动修改不会生效，还会在几秒内被覆写回去。
- 因此手动编辑前请**完全关闭 DDTV**，编辑完成后校验 JSON 合法性再启动。
- 推荐做法：通过 WEBUI / API 管理房间，或使用 WEBUI 提供的**房间列表导入功能**从 JSON 文件批量导入（导入时 `UID` 必须大于 0，已存在的 UID 会计为重复跳过）。

## 文件结构

整个文件是一个对象，`data` 数组中每个元素是一个房间的配置：

```json
{
  "data": [
    {
      "name": "未来明-MiraiAkari",
      "Description": "",
      "RoomId": 6792401,
      "UID": 238537745,
      "IsAutoRec": true,
      "IsRemind": false,
      "IsRecDanmu": true,
      "Like": false,
      "Shell": "",
      "AppointmentRecord": false,
      "RoomCutAccordingToSize": 0,
      "RoomCutAccordingToTime": 0
    },
    {
      "name": "AIChannel官方",
      "Description": "A.I.Channel",
      "RoomId": 1485080,
      "UID": 1473830,
      "IsAutoRec": false,
      "IsRemind": false,
      "IsRecDanmu": false,
      "Like": false,
      "Shell": "",
      "AppointmentRecord": false,
      "RoomCutAccordingToSize": 4294967296,
      "RoomCutAccordingToTime": 0
    }
  ]
}
```

## 字段说明

| 字段 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `name` | 字符串 | `""` | 主播昵称（DDTV 会自动从 B 站更新） |
| `Description` | 字符串 | `""` | 备注，仅自己可见 |
| `RoomId` | 整数 | `-1` | 直播间房间号（长号） |
| `UID` | 整数 | `-1` | 主播账号 UID（mid）。**这是房间的唯一标识**，手动添加房间时必须填写正确且大于 0 |
| `IsAutoRec` | 布尔 | `false` | 开播后是否自动录制 |
| `IsRemind` | 布尔 | `false` | 开播后是否触发提醒（Desktop 系统通知、邮件提醒等，需配合对应通知开关使用） |
| `IsRecDanmu` | 布尔 | `false` | 是否录制该房间的弹幕（含 SC、礼物、大航海记录） |
| `Like` | 布尔 | `false` | 特别关注标记，用于在界面中标记/筛选，不影响录制行为 |
| `Shell` | 字符串 | `""` | 该房间录制结束后执行的 Shell 命令（仅 Linux，需总开关开启）。优先级高于全局 `Linux_Only_ShellCommand`，详见 [Shell 使用说明](./shell.md) |
| `AppointmentRecord` | 布尔 | `false` | 预约下一场录制：即使未开启自动录制，下次开播也会触发一次录制。取消录制任务时该标记会被一并清除 |
| `RoomCutAccordingToSize` | 整数（字节） | `0` | 仅对本房间生效的按大小切割阈值。`0` 表示跟随全局配置 `CutAccordingToSize`；大于 0 时覆盖全局设置。例如 4GB 填 `4294967296` |
| `RoomCutAccordingToTime` | 整数（秒） | `0` | 仅对本房间生效的按时长切割阈值。`0` 表示跟随全局配置 `CutAccordingToTime`；大于 0 时覆盖全局设置。例如 1 小时填 `3600` |

::: danger 警告
手动编辑后请务必校验 JSON 格式合法性（可使用任意在线 JSON 校验工具）。格式错误会导致 DDTV 启动时解析失败，房间列表无法加载！
:::
