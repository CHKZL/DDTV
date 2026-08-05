# DDTV_Desktop（Windows）
:::warning 注意
DDTV_Desktop仅支持Windows平台运行，其他平台需要录制请使用DDTV_Server
:::

- Desktop为Windows下的完全体：集成本体录制核心（Core）、WEB服务（Server）和桌面GUI于一体，包含Server和Client的所有功能，以及特有的观看和桌面端控制UI，支持连接远程Server，为只适配Windows的WPF应用  
- 内置两种播放窗口：WebView2播放窗口和VLC播放窗口，可直接在桌面端多开观看直播、查看和发送弹幕  
- 如果为Windows环境，强烈推荐使用该版本  

## 1.下载
从以下地方选一个下载DDTV最新版本    
[bilibili](https://play-live.bilibili.com/details/1651690688387)   
QQ群共享(其实我推荐这个(这里的人超好的，还能直接对线(((  
DDTV功能反馈讨论群:`338182356`  
[GitHub](https://github.com/CHKZL/DDTV/releases/latest)  
DDTV聊天吹水群:`522865400`   

请下载`DDTV-Desktop-windows-latest-win-x64-release[版本号].zip`。

## 2.安装
DDTV_Desktop本体是免安装的，且为**自包含发布**（无需安装.NET运行时），把下载下来的压缩包解压到任意位置即可  

### WebView2运行时依赖
DDTV_Desktop的Web播放窗口等界面依赖**Microsoft Edge WebView2 Runtime**。  
Windows 11系统一般已自带；Windows 10或精简版系统如果没有，请到微软官方页面下载安装（下载`Evergreen Bootstrapper`在线安装即可）：  
[https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/](https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/)  

### 内置组件（开箱即用）
DDTV_Desktop的压缩包中已经内置了以下组件，均无需另行安装：  
- `ffmpeg`（位于`bin/Plugins/ffmpeg/`）：用于录制完成后的自动修复、合并与转码  
- `VLC`播放组件（位于`bin/Plugins/vlc/`）：用于内置VLC播放窗口观看直播  
- `MKVToolnix`（位于`bin/Plugins/MKVToolnix/`，含`mkvmerge.exe`）：用于录制文件封装处理  

## 3.运行
在下载的压缩包中，最外层提供了快捷启动的`启动DDTV_Desktop.bat`脚本；如果希望创建桌面快捷方式，请对`./bin/Desktop.exe`右键创建快捷方式  
