#!/bin/bash
set -e; set -u

# 参数更新需修改 README.md docker-compose.yml
# 可用参数有:
#   --no-update 不更新 DDTV
#   --verbose   脚本输出更多信息（若服务器多人使用docker，请谨慎使用该参数，因为会将DDTV中的个人信息\配置输出到docker日志中）
ARGs="$*"
say_verbose() { if [[ "$ARGs" == *"--verbose"* ]]; then printf "\n%b\n" "$0: $1"; fi }

case "$ARGs" in
    ""|*"--verbose"*|*"--no-update"*)
        # 运行 /docker-entrypoint.d/*.sh
        find "/docker-entrypoint.d/" -follow -type f -print | sort -V | while read -r file; do
            case "$file" in
                *.sh)
                    if [ -x "$file" ]; then
                        say_verbose "Launching $file $*";
                        "$file" "$@"
                    else
                        say_verbose "Ignoring $file, not executable";
                    fi
                    ;;
                *)      say_verbose "Ignoring $file"
                    ;;
            esac
        done
        # 更新 DDTV
        cd /DDTV
        [[ "$ARGs" == *"--no-update"* ]] || (dotnet DDTV_Update.dll docker || echo "更新失败，请稍候重试！")
        ;;
        # 运行测试用命令
    *)  echo "eval $ARGs" && eval "$ARGs" && exit $?
        ;;
esac

# 运行 DDTV
# 可用参数有:
#   $PUID
#   $PGID
#   $DownloadPath
#   $TmpPath
. /etc/os-release
echo "Running as UID ${PUID:=$UID} and GID ${PGID:=$PUID}."
mkdir -p "${DownloadPath:=./Rec/}" "${TmpPath:=./tmp/}"
chown -R "$PUID:$PGID" /DDTV "$DownloadPath" "$TmpPath"

case $ID in
    alpine)
        su-exec $PUID:$PGID dotnet "${DDTV_Docker_Project}.dll"
        ;;
    debian|ubuntu)
        gosu $PUID:$PGID dotnet "${DDTV_Docker_Project}.dll"
        ;;
    *)
        echo "Error OS ID: $ID!" && exit 1
        ;;
esac
