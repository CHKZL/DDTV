# WebHook

WebHook 功能会把 DDTV 的每一条运行状态推送消息（与 WebSocket 服务推送的内容相同）以 HTTP POST 的方式实时转发给你指定的地址。适合想把 DDTV 的运行事件（开播、录制开始/结束、修复完成等）对接到自己服务、机器人或自动化流程的用户。

## 这是什么

DDTV 内部有一个 WebSocket 推送通道，运行过程中的各种事件（开播事件、录制开始、录制结束、修复/转码完成、弹幕文件保存等）都会生成一条 JSON 文本消息推送出去。开启 WebHook 后，DDTV 会把**每一条这样的消息原样** POST 到你配置的地址。

## 怎么开启

配置文件 `./Config/DDTV_Config.ini`（也可在 Desktop 设置页 / WEBUI 设置中修改，修改后**立即生效**，无需重启）：

| 配置项 | 默认值 | 说明 |
|:--|:--:|:--|
| `WebHookSwitch` | `false` | WebHook 总开关 |
| `WebHookAddress` | 空 | 推送目标地址，需为完整 URL，如 `http://192.168.1.10:9000/ddtv-hook` |

两个条件同时满足（开关打开且地址不为空）才会推送。

## 推送行为细节

* **请求方式**：`POST`，`Content-Type: application/json`，正文就是该条消息的 JSON 文本（与 WebSocket 推送的消息格式一致，包含 `cmd`、`code`、`message`、`data` 等字段）。
* **只发一次，不重试**：每条消息只尝试发送一次，失败（超时、网络错误、对方返回错误等）只在 DDTV 日志中记录一条错误，不会补发。
* **超时时间 8 秒**：目标服务如果 8 秒内没有响应，本次推送判定失败并放弃。
* **异步发送**：推送在后台线程进行，不会阻塞 DDTV 的录制等主流程。

::: warning 使用建议
* 由于"只发一次不重试"，WebHook 适合做实时性提醒，不适合做要求必达的关键业务流转；如果消息不能丢，建议自己轮询 API 或对接 WebSocket 服务（见 `WebSocket服务器` 文档）。
* 目标服务请尽量快速返回响应（收到后异步处理），避免大量消息时频繁触发 8 秒超时。
* 事件较频繁（例如多房间同时开播/下播），接收端请做好并发处理。
:::

## 一个简单的接收示例

如果你只是想快速验证，可以用任何能起 HTTP 服务的工具接收，例如用 Python：

```python
from http.server import BaseHTTPRequestHandler, HTTPServer

class H(BaseHTTPRequestHandler):
    def do_POST(self):
        body = self.rfile.read(int(self.headers['Content-Length']))
        print(body.decode('utf-8'))
        self.send_response(200); self.end_headers()

HTTPServer(('0.0.0.0', 9000), H).serve_forever()
```

然后把 `WebHookAddress` 填为 `http://<本机IP>:9000/` 即可看到 DDTV 推送过来的消息。
