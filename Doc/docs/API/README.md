# API 总览

DDTV5 的 Server（以及内嵌了 Server 的 Desktop / Client）内置了一套 HTTP API 和 WebSocket 推送服务，用于二次开发和远程管理。本文档面向想对接 DDTV API 的开发者，包含鉴权机制、公共参数、返回包结构和全部接口目录。

::: tip 快速开始
如果你只想尽快跑通第一个请求，看这里：

1. 启动 DDTV Server，确认 WEB 服务已开启（默认监听 `http://localhost:11419`）。
2. 打开 `./Config/DDTV_Config.ini`，找到 `AccessKeyId`（默认 `ddtv`）和 `AccessKeySecret`（首次启动随机生成并打印到控制台）。
3. 按下面[鉴权机制](#鉴权机制)的算法计算 `sig`，带上公共参数请求任意接口即可，例如：

```bash
curl "http://127.0.0.1:11419/api/system/get_core_version?access_key_id=ddtv&time=1754400000&sig=计算出的签名"
```

一个可以直接复制运行的 Python 示例见 [Python 示例](#python-示例)。
:::

## 服务基本信息

| 项目 | 说明 |
|:--|:--|
| 默认监听地址 | `http://localhost:11419`（由配置项 `IP` + `Port` 控制） |
| 相关配置 | `./Config/DDTV_Config.ini` 中的 `EnableWebServer`（默认 `true`）、`IP`、`Port` |
| API 路由格式 | `api/<分组>/<接口名>`，例如 `api/system/get_core_version` |
| 请求方式 | 仅支持 GET / POST；POST 参数放 form（`application/x-www-form-urlencoded` 或 `multipart/form-data`），GET 参数放 query |
| WebSocket | 挂在同一端口的 `/ws` 路径，详见 [WebSocket 推送](./WEB.md) |

## 鉴权机制

除少数免鉴权接口外，所有接口都要求通过 `sig` 签名校验（代码实现见 `Server/WebAppServices/Api/InterfaceAuthentication.cs` 的 `LoginAttribute`）。

### 免鉴权接口

以下接口**不需要**签名，直接调用即可：

| 接口 | 说明 |
|:--|:--|
| `GET api/init_inspect` | 检测 WEB 服务是否初始化完成 |
| `GET api/login/get_login_qr` | 获取登录二维码图片 |
| `GET api/login/get_login_url` | 获取用于生成登录二维码的 URL 字符串 |
| `GET api/unauthorized` | 鉴权失败跳转地址（内部使用） |
| `GET api/not_found` | 404 页面（内部使用） |

### 公共参数

除免鉴权接口外，每个请求都必须携带以下三个公共参数（和业务参数一起放在 form 或 query 中）：

| 参数名 | 类型 | 必填 | 说明 |
|:--|:--:|:--:|--|
| `access_key_id` | string | 是 | 配置文件中的 `AccessKeyId`，默认 `ddtv` |
| `time` | long | 是 | 发起请求时的 **Unix 时间戳（单位：秒）**，与服务器时间相差超过 300 秒的请求会被拒绝 |
| `sig` | string | 是 | 按下方算法计算出的签名，**40 位小写十六进制字符串** |

### sig 计算步骤

1. 收集本次请求的**全部业务参数**（例如 `uid=123456`），加上公共参数 `access_key_id` 和 `time`，**不要**包含 `sig` 本身。
2. 再补上一项 `access_key_secret=<配置文件中的AccessKeySecret>`。注意：**`access_key_secret` 只参与签名计算，绝不能作为参数提交**——请求中一旦携带 `access_key_secret` 参数会直接鉴权失败。
3. 把所有参数按**参数名的字典序升序**排列，每项拼成 `参数名小写=参数值`，用英文分号 `;` 连接（**结尾没有分号**），得到待签名原串。
   - 参数值使用你实际提交的原始字符串（例如布尔值 `true`）。
   - 同名参数传了多个值（如批量接口的 `uid`）时，将所有值用英文逗号连接后作为该参数的值参与签名，例如 `uid=1,2,3`。
4. 对原串做 **SHA1（UTF-8 编码）**，取**小写**十六进制字符串，即为 `sig`。
5. 把 `access_key_id`、`time`、`sig` 和业务参数一起提交（GET 放 query，POST 放 form）。

#### 计算示例

假设：

- `AccessKeyId = ddtv`，`AccessKeySecret = abc123`
- 调用 `POST api/rec_task/single_task`，业务参数 `uid=672346917`
- `time = 1754400000`

待签名原串为：

```text
access_key_id=ddtv;access_key_secret=abc123;time=1754400000;uid=672346917
```

对该字符串做 SHA1 后得到的小写十六进制哈希即为 `sig`。

#### Python 示例

```python
import hashlib
import time
import requests

BASE_URL = "http://127.0.0.1:11419"
ACCESS_KEY_ID = "ddtv"
ACCESS_KEY_SECRET = "替换成你配置文件里的AccessKeySecret"


def signed_params(params: dict) -> dict:
    """给参数附加公共参数并计算sig"""
    params = dict(params)
    params["access_key_id"] = ACCESS_KEY_ID
    params["time"] = int(time.time())
    # access_key_secret 仅参与签名，不随请求发送
    raw_items = list(params.items()) + [("access_key_secret", ACCESS_KEY_SECRET)]
    raw = ";".join(f"{k.lower()}={v}" for k, v in sorted(raw_items))
    params["sig"] = hashlib.sha1(raw.encode("utf-8")).hexdigest()  # 小写
    return params


# 示例：为 UID 672346917 的房间手动增加一个录制任务
data = signed_params({"uid": 672346917})
resp = requests.post(f"{BASE_URL}/api/rec_task/single_task", data=data)
print(resp.json())
```

#### C# 示例

```csharp
using System.Security.Cryptography;
using System.Text;

static Dictionary<string, string> SignedParams(Dictionary<string, string> param)
{
    const string AccessKeyId = "ddtv";
    const string AccessKeySecret = "替换成你配置文件里的AccessKeySecret";

    param["access_key_id"] = AccessKeyId;
    param["time"] = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

    var rawItems = param
        .Append(KeyValuePair.Create("access_key_secret", AccessKeySecret))
        .OrderBy(kv => kv.Key);
    string raw = string.Join(";", rawItems.Select(kv => $"{kv.Key.ToLower()}={kv.Value}"));

    using var sha1 = SHA1.Create();
    byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(raw));
    param["sig"] = Convert.ToHexString(hash).ToLower(); // 小写
    return param;
}

// 使用示例：POST form 请求
var p = SignedParams(new Dictionary<string, string> { ["uid"] = "672346917" });
using var http = new HttpClient();
var resp = await http.PostAsync(
    "http://127.0.0.1:11419/api/rec_task/single_task",
    new FormUrlEncodedContent(p));
Console.WriteLine(await resp.Content.ReadAsStringAsync());
```

### 鉴权失败的行为

出现以下任一情况时请求会被拒绝，直接返回 **HTTP 401** 状态码（响应体为文本 `HTTP 401`）：

- 请求方法不是 GET / POST；
- 缺少 `sig` / `access_key_id` / `time` 中任意一个；
- 携带了 `access_key_secret` 参数；
- `access_key_id` 与配置文件不一致；
- `time` 不是有效的 Unix 秒时间戳，或与服务器时间相差超过 300 秒；
- `sig` 校验不通过。

::: warning 注意
签名比较是**大小写敏感**的，服务端计算结果为 SHA1 的**小写**十六进制，请确保提交的 `sig` 为小写。另外请保证调用方机器与 DDTV 服务器的系统时间一致（`time` 有效期只有 300 秒）。
:::

## 返回包结构

所有 API 接口（`get_login_qr` 返回图片除外）统一返回如下 JSON 结构：

```json
{
    "cmd": "single_task",
    "code": 0,
    "data": true,
    "message": "任务增加成功"
}
```

| 字段 | 类型 | 说明 |
|:--|:--|:--|
| `cmd` | string | 本次请求的接口名称 |
| `code` | int | 状态码，见下表 |
| `data` | any | 返回数据主体，结构因接口而异（对象 / 数组 / 布尔 / 字符串 / null） |
| `message` | string | 提示文本消息 |

### 状态码

| code | 名称 | 含义 |
|:--:|:--|:--|
| 0 | ok | 请求成功 |
| 5000 | ParameterError | 参数有误 |
| 6000 | LoginInfoFailure | 登录信息失效（B站登录态无效） |
| 7000 | OperationFailed | 操作失败 |
| 10101 | ReadingConfigurationFileComplet | 读取配置文件完成 |

::: warning 注意
当 DDTV 的 B 站登录态失效时，**所有接口**的返回包 `code` 都会被强制改写为 `6000`，即使操作本身成功。发现接口普遍返回 6000 时请先通过 `api/login/get_login_status` 检查登录态。
:::

## 接口总目录

详细文档见 [API 接口详情](./API.md)，WebSocket 推送见 [WebSocket 推送](./WEB.md)。

### system 系统信息

| 方法 | 路由 | 功能 |
|:--|:--|:--|
| GET | `api/system/get_core_version` | 获取当前 Core 版本号 |
| GET | `api/system/get_webui_version` | 获取当前 WEBUI 版本信息 |
| GET | `api/system/get_system_resources` | 获取内存和录制路径磁盘使用情况 |
| GET | `api/system/generate_debug_file_snapshot` | 生成 debug 快照文件 |
| GET | `api/system/get_c` | 获取当前登录态 Cookie（桌面端播放器用） |

### config 配置管理

| 方法 | 路由 | 功能 |
|:--|:--|:--|
| POST | `api/config/reload_configuration` | 从配置文件重新加载配置到内存 |
| POST | `api/config/set_recording_path` | 设置录制文件储存路径（需二次确认） |
| GET | `api/config/get_recording_path` | 获取录制文件储存路径 |
| POST | `api/config/set_default_file_path_name_format` | 设置录制储存路径中的子路径和文件名格式（需二次确认） |
| GET | `api/config/get_default_file_path_name_format` | 获取录制储存路径中的子路径和文件名格式 |
| GET | `api/config/restore_all_settings_to_default` | 恢复所有设置为默认（重启生效） |
| POST | `api/config/set_hls_waiting_time` | 修改 HLS 等待时间 |
| GET | `api/config/get_hls_waiting_time` | 获取 HLS 等待时间 |
| POST | `api/config/set_automatic_repair` | 设置自动修复状态 |
| GET | `api/config/get_automatic_repair` | 获取自动修复状态 |
| POST | `api/config/reinitialize` | 重新初始化（清空全部配置并退出，需二次确认） |
| POST | `api/config/set_cut_according_time` | 全局设置按时长切割录制文件 |
| POST | `api/config/set_cut_according_size` | 全局设置按大小切割录制文件 |
| GET | `api/config/get_cut_according_config` | 获取全局切割配置 |
| POST | `api/config/set_room_cut_according_to_size` | 房间维度设置按大小切割 |
| POST | `api/config/set_room_cut_according_to_time` | 房间维度设置按时长切割 |
| POST | `api/config/get_room_cut_according_config` | 获取房间维度的切割配置 |

### rec_task 录制任务

| 方法 | 路由 | 功能 |
|:--|:--|:--|
| POST | `api/rec_task/single_task` | 手动增加一个录制任务 |
| POST | `api/rec_task/cancel_task` | 取消录制任务 |
| POST | `api/rec_task/cut_task` | 手动切断并重连当前录制任务 |
| POST | `api/rec_task/generate_snapshot` | 创建直播间快照用于快速切片 |

### get_rooms 房间查询

| 方法 | 路由 | 功能 |
|:--|:--|:--|
| POST | `api/get_rooms/room_statistics` | 获取房间监控列表统计 |
| POST | `api/get_rooms/room_information` | 查询单个房间完整信息 |
| POST | `api/get_rooms/batch_complete_room_information` | 批量获取房间完整信息 |
| POST | `api/get_rooms/batch_basic_room_information` | 批量获取房间基本信息 |

### set_rooms 房间管理

| 方法 | 路由 | 功能 |
|:--|:--|:--|
| POST | `api/set_rooms/add_room` | 添加房间 |
| POST | `api/set_rooms/batch_add_room` | 批量添加房间 |
| POST | `api/set_rooms/del_room` | 删除房间 |
| POST | `api/set_rooms/batch_delete_rooms` | 批量删除房间 |
| POST | `api/set_rooms/modify_recording_settings` | 批量修改自动录制设置 |
| POST | `api/set_rooms/modify_room_prompt_settings` | 批量修改开播提醒设置 |
| POST | `api/set_rooms/modify_room_dm_settings` | 批量修改弹幕录制设置 |
| POST | `api/set_rooms/modify_room_settings` | 修改单个房间配置 |

### file 文件

| 方法 | 路由 | 功能 |
|:--|:--|:--|
| POST | `api/file/get_file_structure` | 获取录制文件夹的目录结构 |

### login 登录

| 方法 | 路由 | 功能 | 鉴权 |
|:--|:--|:--|:--|
| GET | `api/login/get_login_qr` | 获取登录二维码（PNG 图片） | 免鉴权 |
| GET | `api/login/get_login_url` | 获取生成登录二维码的 URL 字符串 | 免鉴权 |
| POST | `api/login/re_login` | 触发重新登录 | 需要 |
| POST | `api/login/get_login_status` | 获取本地登录态有效状态 | 需要 |
| GET | `api/login/get_nav` | 获取登录用户基本信息 | 需要 |
| POST | `api/login/use_agree` | 同意用户协议 | 需要 |
| POST | `api/login/use_agree_state` | 获取用户协议同意状态 | 需要 |

### 其他

| 方法 | 路由 | 功能 | 鉴权 |
|:--|:--|:--|:--|
| GET/POST | `api/dokidoki` | 获取当前运行心跳信息 | 需要 |
| GET | `api/init_inspect` | 检测 WEB 服务是否初始化完成 | 免鉴权 |
| GET | `api/unauthorized` | 鉴权失败跳转地址 | 免鉴权 |
| GET | `api/not_found` | 404 页面 | 免鉴权 |
