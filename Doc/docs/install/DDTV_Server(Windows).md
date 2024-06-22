# DDTV_Server（Windows）
## 1.下载
从以下地方选一个下载DDTV最新版本  
QQ群共享(其实我推荐这个(这里的人超好的，还能直接对线(((  
DDTV功能反馈讨论群:`338182356`  
[GitHub](https://github.com/CHKZL/DDTV/releases/latest)   
DDTV聊天吹水群:`522865400`  


## 2.安装
DDTV_Server是免安装的，把下载下来的压缩包解压到任意用户有**读写权限**的路径即可   

## 3.启动准备

### 配置房间文件
默认房间文件`./Config/RoomListConfig.json`格式为json字符串，默认为空json，并且和DDTV_GUI通用  
可以直接使用其他版本DDTV的房间配置文件复制过来即可  
完整格式可参考配置文件说明中关于房间文件的说明  
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
如果是部署在公网或者有需要从外部访问的需求，请在系统防火墙和可能存在的云平台安全组中打开DDTV_Server的WEB服务所需端口(默认为**11419**)  

## 4.启动&初始化
1.使用压缩包最外层提供的bat脚本启动或者直接打开`./bin/Server.exe`直接启动。  
然后根据控制台窗口显示的内容操作即可

## 其他功能
如有任何疑问都可以加群338182356进行对线  
