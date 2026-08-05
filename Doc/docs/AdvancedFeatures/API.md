# API

DDTV5 内置了完整的 HTTP API 和 WebSocket 推送服务，可以用来做远程管理、状态监控、接入自己的程序或自动化脚本等二次开发。

## 能做什么

- **房间管理**：添加/删除/批量修改监控房间，设置自动录制、开播提醒、弹幕录制
- **录制任务**：手动触发/取消/切断录制任务，创建快剪快照
- **状态查询**：房间开播/录制状态、系统资源占用、运行心跳
- **配置管理**：录制路径、文件名格式、自动修复、切割配置等
- **实时推送**：通过 WebSocket 或 WebHook 实时接收开播、录制开始/结束、登录态变化等事件

## 三分钟上手

### 1. 确认 WEB 服务已开启

DDTV Server 默认开启 WEB 服务并监听 `http://localhost:11419`（配置项 `EnableWebServer`、`IP`、`Port`）。启动后可以先访问免鉴权接口确认服务可用：

```bash
curl http://127.0.0.1:11419/api/init_inspect
# {"cmd":"init_inspect","code":0,"data":"OK","message":"OK"}
```

### 2. 找到你的密钥

打开 `./Config/DDTV_Config.ini`：

- `AccessKeyId`：默认 `ddtv`
- `AccessKeySecret`：**首次启动时随机生成并打印到控制台**，之后保存在配置文件中

### 3. 计算签名并发起请求

除少数免鉴权接口外，每个请求都需要携带 `access_key_id`、`time`（Unix 秒时间戳）、`sig`（SHA1 签名）三个公共参数。一个可直接运行的 Python 例子：

```python
import hashlib
import time
import requests

BASE_URL = "http://127.0.0.1:11419"
ACCESS_KEY_ID = "ddtv"
ACCESS_KEY_SECRET = "替换成你的AccessKeySecret"

def signed_params(params: dict) -> dict:
    params = dict(params)
    params["access_key_id"] = ACCESS_KEY_ID
    params["time"] = int(time.time())
    raw_items = list(params.items()) + [("access_key_secret", ACCESS_KEY_SECRET)]
    raw = ";".join(f"{k.lower()}={v}" for k, v in sorted(raw_items))
    params["sig"] = hashlib.sha1(raw.encode("utf-8")).hexdigest()
    return params

# 查询监控房间统计
resp = requests.post(f"{BASE_URL}/api/get_rooms/room_statistics", data=signed_params({}))
print(resp.json())
```

## 详细文档

- [API 总览（鉴权机制 / 公共参数 / 返回包 / 状态码 / 接口目录）](../API/README.md)
- [API 接口详情（全部接口的参数与返回示例）](../API/API.md)
- [WebSocket 推送（事件推送协议与客户端示例）](../API/WEB.md)
- [WebSocket 服务器](./WebSocket服务器.md)
- [WEB 服务器](./WEB服务器.md)
- [WebHook](./WebHook.md)
