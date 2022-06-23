#!/bin/bash
set -e; set -u
# $1: $*_REPO in Docker_Release.yml#L28-40
case $1 in
    ddtv/deps)
        exit 0
        ;;
    ddtv/cli)
        Keyword=CLI
        ;;
    ddtv/webserver)
        Keyword=Server
        ;;
    ddtv/webui)
        Keyword=Server
        is_nginx=true
        ;;
esac
KeyFile=DDTV_Core.dll

# 下载DDTV
wget --no-verbose https://api.github.com/repos/CHKZL/DDTV/releases/latest                               \
     && wget --no-verbose "$(cat < latest | awk '/download_url/{print $4}' FS='"' | grep -i $Keyword )" \
     && File_Path=$(         cat < latest | awk '/name/{print $4}' FS='"'         | grep -i $Keyword )  \
     && 7z x -bd "$File_Path"                                                                           \
     && echo "Extract ($File_Path) succeeded"                                                           \
     && File_Path=$(7z l "$File_Path" | awk "/$KeyFile/{print \$6}" | awk '{print $1}' FS="/$KeyFile")  \
     && echo "File path ($File_Path) geted"                                                             \
     || eval 'echo "Failed to get File path OR extract" && exit 1'

# 转移DDTV
mkdir -vp "$1/root/DDTV"
mv -v "$File_Path"               \
          "$1/root/DDTV_Backups"
mv -v ./*-checkup.sh             \
          "$1/root/docker-entrypoint.d"
mv -v ./docker-entrypoint.sh     \
          "$1/root"

shopt -s extglob
if [ -n "${is_nginx:-}" ]; then
    rm  "$1/root/docker-entrypoint.sh"
    cd  "$1/root/DDTV_Backups"
    rm -rf !(keep|keep2)
fi
