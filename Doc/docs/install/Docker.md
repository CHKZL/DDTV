# DDTV_Docker版使用教程（Linux）

Docker 镜像在 [Docker Hub](https://hub.docker.com/u/ddtv) 和 [GitHub Container registry](https://github.com/CHKZL?tab=packages&repo_name=DDTV) 上提供。

两个位置提供的镜像完全一样，都是对 DDTV 发行版本的简单包装。

## 先决条件
  - Linux
  - 容器引擎，如 Docker-ce 18.03 或更高版本 ([安装教程](https://mirrors.tuna.tsinghua.edu.cn/help/docker-ce/))、Podman 等

#### 镜像名

| Docker 项目名 | GHCR 镜像名 | Docker Hub 镜像名 |
| :---- | :---- | :---- |
| DDTV_Server | ghcr.io/chkzl/ddtv/server | ddtv/server |

#### Server 支持的架构及可用标签

| 系统 \ 架构 | amd64 | arm64v8 | arm32v7 | 可用标签 |
| :---- | :----: | :----: | :----: | :---- |
| debian | ✅ | ✅ | ✅ | `latest` `debian` `5.*` `5.*.*` |
| alpine | ✅ | ✅ | ✅ | `alpine` `5.*-alpine` `5.*.*-alpine` |

## 最佳实践

## docker cli 运行容器

#### 运行 DDTV_Server

```shell
sudo docker run -d -p 11419:11419     \ # \后面不能有字符
        -v ${PWD}/DDTV_Rec:/DDTV/Rec  \ # 持久化录制文件
        -v ${CONFIG_DIR}:/DDTV/Config \ # 持久化配置文件
        -v ${LOGS_DIR}:/DDTV/Logs     \ # 持久化日志文件
        -e PUID=$(id -u)              \
        -e PGID=$(id -g)              \
        --name DDTV_Server            \
        ghcr.io/chkzl/ddtv/server
# 删除容器
sudo docker rm -f DDTV_Server
```

## 可用环境变量

#### Docker 版独有环境变量

| 参数名 | 格式 | 默认值 | 说明 | 可用镜像 |
| ---- | ---- | ---- | ---- | ---- |
| TZ | `州/城市` | `Asia/Shanghai` | 时区 | `server` |
| PUID | `num` | `0` | 运行 DDTV 的用户 ID | `server` |
| PGID | `num` | `0` | 运行 DDTV 的用户组 ID | `server` |

:::tip alpine 镜像设置时区
alpine 镜像未安装tzdata，如有需要请挂载时区信息文件，一般如下：
```
-v /usr/share/zoneinfo:/usr/share/zoneinfo
```
:::

#### server 可用配置文件变量

参数名与[DDTV Core通用配置文件](/config/DDTV_Config.html)的配置名完全相同。

| 参数名 | 格式 | 默认值 | 说明 |
| ---- | ---- | ---- | ---- |
| DevelopmentVersion | `bool` | `false` | 是否使用开发版更新模式 |
| IP | `string` | `http://0.0.0.0` | 监听地址 |

## Server 可用运行参数

```shell
sudo docker run  \
     ...         \
     镜像[:标签] \
     --no-update \ # 容器重启后不自动更新 DDTV
     --verbose   \ # 脚本输出更多信息
```
