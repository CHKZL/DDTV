# 版本介绍
DDTV5提供了`DDTV_Server`、`DDTV_Client`、`DDTV_Desktop`三个版本方便给不同需求的用户自行选择使用  

【Server】：为控制台应用，自带WEBUI服务，跨平台适配Windows、Linux、macOS  
【Client】：为Server的Windows平台下的窗口程序封装，在Server的基础上提供了WEBUI的桌面窗口，只适配Windows，适合Windows下的轻量化用户  
【Desktop】：Windows下的完全体，提供Server和Client的所有功能，以及特有的观看和桌面端控制UI（内置WebView2/VLC播放窗口），支持连接远程Server，为只适配Windows的WPF应用  

  
### 版本区别详情  

| 功能| DDTV_Server | DDTV_Client |DDTV_Desktop|
|:---------------------|:---------:|:-------------:|:--:|
|Windows平台部署|✔️|✔️|✔️|
|Linux平台部署|✔️|||
|macOS平台部署（仅Apple Silicon）|✔️|||
| 桌面窗口界面 |  |✔️|✔️|
|WEBUI|✔️|✔️|✔️|
| 自动录制    |✔️|✔️|✔️|
| 多路异步下载  |✔️|✔️|✔️|
| 扫码登陆功能  |✔️|✔️|✔️|
| 关注列表一键自动导入   |  |  |✔️|
| 弹幕录制    | ✔️ | ✔️|✔️|
| 自动转码    | ✔️ | ✔️|✔️|
| 时间轴错误自动修复|  ✔️| ✔️|✔️|
| 开播,录像气泡提示|  ✔️|✔️ |✔️|
| 直播观看（内置播放窗口）    |  | |✔️|
| 开播状态列表  |  | |✔️|
| 登陆买票后可以观看付费直播内容|  ✔️| ✔️|✔️|
| 登陆买票后可以录制付费直播内容|  ✔️|✔️ |✔️|
| 直播弹幕查看  |  | |✔️|
| 直播弹幕发送  |  | |✔️|
| 多路音频分区调整|  | |✔️|
| GPU硬件视频解码|  | |✔️|
| 自动更新脚本    | ✔️ | ✔️ |✔️|
| 跨平台| ✔️  | ||
|WEB服务|✔️|✔️|✔️|
| API接口   | ✔️ |✔️|✔️|
| API鉴权   |  ✔️ |✔️ |✔️|

## 下载与包体命名规则

请到[GitHub Releases](https://github.com/CHKZL/DDTV/releases/latest)下载最新版本。  

DDTV5的发布包为**自包含发布**（无需自行安装.NET运行时），包体命名规则为：  

```
DDTV-[版本]-[构建系统]-[硬件架构]-release[版本号].zip
```

目前提供的包体为：  

| 包体 | 适用环境 |
|:---------------------|:---------|
| `DDTV-Server-windows-latest-win-x64-release[版本号].zip` | 64位Windows |
| `DDTV-Server-ubuntu-latest-linux-x64-release[版本号].zip` | 64位Linux（x86_64） |
| `DDTV-Server-ubuntu-latest-linux-arm-release[版本号].zip` | 32位ARM Linux（armv7，如树莓派） |
| `DDTV-Server-ubuntu-latest-linux-arm64-release[版本号].zip` | 64位ARM Linux（aarch64） |
| `DDTV-Server-macOS-latest-osx-arm64-release[版本号].zip` | macOS（仅Apple Silicon，M系列芯片） |
| `DDTV-Desktop-windows-latest-win-x64-release[版本号].zip` | 64位Windows桌面完全体 |
| `DDTV-Client-windows-latest-win-x64-release[版本号].zip` | 64位Windows轻量窗口版 |

:::warning 注意
macOS目前**只提供Apple Silicon（osx-arm64）版本**，没有Intel芯片（osx-x64）版本；Intel芯片的Mac请使用虚拟机或Docker方式运行Linux版。  
:::
