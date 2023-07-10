# DDTV_Docker版使用教程（Linux）

Docker 镜像在 [Docker Hub](https://hub.docker.com/u/ddtv) 和 [GitHub Container registry](https://github.com/CHKZL?tab=packages&repo_name=DDTV) 上提供。

两个位置提供的镜像完全一样，都是对 DDTV 发行版本的简单包装。

## 先决条件
  - Linux
  - 容器引擎，如 Docker-ce 18.03 或更高版本 ([安装教程](https://mirrors.tuna.tsinghua.edu.cn/help/docker-ce/))、Podman 等

:::tip DockerHub 更换镜像源
[DockerHub 更换镜像源教程](https://www.daocloud.io/mirror)
:::

## 可用镜像

| Docker 项目名 | 特点 |
| :---- | :---- |
| DDTV_CLI | 重启容器即可更新 DDTV |
| DDTV_WEB_Server | 重启容器即可更新 DDTV |
| DDTV_Deps | 只含 DDTV 必须依赖 |
<!--
| DDTV_WEBUI | 支持`amd64` `arm64v8` `arm32v7` `386` `arm32v6` `ppc64le` `s390x`架构 |
-->

:::tip 在容器中更新 DDTV
重启容器即可，配置`AutoInsallUpdate`目前仅供DDTV_GUI使用。
:::

#### 镜像名

| Docker 项目名 | GHCR 镜像名 | Docker Hub 镜像名 |
| :---- | :---- | :---- |
| DDTV_CLI | ghcr.io/chkzl/ddtv/cli | ddtv/cli |
| DDTV_WEB_Server | ghcr.io/chkzl/ddtv/webserver | ddtv/webserver |
| DDTV_Deps | ghcr.io/chkzl/ddtv/deps | ddtv/deps |
<!--
| DDTV_WEBUI | ghcr.io/chkzl/ddtv/webui | ddtv/webui |
-->

#### CLI / WEB_Server 支持的架构及可用标签

| 系统 \ 架构 | amd64 | arm64v8 | arm32v7 | 可用标签 |
| :---- | :----: | :----: | :----: | :---- |
| alpine | ✅ | ❓ <sup>1</sup> | ❓ <sup>1</sup> | `alpine` `3.0-alpine` `3.0.*.*-alpine` |
| debian | ✅ | ✅ | ✅ | `latest` `debian` `3.0` `3.0.*.*` |

:::warning Tip 1
受 DDTV 依赖影响，目前 alpine arm 下 DDTV 的~~日志功能~~、`二维码功能`无法使用，~~并因此存在内存泄露问题~~（日志及内存泄露问题应该解决了，等个有缘人测试.jpg），不建议使用。
:::

## 最佳实践

查阅修改`docker-ddtv.env`后运行

#### 方法一：修改`docker-compose.yml`后自行运行

```shell
wget https://raw.githubusercontent.com/moomiji/docker-ddtv/docker/docker-ddtv.env
nano docker-ddtv.env

wget https://raw.githubusercontent.com/moomiji/docker-ddtv/docker/docker-compose.yml
nano docker-compose.yml
> version: '3'
> services:
>   DDTV.service_name:
>     env_file:
>       - docker-ddtv.env
> ...
# env_file 优先级低于 environment 指定的环境变量
```

#### 方法二：使用参数`--env-file`运行

<!--
看官方文档是提供了的，但实际测试没成功过，有没有佬来修下

- docker-compose
```shell
wget https://raw.githubusercontent.com/moomiji/docker-ddtv/docker/docker-ddtv.env
nano docker-ddtv.env

wget https://raw.githubusercontent.com/moomiji/docker-ddtv/docker/docker-compose.yml
sudo docker-compose --env-file ./docker-ddtv.env up
```
-->

- docker cli
```shell
wget https://raw.githubusercontent.com/moomiji/docker-ddtv/docker/docker-ddtv.env
vi docker-ddtv.env
sudo docker run --env-file=./docker-ddtv.env ...
```

## docker cli 运行容器

#### 运行 DDTV_WEB_Server

```shell
sudo docker volume create DDTV_Rec
sudo docker run -d -p 11419:11419 \ # \后面不能有字符
        -v DDTV_Rec:/DDTV/Rec     \ # 必须，挂载卷或文件夹；否则会挂载匿名卷
        -v ${CONFIG_DIR}:/DDTV    \ # 可选，持久化 DDTV_WEB_Server 与 配置文件
        -e PUID=$(id -u)          \
        -e PGID=$(id -g)          \
        --name DDTV_WEB_Server    \
        ghcr.io/chkzl/ddtv/webserver
# 删除容器
sudo docker rm -f DDTV_WEB_Server
# 删除录制文件
sudo docker volume rm DDTV_Rec
```

#### 运行 DDTV_CLI

```shell
sudo docker volume create DDTV_Rec
sudo docker run -d -p 11419:11419 \ # \后面不能有字符
        -v DDTV_Rec:/DDTV/Rec     \ # 必须，挂载卷或文件夹；否则会挂载匿名卷
        -v ${CONFIG_DIR}:/DDTV    \ # 可选，持久化 DDTV_CLI 与 配置文件
        -e PUID=$(id -u)          \
        -e PGID=$(id -g)          \
        --name DDTV_CLI           \
        ghcr.io/chkzl/ddtv/cli
# 删除容器
sudo docker rm -f DDTV_CLI
# 删除录制文件
sudo docker volume rm DDTV_Rec
```

<!--
#### 在 DDTV_Deps 中运行 CLI 或 WEB_Server

```shell
Project=CLI
Project=WEB_Server
wget  https://github.com/CHKZL/DDTV/releases/latest/download/DDTV_${Project}.zip
unzip DDTV_${Project}.zip
mv    DDTV_${Project} DDTV
cat   启动说明.txt && rm 启动说明.txt
sudo docker run -d -p 11419:11419 \ # \后面不能有字符
        -v ${Rec_DIR}:/DDTV/Rec   \ # 可选，持久化录制文件
        -v ${PWD}/DDTV:/DDTV      \ # 必选，挂载 DDTV 目录
        -e PUID=$(id -u)          \
        -e PGID=$(id -g)          \
        --name DDTV_${Project}    \
        ghcr.io/chkzl/ddtv/deps:alpine /bin/bash -c "dotnet /DDTV/DDTV_${Project}.dll"
```
-->

## 单独挂载配置文件

#### CLI / WEB_Server

```shell
-v ${PWD}/DDTV:/DDTV    \ # 可选，持久化 DDTV_CLI 与 配置文件

# 替换成

-v ${PWD}/DDTV/RoomListConfig.json:/DDTV/RoomListConfig.json \
-v ${PWD}/DDTV/DDTV_Config.ini:/DDTV/DDTV_Config.ini         \
-v ${PWD}/DDTV/BiliUser.ini:/DDTV/BiliUser.ini               \
```

::: tip
若单独挂载RoomListConfig.json，环境变量RoomList、变量BiliUser 和 CLI / WEB_Server 配置文件变量将不可用。<sup>2</sup>
:::

## 可用环境变量

#### Docker 版独有环境变量

| 参数名 | 格式 | 默认值 | 说明 | 可用镜像 |
| ---- | ---- | ---- | ---- | ---- |
| TZ | `州/城市` | `Asia/Shanghai` | 时区 | `cli` `webserver` <!--`webui`--> |
| PUID | `num` | `0` | 运行 DDTV 的用户 ID | `cli` `webserver` |
| PGID | `num` | `0` | 运行 DDTV 的用户组 ID | `cli` `webserver` |
| RoomList <sup>2</sup> | `json` | `{"data":[]}` | 来自同名配置文件 | `cli` `webserver` |
| BiliUser <sup>2</sup> | `ini` | 无 | 来自同名配置文件 | `cli` `webserver` |
<!--
| WEBUI_Path | 路径 | `/DDTV/static` | WEBUI的文件夹路径 | `webui` |
| PROXY_PASS | `http(s)://you.host:port` | `http://127.0.0.1:11419` | 需要反代的后端地址, apiUrl=false 时 WEBUI 从反代地址联系 WEB_Server | `webui`|
-->

::: tip RoomList \ BiliUser 的食用方法
务必使用`单引号`括起变量；使用`\n`转义换行符，例如：
```shell
-e RoomList='{"data":[{"name":"蒂蒂媞薇_Official", "Description":"", "RoomId":21446992, "UID":408490081, "IsAutoRec":false, "IsRemind":false, "IsRecDanmu":false, "Like":false, "Shell":""}]}' \
-e BiliUser='cookie=...\nExTime=...\ncsrf=...\nuid=...' \
```
:::

#### CLI / WEB_Server 配置文件常用环境变量 <sup>2</sup>

参数名与[DDTV Core通用配置文件](/config/DDTV_Config.html)的配置名完全相同。

| 参数名 | 格式 | 默认值 | 说明 |
| ---- | ---- | ---- | ---- |
| RoomListConfig | 路径 | `./RoomListConfig.json` | RoomListConfig.json文件位置 |
| IsHls | `bool` | `True` | 是否优先使用HLS进行录制 |
| IsDev | `bool` | `False` | 是否使用开发版更新模式 |
| IsAutoTranscod | `bool` | `False` | 是否启用自动转码 |
| IsFlvSplit | `bool` | `False` | 是否启用全局FLV文件按大小切分开关，注：启动后自动合并、自动转码失效 |
| FlvSplitSize | `longint` | `1073741824` | 文件切分的大小(byte) |
| WebUserName | `string` | `ami` | WEB登陆使用的用户名 |
| WebPassword | `string` | `ddtv` | WEB登陆使用的密码 |
| Shell | `bool` | `False` | 用于控制下载完成后是否执行对应房间的Shell命令 |
| WebHookUrl | `string` | `string.Empty` | WebHook的目标地址 |

:::tip 使用dev开发版
法1：初次启动容器，可设置环境变量`IsDev=True`后启动容器 <sup>2</sup>，在登陆阿B（即配置文件写入）后关闭容器
<br>法2：停止容器，修改配置文件DDTV_Config.ini的配置`IsDev=True`
<br>之后启动容器获取dev更新
:::

:::warning Tip 2
变量只在`初次启动`，即`配置或配置文件不存在`时可用。
:::

<!--
#### WEBUI 配置文件变量<sup>3</sup>

| 参数名 | 格式 | 默认值 | 说明 |
| ---- | ---- | ---- | ---- |
| apiUrl | `bool` `http(s)://you.host:port` | `http://127.0.0.1:11419` | 后端地址, 同源也请更换为主机IP, 需要反代请填 false |
| mount | 路径 | `/` | 展示目录所在文件系统占用 |
| show | `bool` | `true` | 是否显示 |
| infoshow | `bool` | `true` | 是否显示版权信息 |
| infotext | `string` | 无 | 版权信息 |
| infolink | `string` | 无 | 版权信息跳转链接 |
| ICPshow | `bool` | `true` | 是否显示TCP备案信息 |
| ICPtext | `string` | 无 | TCP备案信息 |
| ICPlink | `string` | 无 | TCP备案信息跳转链接 |
| GAshow | `bool` | `true` | 是否显示公网安备信息 |
| GAtext | `string` | 无 | 公网安备信息 |
| GAlink | `string` | 无 | 公网安备信息跳转链接 |
-->

<!--
:::warning Tip 3
变量只在 `第一次启动时` 可用。
:::
-->

更多可用变量见 [官网配置说明](/config/DDTV_Config.html#配置说明) 与 [docker-ddtv.env](https://github.com/moomiji/docker-ddtv/blob/docker/docker-ddtv.env)。

## CLI / WEB_Server 可用运行参数

```shell
sudo docker run  \
     ...         \
     镜像[:标签] \
     --no-update \ # 容器重启后不自动更新 DDTV
     --verbose   \ # 脚本输出更多信息（若服务器多人使用docker，请谨慎使用该参数，因为会将DDTV中的个人信息\配置输出到docker日志中）
```
