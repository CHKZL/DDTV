# WEB 服务器

DDTV5 的 Server（以及内嵌了 Server 的 Desktop / Client）在启动时会运行一个内置的 ASP.NET Core WEB 服务，它同时承担四个角色：**WEBUI 静态站点、HTTP API、WebSocket 推送通道、录制文件在线访问**。本文介绍这个 WEB 服务本身的配置与行为。

## 启用与监听配置

WEB 服务默认开启，相关配置项都在 `./Config/DDTV_Config.ini` 中：

| 配置项 | 默认值 | 说明 |
|:--|:--:|:--|
| `EnableWebServer` | `true` | 是否启用 WEB 服务和端口监听；为 `false` 或 `Port` 为 0 时不启动 WEB 服务 |
| `Port` | `11419` | WEB 服务监听端口 |
| `IP` | `http://localhost` | WEB 服务监听地址，格式为 `协议://地址`（**不带端口**）。需要局域网访问时改为 `http://0.0.0.0` |

实际监听 URL 由 `IP` 和 `Port` 拼接而成（`IP:Port`），例如默认即 `http://localhost:11419`。

::: warning 注意
- 启动时如果检测到端口已被占用，WEB 服务会启动失败并在控制台输出错误，请更换 `Port` 或释放端口。
- 修改 `IP` / `Port` / `EnableWebServer` 后需要重启程序生效。
- API 有 sig 鉴权，但 WebSocket（`/ws`）无鉴权；把服务暴露到局域网/公网前请评估风险，建议配合防火墙或反向代理的访问控制。
:::

## 内置 WEBUI

访问 WEB 服务根路径（默认 `http://localhost:11419/`）会直接返回 WEBUI 首页（`./Static/index.html`），整个 `./Static/` 目录被映射为根路径的静态文件服务，WEBUI 的页面、脚本、样式都从这里提供。

WEBUI 提供房间管理、录制任务查看、文件浏览与在线播放、配置修改等图形化功能，是管理 DDTV 最方便的方式。

### 录制文件在线访问

录制文件目录（配置项 `RecFileDirectory`）被映射为虚拟路径 **`/rec_file`** 的静态文件服务，因此可以直接通过浏览器播放或下载录制文件：

```
http://localhost:11419/rec_file/<主播文件夹>/<日期文件夹>/<文件名>.flv
```

WEBUI 的文件管理和在线播放功能就是基于这个路径实现的；通过 [get_file_structure](../API/API.md#get_file_structure) 接口拿到目录结构后，你也可以在自己的程序里拼出文件 URL。

## HTTP API

所有接口路由形如 `api/<分组>/<接口名>`，除少数免鉴权接口外都需要 sig 签名。详见：

- [API 总览与鉴权机制](../API/README.md)
- [API 接口详情](../API/API.md)

### Swagger 调试页面

WEB 服务内置了 Swagger，但默认关闭。把配置项 `EnableSwagger` 改为 `true` 并重启后，访问：

```
http://localhost:11419/swagger
```

即可查看和在线调试全部 API 接口。

::: warning 注意
Swagger 页面可列出全部接口定义，仅建议在受信任的网络环境中开启，调试完成后建议改回 `false`。
:::

## 跨域设置

如果你想把 WEBUI 换成自己开发的前端页面（前后端分离部署），WEB 服务会为所有响应附加跨域头，相关配置项：

| 配置项 | 默认值 | 说明 |
|:--|:--:|:--|
| `AccessControlAllowOrigin` | `*` | `Access-Control-Allow-Origin` 响应头，可改为完整的前端地址（含协议和端口） |
| `AccessControlAllowCredentials` | `true` | `Access-Control-Allow-Credentials` 响应头 |

修改后重启生效。

## WebSocket 推送

WEB 服务在 `/ws` 路径提供 WebSocket 事件推送（开播、录制开始/结束等实时事件），与 HTTP 服务同端口、无鉴权。详见 [WebSocket 服务器](./WebSocket服务器.md)。

## 杂项端点

| 地址 | 说明 |
|:--|:--|
| `GET /` | WEBUI 首页 |
| `GET /api/init_inspect` | 免鉴权，检测 WEB 服务是否初始化完成，返回 `OK` |
| `GET /api/unauthorized` | API 鉴权失败时的跳转地址 |
| `GET /api/not_found` | 404 地址，存在 `./static/404.html` 时返回该页面，否则返回内置 404 图片 |
