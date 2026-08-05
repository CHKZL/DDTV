# WEBUI说明

- 任一版本均携带WEBUI，启动后访问配置的域名以及端口即可（默认为`http://127.0.0.1:11419`）  
- 在WEBUI登陆页，输入配置文件中的`AccessKeyId`以及`AccessKeySecret`后进入WEBUI  

## 关于AccessKeyId和AccessKeySecret

- `AccessKeyId`：默认为`ddtv`  
- `AccessKeySecret`：**首次启动时随机生成**（16位随机字符串），生成时会打印在控制台中，内容类似：  
  ```
  初次使用，随机生成的AccessKeySecret为：xxxxxxxxxxxxxxxx
  ```
  同时会写入配置文件`./bin/Config/DDTV_Config.ini`中，忘记时可直接打开该文件查看。  

:::tip 忘记AccessKeySecret怎么办
1. 先尝试直接打开`./bin/Config/DDTV_Config.ini`，搜索`AccessKeySecret`查看当前值；  
2. 如果想重置：关闭DDTV后，删除配置文件中`AccessKeySecret`所在的行（或直接删除整个`DDTV_Config.ini`文件），重新启动后会重新随机生成一个新的密钥并再次打印到控制台。  
:::

:::warning 注意
如需将WEBUI部署在公网环境中，强烈推荐修改`AccessKeyId`和`AccessKeySecret`这两个参数  
该参数在`./bin/Config/DDTV_Config.ini`中，请在关闭DDTV的情况下进行修改
:::

## WebSocket实时推送

除REST API外，DDTV还在同端口提供WebSocket服务，路径为`/ws`（默认完整地址`ws://127.0.0.1:11419/ws`），用于向WEBUI和第三方程序实时推送开播、录制状态、日志等消息。  
