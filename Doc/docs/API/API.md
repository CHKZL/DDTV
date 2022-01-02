# API操作

## 说明
API操作一般是用于程序或者二次开发调用，每个接口都需要根据配置文件中的`AccessKeyId`和`AccessKeySecret`根据一定的规则计算出来`sig`值并附带在请求中，每个接口都可以根据这个逻辑直接调用，接口之间没有调用关系依赖。  
每个接口调用都需要拥有全部公共参数和接口私有参数才可以使用

## 请求流程说明

1.把`accesskeyid`、`accesskeysecret`、`cmd`、`time`根据字母顺序且全部小写用键值对的形式连接在一起用`;`分割，形成原始字符串。
2.用步骤1产生的原始字符串进行SHA1散列加密的到`sig`
3.在请求的时候除了接口的必要参数，还在请求体中带上`cmd`、`time`、`accesskeyid`、`sig`一起提交即可

例：调用api/Room_Info接口
假设accesskeyid为"1"，accesskeysecret值为"2"，那么请求体如下
>- Request:
>```text
>method: POST
>path: http://127.0.0.1:11419/api/Room_Info
>```
>```json
>"form-data":
>{
>    "time":1641149566,
>    "cmd":"Room_Info",
>    "sig":"566a322043b6217334bc15f4e6d18973d033aa4b",
>    "accesskeyid":1
>}

在这个请求中，用于计算sig的原始字符串就是`accesskeyid=1;accesskeysecret=2;cmd=room_info;time=1641149566`

## 公共参数

|参数名|格式|是否必须|解释|
|:--:|:--:|:--:|--|
|time|int|是|请求发出时的UTC时间戳(注意是UTC时间不需要+8小时)，单位为秒，和服务器时间300秒以内的为有效请求(如:1626508097)|
|cmd|string|是|请求的API接口的接口名称(如:Room_Info)|
|sig|string|是|其他变量排序后按照规则拼接过后使用SHA1散列加密后得到的签名|
|accesskeyid|int|是|用于加密字符串的验证KeyId，存在于配置文件中需要和accesskeysecret成对的使用|