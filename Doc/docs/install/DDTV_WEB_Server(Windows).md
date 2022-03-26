# DDTV_WEB_Server安装教程（Windows）
## 1.下载
从以下地方选一个下载DDTV最新版本  
[GitHub](https://hub.fastgit.xyz/CHKZL/DDTV/releases/latest)  
QQ群共享(其实我推荐这个(这里的人超好的，还能直接和我对线(((  
DDTV功能反馈讨论群:`338182356`
DDTV聊天吹水群:`307156949`  

## 2.安装
DDTV_WEB_Server是免安装的，把下载下来的压缩包解压到任意位置即可   

## 3.启动准备
:::warning 注意
如果在这个步骤中出现了你本地并不存在的文件，请尝试先启动一次DDTV_WEB_Server，正常情况下会自动生成缺失的文件。当然也可以手动新建，但是在linux下请注意权限问题。
:::
### (1)运行环境准备
>DDTV_WEB_Server依赖于`.NET Runtime 6.0`和`ASP.NET Core Runtime 6.0`环境运行，请先安装`.NET Runtime 6.0`和`ASP.NET Core Runtime 6.0`：  
[下载.NET Runtime(x64)](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-6.0.1-windows-x64-installer)   
[下载ASP.NET Core Runtime(x64)](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-6.0.1-windows-x64-installer)  
如果你的操作系统不是`64位Windows10`请到参考[微软文档](https://docs.microsoft.com/zh-cn/dotnet/core/install/windows?tabs=net60)进行环境的安装  

### 配置房间文件
默认房间文件`RoomListConfig.json`格式为json字符串，默认为空json，并且和DDTV_GUI通用  
在API中支持一键导入  
可以直接使用DDTV的房间配置文件复制过来即可  
在下载的压缩包里附带了一个参考的文件，也可以参考那个文件进行手动编写  
房间配置文件格式为
```json
{
            "name": "未来明-MiraiAkari",//昵称
            "Description": "",//备注
            "RoomId": 6792401,//房间号
            "UID": 238537745,//账号UID
            "IsAutoRec": false,//开播后是否自动录制
            "IsRemind": false,//开播后是否提醒(DDTV_GUI特有，在本版本中无效)
            "IsRecDanmu": false,//是否录制该房间弹幕(需要打开总弹幕录制开关)
            "Like": false///特别标注(本版本无效)
},
```
多个这种格式的内容组成  

:::danger 警告 
手动编辑过后请检查JSON字符串的合法性，请保证确保符合参考文件的JSON文件格式！！！
::: 

### WEB端口设置
如果是部署在公网或者有需要从外部访问的需求，请在系统防火墙和可能存在的云平台安全组中打开DDTV_WEB_Server的WEB服务所需端口(默认为**11419**) 

## 4.启动&初始化
在Windows下直接打开DDTV_WEB_Server.exe即可  
然后根据控制台窗口显示的内容操作即可



:::danger 警告 
在Windows下cmd\PowerShell都有一个默认勾选的选项`快速编辑模式`，程序在运行中点击控制台窗口会导致程序冻结！停止运行！请关闭该选项或注意不要点击控制台窗口
:::

## 其他功能
其他高级功能请参考`配置说明`页面的内容进行相应功能的配置即可，如有任何疑问都可以加群338182356进行对线
