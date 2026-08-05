# 常见问题

:::tip 有问题不能解决怎么办
可以加入DDTV反馈群联系到我:  
DDTV功能反馈讨论群:`338182356`
:::

## WEB端扫码登陆时出现问题
::: details WEB端扫码登陆时出现问题
* 终端中提示`Gdip`或者`libgdiplus`字样  
> 当前版本的登录二维码已改用SkiaSharp/ZXing实现（Linux下控制台会直接打印二维码字符画），**正常情况下已经不再需要libgdiplus**。  
>如果你运行的是较老的版本，可以通过安装libgdiplus解决：  
>* Centos
>```bash
>sudo yum install libgdiplus-devel
>```
>* Ubuntu
>```bash
>sudo apt install libgdiplus
>```
>PS:如果提示无法找到该库，根据微软官方说明，需要将 Mono 存储库添加到系统来安装最新版 libgdiplus
>[说明文档](https://www.mono-project.com/download/stable/)  
>建议直接升级到最新版本DDTV以彻底摆脱该依赖。

* 二维码不显示或显示异常  
>请检查DDTV是否有权限向当前目录写入文件；Linux下也可直接扫描控制台中打印的字符画二维码。
:::

## 不能打开WEB端  
::: details 不能打开WEB端
* 访问超时或网页不存在  
>1.请确认对应的端口(默认为11419)在防火墙允许列表中  
:::

## WEBUI的AccessKeySecret是什么、在哪里看？
::: details WEBUI的AccessKeySecret是什么、在哪里看？
>`AccessKeyId`默认为`ddtv`；`AccessKeySecret`没有固定默认值，是**首次启动时随机生成**的16位字符串，生成时会打印在控制台中（`初次使用，随机生成的AccessKeySecret为：...`），并写入配置文件。  
>忘记时可打开`./bin/Config/DDTV_Config.ini`搜索`AccessKeySecret`查看；也可以在关闭DDTV后删除该行（或整个配置文件）再重启，会重新生成并再次打印到控制台。
:::

## 启动时提示"请确保已安装ffmpeg"
::: details 启动时提示"请确保已安装ffmpeg"
>非Windows系统启动时DDTV会提示`当前为非Windows环境，请确保已安装ffmpeg，否则自动修复和封装转码会失败！`。  
>这是因为Linux/macOS版的包内不附带ffmpeg，需要用系统的包管理器自行安装（如`sudo apt install ffmpeg`）。该提示只影响录制完成后的自动修复和转码封装功能，不影响录制本身；如确认已安装可忽略。  
>Windows版的压缩包已内置ffmpeg，无需另行安装。
:::

## Windows桌面端打开窗口报错或白屏（WebView2）
::: details Windows桌面端打开窗口报错或白屏（WebView2）
>DDTV_Client和DDTV_Desktop的窗口界面依赖`Microsoft Edge WebView2 Runtime`。Windows 11一般已自带；Windows 10或精简版系统如果缺失，请到微软官方页面下载安装`Evergreen Bootstrapper`：  
>[https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/](https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/)
:::

<!-- ## WEB端无法登陆或卡在登陆界面
::: details WEB端无法登陆或卡在登陆界面
* 确认WEB端apiUrl配置正确
>请确认在DDTV文件夹里的`\static\config.js`文件中的`apiUrl`为你服务器的域名或IP，并根据有无证书修改为`http`或`https`
::: -->
