# 食用指南

该指南用于 ddtv 出现异常占用时，构建镜像并进行性能探测并记录给开发者分析。

## 构建与运行
```shell
git clone --depth 1 https://github.com/CHKZL/DDTV
DOTNET_VERSION=$(awk '/DOTNET_VERSION: /{print $2;exit}' FS="'" .github/workflows/Docker_Release.yml)
cd ./DDTV/Docker/monitor
# alpine
sudo docker build --rm -t ddtv/monitor:alpine --build-arg IMAGE_TAG=6.0-alpine .
sudo docker run -it ${与 运行容器 相同的参数&配置文件} --rm --network host --cap-add=SYS_ADMIN --cap-add=SYS_PTRACE --privileged --name ddtvmonitor ddtv/monitor:alpine
# debian
sudo docker build --rm -t ddtv/monitor:debian --build-arg IMAGE_TAG=6.0 .
sudo docker run -it ${与 运行容器 相同的参数&配置文件} --rm --network host --cap-add=SYS_ADMIN --cap-add=SYS_PTRACE --privileged --name ddtvmonitor ddtv/monitor:debian
```

## 性能探测与记录

- CPU 占用异常

当出现 CPU 占用异常时，运行以下命令 20~30 s，`Ctrl+C`停止 perf 记录。

```shell
# sudo docker exec -it --privileged ddtvmonitor /bin/bash
# perf record -p ${DDTV_WEB_Server进程号} -g
perf record -p $(ps -ef | grep -v "grep" | awk '/DDTV_WEB_Server/{print $1}') -g

# perf record -p ${DDTV_CLI进程号} -g
perf record -p $(ps -ef | grep -v "grep" | awk '/DDTV_CLI/{print $1}') -g
```

## perf 记录转换为火焰图

```shell
# DDTV_WEB_Server
. /etc/os-release
DDTV_WEB_Server_Version=$(cat /DDTV/DDTV_WEB_Server.deps.json | awk '/DDTV_WEB_Server\//{print $3}' FS='[/"]' | awk 'NR==1')
eval "perf script | FlameGraph/stackcollapse-perf.pl | FlameGraph/flamegraph.pl > DDTV.WEB_Server_Ver${DDTV_WEB_Server_Version}.in_${PRETTY_NAME}.svg"

# DDTV_CLI
. /etc/os-release
DDTV_CLI_Version=$(cat /DDTV/DDTV_CLI.deps.json | awk '/DDTV_CLI\//{print $3}' FS='[/"]' | awk 'NR==1')
eval "perf script | FlameGraph/stackcollapse-perf.pl | FlameGraph/flamegraph.pl > DDTV.CLI_Ver${DDTV_CLI_Version}.in_${PRETTY_NAME}.svg"
```
