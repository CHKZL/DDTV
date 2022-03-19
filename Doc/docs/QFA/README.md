# 常见问题

:::tip 有问题不能解决怎么办
可以加入DDTV反馈群联系到我:  
DDTV功能反馈讨论群:`338182356`
:::

## WEB端扫码登陆时出现问题
::: details WEB端扫码登陆时出现问题
* 终端中提示`Gdip`或者`libgdiplus`字样
> 这是一个已知的问题，在Linux下缺少Mono库中的libgdiplus，该库用于对非Windows操作系统提供GDI+兼容的API，libgdiplus是mono中的System.Drawing依赖的一个组件，用于显示web页面基本颜色等。可用于生成netcore验证码，处理图片等。    
>解决方法:  
>在终端中执行以安装libgdiplus库  
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

* 终端中提示`Could Not find '/xxxxxxxxx/BiliQR.png'`  
>请检查DDTV是否有权限向当前目录写入文件
:::

## 不能打开WEB端  
::: details 不能打开WEB端
* 访问超时或网页不存在  
>1.请确认对应的端口(默认为11419)在防火墙允许列表中  
:::


<!-- ## WEB端无法登陆或卡在登陆界面
::: details WEB端无法登陆或卡在登陆界面
* 确认WEB端apiUrl配置正确
>请确认在DDTV文件夹里的`\static\config.js`文件中的`apiUrl`为你服务器的域名或IP，并根据有无证书修改为`http`或`https`
::: -->
