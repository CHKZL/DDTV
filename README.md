# DDTV2

DDTV1.0的精神续作.jpg     
使用.NET Core和WPF完全重写了，修改了很多在1.0不科学的地方。    
***
(代码中大量 **中文** 变量\函数 **易#** 警告，美术生出身，请各位大佬轻拍 
***
# 包含的项目(点击项目名查看详细说明)
**(点击项目名查看详细说明)**  
**(点击项目名查看详细说明)**  
|  项目名称 | 框架 | 平台 | 说明 |
|  ----  | ---- | ---- | ---- |
| [DDTV_New](https://github.com/CHKZL/DDTV2/tree/master/DDTV_New)  | .NET Framework 4.7.2 | Windows | DDTV2本体 |
| [DDTVLiveRec](https://github.com/CHKZL/DDTV2/tree/master/DDTVLiveRec) | .NET5 | Windows/Linux/MacOS | 适用于多平台的录制工具 |
| Auxiliary  | .NET Framework 4.7.2(没用特殊的库,兼容.NET5) | Windows | 为项目写的各种共用方法依赖 |
| DDTVLiveRecWebServer  | .NET5 | Windows/Linux/MacOS | 为项目提供WEB访问查询功能 |
| PlayW  | .NET Framework 4.7.2 | Windows | 用于播放视频流和弹幕解析 |
***
###
功能  
DDTV_New：
* 多路直播监控，可自定义监听房间，摸鱼中\直播中一目了然
* 多窗口随意排列
* 播放窗口无边框，勿扰模式
* 每路声音可单独调整
* 手动录像/开播自动录像
* 监控列表开播提示
* 在滑动鼠标滚轮修改窗口音量
* 缩小到系统托盘后台监听
* 弹幕显示
* 野生字幕显示
* 油管，TC平台直播状况查看
* 开播,录像气泡提示
* 多路异步下载
* 在播放窗口CTRL+D老板键
* 扫码登陆功能
* 登陆后阿B关注列表VTB\VUP一键自动导入
* 登陆买票后可以观看付费直播内容
* 直播弹幕发送
* 录制完成后自动合并文件
* 录制完成后自动转码为MP4并修复直播流flv时间轴错误的问题
* 在录制的同时储存弹幕信息为ass文件
* 本体自动更新


DDTVLiveRec
* 支持linux，可以挂在路由器或树莓派等linux嵌入式设备上运行
* 开播自动录制
* 录制完成后自动合并文件
* 录制完成后自动转码为MP4并修复直播流flv时间轴错误的问题 (在Linux/MacOS上需要自行安装ffmpeg)
* 多路异步下载
* 在网页直接查看运行状态\日志\下载文件列表
* 登陆买票后可以录制付费直播内容
* 在录制的同时储存弹幕信息为ass文件

***

## 写给不会使用gayhub的：怎么下载？？？怎么下载？？？怎么下载？？？
↓↓↓↓↓↓↓↓↓↓↓↓↓↓点击跳转下载页面↓↓↓↓↓↓↓↓↓↓↓↓↓↓  
[点击跳转到releases下载页面](https://github.com/CHKZL/DDTV2/releases/latest)  
↑↑↑↑↑↑↑↑↑↑↑↑↑↑点击跳转下载页面↑↑↑↑↑↑↑↑↑↑↑↑↑↑  
===如果实在是下载不动也可以加群在群共享中下载,群：307156949===  
(有任何问题和需要增加的功能也欢迎加群
# 使用到的第三方组件
* [BiliAccount](https://github.com/LeoChen98/BiliAccount)
* [FFmpeg](https://github.com/FFmpeg/FFmpeg)
* [vtbs.moe](https://github.com/dd-center/vtbs.moe)

# 捐助
### 捐助表示您对我这个项目的认可，也能激励我继续开发更多好的项目

![生活](https://github.com/CHKZL/DDTV2/blob/master/DDTV_New/%E7%94%9F%E6%B4%BB.png)

* 支付宝

![支付宝](https://github.com/CHKZL/DDTV/blob/master/src/ZFB.png)
* 微信

![微信](https://github.com/CHKZL/DDTV/blob/master/src/WX.png)
