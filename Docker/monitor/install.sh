#!/bin/sh
set -e; set -u

. /etc/os-release

case $ID in
    alpine)
        sed -i 's/dl-cdn.alpinelinux.org/mirrors.aliyun.com/g' /etc/apk/repositories
        apk add --no-cache perf perl git
        git clone --depth=1 https://github.com/BrendanGregg/FlameGraph
        apk add --no-cache ffmpeg bash tzdata su-exec
        apk add libgdiplus --no-cache --repository http://dl-cdn.alpinelinux.org/alpine/edge/testing/ --allow-untrusted
        ;;

    debian|ubuntu)
        sed -i 's/deb.debian.org/mirrors.aliyun.com/g'     /etc/apt/sources.list
        sed -i 's/archive.ubuntu.com/mirrors.aliyun.com/g' /etc/apt/sources.list
        apt update
        apt install --no-install-recommends linux-perf perl git -y && mv /usr/bin/perf* /usr/bin/perf
        git clone --depth=1 https://github.com/BrendanGregg/FlameGraph
        apt install --no-install-recommends ffmpeg bash tzdata gosu libgdiplus -y
        apt clean -y
        ;;
    *)
        echo "Error OS ID: $ID!" && exit 1
        ;;
esac
rm -rf /var/lib/apt/lists/* /var/cache/apk/* /root/.cache /tmp/*
echo "dotnet tool 安装中……"
dotnet tool install --no-cache --tool-path /tools dotnet-counters
dotnet tool install --no-cache --tool-path /tools dotnet-coverage
dotnet tool install --no-cache --tool-path /tools dotnet-dump
dotnet tool install --no-cache --tool-path /tools dotnet-gcdump
dotnet tool install --no-cache --tool-path /tools dotnet-trace
dotnet tool install --no-cache --tool-path /tools dotnet-stack
dotnet tool install --no-cache --tool-path /tools dotnet-symbol
dotnet tool install --no-cache --tool-path /tools dotnet-sos
dotnet tool install --no-cache --tool-path /tools dotnet-monitor
echo "dotnet tool 安装完成。"
