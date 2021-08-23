# 常见问题
## WEB端扫码登陆时出现问题
::: details WEB端扫码登陆时出现问题
* 终端中提示`Gdip`或者`libgdiplus`字样
> 这是一个已知的问题，在Centos下缺少Mono库中的libgdiplus，该库用于对非Windows操作系统提供GDI+兼容的API，libgdiplus是mono中的System.Drawing依赖的一个组件，用于显示web页面基本颜色等。可用于生成netcore验证码，处理图片等。    
>解决方法:  
>在终端中执行以安装libgdiplus库  
>```bash
>sudo yun install libgdiplus-devel
>```

* 终端中提示`Could Not find '/xxxxxxxxx/BiliQR.png'`
>请检查DDTV是否有权限向当前目录写入文件
:::