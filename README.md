# 目前DDTV升级到5.x版本，完全重构的全新版本

# 走过路过留个star吧

[![Stargazers over time](https://starchart.cc/CHKZL/DDTV.svg)](https://starchart.cc/CHKZL/DDTV)



# DDTV
* 开播气泡提醒，让你对自己单推列表的开播状态一目了然
* 开播自动录制，让你再也不错过精彩内容
* 完善的弹幕\SC\舰队\礼物信息录制功能不让你错过任何一个瞬间
* 录制机制保证时间轴正确，让你不用忍受修复等待时间
* 支持弹幕发送、备线切换、清晰度切换、等除送礼外的所有原生直播间功能
* 支持自动文件合并和转码
* 支持完善的鉴权API和WEBUI，方便大家二次开发

更多功能请下载体验

## 仓库说明

|项目|框架|说明|
|:--:|:--:|:--:|
|Core|.NET 8|DDTV核心库|
|Server|.NET 8 & ASP.NET 8|对Core进行更进一步封装集成了API接口和WEBUI服务|
|Desktop|.NET8 Desktop|为Windows独占，带GUI以及在线观看等高级功能，包含Core和Server的所有功能|
|Client|.NET 8|对Server进行了桌面化封装，提供WPF内嵌WEBUI|


如果有更多功能欢迎加交流群 进行交流
Q群	338182356	功能咨询和反馈
Q群	522865400(新)	聊天吹水的地方


## 感谢
|名称|说明|主页|
|---|---|---|
|DDTV_GUI_React|为DDTV提供WEBUI|[moehuhu/DDTV_GUI_React](https://github.com/moehuhu/DDTV_GUI_React)|
|docker-ddtv|为DDTV提供Docker|[moomiji/docker-ddtv](https://github.com/moomiji/docker-ddtv)|

## Q&A
Q：Desktop无法打开播放窗，或者打开播放窗就闪退怎么办？  
A：尝试安装一下[webview2 runtime](https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/consumer)
