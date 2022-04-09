# DDTV3.0
你的地表最强B站录播机

* 有着良好的初始化引导流程和界面，方便新手上手使用
* 支持一键导入关注列表中的VTB\VUP，不用手动配置冗长的房间信息
* 开播气泡提醒，让你对自己单推列表的开播状态一目了然
* 开播自动录制，让你再也不错过精彩内容
* 完善的弹幕\SC\舰队\礼物信息录制功能不让你错过任何一个瞬间
* 录制机制保证时间轴正确，让你不用忍受修复等待时间
* 支持在录制中进行切片导出，给激光烤肉更添一把火
* 支持弹幕发送、备线切换、清晰度切换、等除送礼外的所有原生直播间功能
* 更符合原版弹幕飘动显示的样式，拒绝丑陋的单独弹幕窗口，并且占用更低！更流畅！的弹幕显示功能
* 支持自动文件合并和转码
* 支持完善的鉴权API和WEBAPI，方便大家二次开发

更多功能请查看官网详细内容

---

# 版本
|项目|平台|说明|
|:--:|:--:|:--:|
|DDTV_CLI|.NET 6|对Core进行封装以适配跨平台部署需求的控制台版本|
|DDTV_WEB_Server|.NET 6 & ASP.NET 6|对CLI进行更进一步封装集成了WEB服务与API接口|
|DDTV_GUI|.NET6 Desktop|为Windows独占，带GUI以及在线观看等高级功能|
|DDTV_Core|.NET 6|核心录制与数据处理等核心功能，以基础库的形式发布|
|[DDTV_Client](https://github.com/FishMagic/DDTV_Client)|Kotlin|基于WEB API开发的客户端，适用于Windows、Linux、MacOS和Android|

# 官网和文档
**强烈建议**使用前推荐在 **[DDTV官网](https://ddtv.pro/)** 查看说明。

如果有更多功能欢迎加交流群 进行交流  
Q群	338182356	功能咨询和反馈  
Q群	307156949	聊天吹水的地方  

# Q&A
如果提示缺少.NET6环境，请先安装.NET6环境
请根据官网的教程进行操作


# 使用到的第三方组件
* [BiliAccount](https://github.com/LeoChen98/BiliAccount)
