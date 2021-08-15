# 一个普通的web ui
> ~~年轻人的第一个web项目~~

## 一、项目介绍

### 1、介绍
本项目使用 辽ddtv 的webapi 开发了一套与之相匹配的在线管理页面。

### 2、基本功能
查看系统当前状况；查看正在录制的任务并能结束；添加、删除房间；开启、关闭自动录制；删除文件；查看目前的文件；在线播放。

TODO:上传状态查看、日志分析、细化文件管理、重构播放页面
### 3、环境
本项目基于**纯天然、植物饲养的**Vue 2.0
### 4、配置
您需要打开 public 下的 static 文件夹，找到config.js
如果您使用的是编译好的包 请您在 static 文件夹，找到config.js 即可
```js
window.apiObj = {
  // 没事别乱加斜杠 域名最 后面没有斜杠
    apiUrl:'https://test.你的域名.com:11451',
    apiName: 'ddtv后端的地址',
    apiArea: '请根据您的部署情况，在上面填上合适的url'
  }
```
# 二、使用

## 1、自行构建
Project setup
```
npm install
```
Compiles and hot-reloads for development
```
npm run serve
```
Compiles and minifies for production
```
npm run build
```
Lints and fixes files
```
npm run lint
```

# 三、贡献
## 1、贡献
如果您发现了更好的使用方法，不妨分享出来！你可以使用pr功能提交请求，我会审阅。或者在使用中出现了什么问题，都可以提交issue。
