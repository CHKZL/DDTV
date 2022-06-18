# 食用指南

使用`ddtv/cli` `ddtv/deps` `ddtv/webserver`镜像请查看官网详细内容

使用`ddtv/monitor`镜像请查看 monitor 目录

## docker常用命令

- 查看日志

```shell
docker logs DDTV_WEB_Server # 打印所有日志
docker logs -f DDTV_WEB_Server # 跟踪日志
```

- 获取容器文件

```shell
docker ps -a
docker cp [DDTV CONTAINER ID]:/DDTV/* .
```
