# DDTV_CLI安装教程（Linux）
## 1.下载
从以下地方选一个下载DDTV最新版本  
[GitHub](https://hub.fastgit.xyz/CHKZL/DDTV/releases/latest)  
QQ群共享(其实我推荐这个(这里的人超好的，还能直接和我对线(((  
DDTV功能反馈讨论群:`338182356`
DDTV聊天吹水群:`307156949`  


## 2.安装
DDTV_CLI是免安装的，把下载下来的压缩包解压到任意当前linux用户有**读写权限**的路径即可   

## 3.启动准备
:::warning 注意
如果在这个步骤中出现了你本地并不存在的文件，请尝试先启动一次DDTV_CLI，正常情况下会自动生成缺失的文件。当然也可以手动新建，但是在linux下请注意权限问题。
:::
### 运行环境准备
* 请从以下两种方法中选择适合你的
### 方法一：x86常见发行版本安装方法：
>DDTV_CLI依赖于`.NET Runtime 6.0`环境运行，请先安装`.NET Runtime 6.0`：  
>请根据你所使用的linux发行版本参考[微软文档](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)进行环境的安装  
>:::warning .NET版本提示 
>安装的版本只要高于.NET 6.0.1即可
>:::  
  
### 方法二：微软文档没有说明的发行版本或者Arm等版本安装方法  
>(所有版本的linux也都可以用这个方法)  
>终端里输入以下内容
>```shell
>sudo wget 不同cpu架构的下载地址 -O dotnet-sdk.tar.gz
>
>sudo mkdir -p /usr/share/dotnet
>
>sudo rm /usr/share/dotnet/* -rf
>
>sudo tar -xzvf dotnet-sdk.tar.gz -C /usr/share/dotnet/
>
>sudo ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet -f
>```
>:::tip 不同cpu架构的下载地址哪里找？   
>在[.NET6 环境下载地址](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)中选择对应的cpu架构版本  
><img :src="$withBase('/dotnet下载地址1.png')">  
><img :src="$withBase('/dotnet下载地址2.png')">  
>ps1:查看cpu架构在终端里输入uname -m  
>ps2:aarch就是Arm
>:::  
>然后在终端里输入
>```shell
>dotnet --info
>```
>如果出现了dotnet相关信息，那么恭喜你，环境配置成功了  


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

## 4.启动&初始化
1.根据你所使用的linux发行版本使用`dotnet ./DDTV_CLI.dll`命令直接启动，如果要在无GUI的linux服务器上后台运行请使用`screen`进行启动，**请勿使用`nohup`的方式进行启动**。   
然后根据控制台窗口显示的内容操作即可

::: tip   
启动或运行出现其他错误请参考[常见问题](../QFA)页面  
:::  

## 其他功能
其他高级功能请参考`配置说明`页面的内容进行相应功能的配置即可，如有任何疑问都可以加群338182356进行对线
