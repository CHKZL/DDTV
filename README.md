# DDTV

一个B站直播伴侣工具：监控你关注的主播，开播自动录制直播流，并同步录制弹幕/SC/礼物/舰队等信息，保证时间轴正确。

**走过路过留个star吧 :)**

## 功能特性

* **开播气泡提醒**：让你对自己单推列表的开播状态一目了然
* **开播自动录制**：再也不错过精彩内容
* **完善的弹幕录制**：弹幕\SC\舰队\礼物信息完整记录，不让你错过任何一个瞬间
* **正确的时间轴**：录制机制保证时间轴正确，不用忍受修复等待时间
* **直播间原生功能**：支持弹幕发送、备线切换、清晰度切换等除送礼外的所有原生直播间功能
* **自动后处理**：支持自动文件合并和转码（基于 ffmpeg）
* **完善的API和WEBUI**：带鉴权的 REST API + WebSocket 推送，方便大家二次开发
* **多种部署形态**：Windows 桌面 GUI、跨平台 Server（无头模式）、Docker 部署

更多功能请下载体验

## 仓库说明

|项目|框架|说明|
|:--:|:--:|:--:|
|Core|.NET 10|DDTV核心库，包含全部业务逻辑|
|Server|.NET 10 & ASP.NET 10|对Core进行更进一步封装，集成了API接口和WEBUI服务|
|Desktop|.NET 10 Desktop|为Windows独占，带GUI以及在线观看等高级功能，包含Core和Server的所有功能|
|Client|.NET 10|对Server进行了桌面化封装，提供WPF内嵌WEBUI|
|Doc|Vue / VitePress|文档站点（含安装、配置、API文档等）|

## 快速开始

### 普通用户

无需安装任何开发环境，直接前往 [Releases](https://github.com/CHKZL/DDTV/releases) 页面下载对应平台的压缩包，解压即用：

| 你的情况 | 下载哪个版本 |
|---|---|
| Windows 用户（推荐） | **Desktop**：带完整 GUI、开播提醒、在线观看等全部功能 |
| Windows 用户（轻量） | **Client**：精简桌面壳，内嵌 WEBUI |
| Linux / macOS / 服务器 / NAS | **Server**：无头模式，通过浏览器访问 WEBUI 管理 |
| Docker 用户 | 见下方 [Docker 部署](#docker-部署) |

下载后解压运行即可，已内置运行环境，无需安装 .NET。

> **注意**：Linux / macOS 平台需要自行安装 `ffmpeg` 并加入 PATH，否则录制文件的自动修复/转码功能不可用。

### 开发者

#### 环境要求

* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* 非 Windows 平台需自行安装 `ffmpeg` 并加入 PATH（录制修复/转码功能依赖）

#### 构建与运行

```bash
dotnet build DDTV.sln              # 构建整个解决方案

dotnet run --project Server        # 无头服务器模式（API + WEBUI）
dotnet run --project Desktop       # Windows 桌面 GUI（含 Core + Server）
dotnet run --project Client        # WPF 壳，内嵌 WEBUI
```

### Docker 部署

推荐使用社区提供的 [moomiji/docker-ddtv](https://github.com/moomiji/docker-ddtv)。

## 文档

详细安装、配置、高级功能与 API 文档请查阅 [Doc/docs](Doc/docs) 目录下的文档站点源码。

## 交流群

| 群号 | 说明 |
|---|---|
| Q群 338182356 | 功能咨询和反馈 |
| Q群 522865400(新) | 聊天吹水的地方 |

## 感谢

|名称|说明|主页|
|---|---|---|
|DDTV_GUI_React|为DDTV提供WEBUI|[moehuhu/DDTV_GUI_React](https://github.com/moehuhu/DDTV_GUI_React)|
|docker-ddtv|为DDTV提供Docker|[moomiji/docker-ddtv](https://github.com/moomiji/docker-ddtv)|


## 免责声明

本项目为**非官方**的个人开源项目，与哔哩哔哩（bilibili）及其关联公司无任何隶属、授权或合作关系。项目中所使用的接口、数据与内容均来自公开互联网，仅供个人学习与技术研究使用，请勿用于任何商业用途或侵犯他人权益的行为。使用本项目所产生的一切后果由使用者自行承担。
