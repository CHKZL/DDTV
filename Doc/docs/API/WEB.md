# WEB接口


## 说明
WEB操作一般由于固定页面的显示功能和前端页面的二次开发，在使用任意接口前，都需先调用`api/login`接口进行登录获取cookie，其他接口使用该cookie进行访问即可；也就是说所有的接口都依赖于login接口返回的cookie数据。  
WEB接口只需要在调用时带上接口的私有参数和公共参数的`cmd`即可，不用带上其他几个公共参数。

:::warning 注意
第二次调用`api/login`会替换掉之前的cookie数据，也就是说只能有一个用户登陆
:::

## 请求流程说明

1.调用`api/login`接口进行登陆
2.凭借步骤1返回的cookie即可访问其他接口

例：调用api/Room_Info接口
>- Request:
>```text
>method: POST
>path: http://127.0.0.1:11419/api/Room_Info
>cookie: xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
>```
>```json
>"form-data":
>{
>    "cmd":"Room_Info",
>}

## 公共参数

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|cmd|string|是|请求的API接口的接口名称(如:Room_Info)|