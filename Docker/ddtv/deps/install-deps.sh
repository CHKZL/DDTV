#!/bin/sh
# WEBServer CLI 安装相关依赖
set -e; set -u

. /etc/os-release

case $ID in
    alpine)
        apk add --no-cache ffmpeg bash tzdata su-exec
        # apk add libgdiplus --no-cache --repository http://dl-cdn.alpinelinux.org/alpine/edge/testing/ --allow-untrusted
        sed -i 's/dl-cdn.alpinelinux.org/mirrors.aliyun.com/g' /etc/apk/repositories
        ;;

    debian|ubuntu)
        apt update
        apt install --no-install-recommends ffmpeg bash tzdata gosu -y # libgdiplus
        sed -i 's/deb.debian.org/mirrors.aliyun.com/g'     /etc/apt/sources.list
        sed -i 's/archive.ubuntu.com/mirrors.aliyun.com/g' /etc/apt/sources.list
        ;;
    *)
        echo "Error OS ID: $ID!" && exit 1
        ;;
esac

rm -rf /var/lib/apt/lists/* /var/cache/apk/* /root/.cache /tmp/* "$0"
