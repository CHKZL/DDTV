# DDTVLiveRec安装教程（Linux）
## 1.下载
从以下地方选一个下载DDTV2.0最新版本  
[GitHub](https://hub.fastgit.org/CHKZL/DDTV2/releases/latest)  
QQ群共享(其实我推荐这个(这里的人超好的，还能直接和我对线(((  
DDTV聊天吹水群:`307156949`  
DDTV功能反馈讨论群:`338182356`

## 2.安装
DDTVLiveRec是免安装的，把下载下来的压缩包解压到任意当前linux用户有**读写权限**的路径即可   

## 3.启动准备
:::warning 注意
如果在这个步骤中出现了你本地并不存在的文件，请尝试先启动一次DDTVLiveRec，正常情况下会自动生成缺失的文件。当然也可以手动新建，但是在linux下请注意权限问题。
:::
### (1)运行环境准备
DDTVLiveRec依赖于`.NET Runtime`和`ASP.NET Core Runtime`环境运行，请先安装`.NET Runtime`和`ASP.NET Core Runtime`：  
请根据你所使用的linux发行版本参考[微软文档](https://docs.microsoft.com/zh-cn/dotnet/core/install/linux)进行环境的安装  
:::warning .NET版本提示 
安装的版本只要高于.NET 5.0.1即可
:::
### (2)配置房间文件
默认房间文件`RoomListConfig.json`格式为json字符串，默认为空json，并且和DDTV2.0通用  
可以直接使用DDTV2.0的房间配置文件复制过来即可  
在下载的压缩包里附带了一个参考的文件，也可以参考那个文件进行手动编写  
房间配置文件格式为
```json
{
            "Name": "绯赤艾莉欧",
            "OfficialName": "緋赤エリオ",
            "RoomNumber": "21396545",
            "Types": "bilibili",
            "status": false,
            "VideoStatus": false,
            "RemindStatus": false,
            "LiveStatus": false
}
```
多个这种格式的内容组成  
使用DDTVLiveRec需要注意的为["Name"]["OfficialName"]["RoomNumber"]["VideoStatus"]；其中：   
* ["Name"]为翻译名称
* ["OfficialName"]为官方名称
* ["RoomNumber"]为BiliLive房间号
* ~~["Types"]为多平台支持预留，现在默认并必须为"bilibili"~~
* ~~["status"]为DDTV需要的配置文件，在DDTVLive中无用，默认为false~~
* ["VideoStatus"]为是否开播自动录制的标识，false为检测到开播后不录制，true为检测到开播后自动录制
* ~~["RemindStatus"]为DDTV开播弹窗提醒标识，在DDTVLive中无用，默认为false~~
* ~~["LiveStatus"]为DDTV直播状态标识，在DDTVLive中无用，默认为false~~  
**也就是说["VideoStatus"]为false的项不会自动录制，请注意**  
:::danger 警告 
手动编辑过后请检查JSON字符串的合法性，请保证确保符合参考文件的JSON文件格式！！！
::: 
### (3)WEB端口设置
如果是部署在公网或者有需要从外部访问WEB页的需求，请在系统防火墙和可能存在的云平台安全组中打开DDTVLiveRec的WEB服务所需端口(默认为**11419**)
## 4.启动&初始化
1.根据你所使用的linux发行版本使用`dotnet ./DDTVLiveRec.dll`命令直接启动或者是用类似`nohup dotnet ./DDTVLiveRec.dll &`或者使用`scress`进行后台启动。   
然后根据控制台窗口显示的内容操作即可



:::danger 警告 
在Windows下cmd\PowerShell都有一个默认勾选的选项`快速编辑模式`，程序在运行中点击控制台窗口会导致程序冻结！停止运行！请关闭该选项或注意不要点击控制台窗口
:::

## 其他功能
其他高级功能请参考`配置说明`页面的内容进行相应功能的配置即可，如有任何疑问都可以加群307156949进行对线