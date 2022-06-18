#!/bin/bash
# 在CLI WEBServer WEBUI 启动之前，检查文件
# 可用参数有:
#   --verbose
set -e; set -u

DDTV_Path=/DDTV
Backups_Path=/DDTV_Backups
WEBUI_Config_Path=${WEBUI_Path:-/DDTV}/static
RoomListConfig=${RoomListConfig:-"./RoomListConfig.json"}

ARGs="$*"
# Use in the the functions: eval $invocation
invocation='say_verbose "Calling: ${FUNCNAME[0]}"'
say_verbose() { if [[ "$ARGs" == *"--verbose"* ]]; then printf "\n%b\n" "$0: $1"; fi }

checkup() {
    eval "$invocation"

    case ${DDTV_Docker_Project:-WTF} in
        DDTV_CLI)
            check_dir_DDTV
            check_file_BiliUser_ini
            check_file_DDTV_Config_ini
            check_file_RoomListConfig_json
            ;;
        DDTV_WEB_Server)
            check_dir_DDTV
            #if [ ! -e "/NotIsFirstStart" ]; then
            #    check_file_config_js
            #    check_file_barinfo_js
            #fi
            check_file_BiliUser_ini
            check_file_DDTV_Config_ini
            check_file_RoomListConfig_json
            ;;
        DDTV_WEBUI)
            check_dir_DDTV
            if [ ! -e "/NotIsFirstStart" ]; then
                check_file_config_js
                check_file_barinfo_js
            fi
            ;;
        *)
            echo "Error DDTV Docker Project!" && exit 1
            ;;
    esac

    if [ ! -e "/NotIsFirstStart" ]; then
        touch /NotIsFirstStart
        echo "IsFirstStart!"
    else
        echo "NotFirstStart!"
    fi
}

# 检测 DDTV 目录文件是否齐全
check_dir_DDTV() {
    eval "$invocation"

    cd "$DDTV_Path" || eval 'echo "不存在目录: $DDTV_Path" && exit 1'
    find "$Backups_Path" -follow -print | sort -V | while read -r file; do
        if [ ! -e "${file//$Backups_Path/$DDTV_Path}" ]; then
            cp -vur "$file" "${file//$Backups_Path/$DDTV_Path}"
        fi
    done
}

# 写入 RoomListConfig.json
# 可用参数有:
#   $RoomListConfig
#   $RoomList
check_file_RoomListConfig_json() {
    eval "$invocation"

    if [[ "$RoomListConfig" == "./"* ]]; then
        File_Path="$DDTV_Path/${RoomListConfig#./}"
    else
        File_Path=$RoomListConfig
    fi

    if [ ! -e "$File_Path" ]; then
        if [ -n "${RoomList:-}" ]; then
            echo -e "$RoomList" > "$File_Path" && echo "已写入 $File_Path 。"

            say_verbose "$File_Path:\n$(
                cat "$File_Path"
            )"
        fi
    fi
}

# 写入 BiliUser.ini
# 可用参数有: 
#   $BiliUser
check_file_BiliUser_ini() {
    eval "$invocation"

    File_Path=$DDTV_Path/BiliUser.ini
    if [ ! -e "$File_Path" ]; then
        if [ -n "${BiliUser:-}" ]; then
            echo -e "$BiliUser" > $File_Path && echo "已写入 $File_Path 。"

            say_verbose "$File_Path:\n$(
                cat "$File_Path"
            )"
        fi
    fi
}

# 写入 DDTV_Config.ini
# 可用参数有:
#   $arg_name 
#       e.g. ${arg_name:+"arg_name=$arg_name"}
check_file_DDTV_Config_ini() {
    eval "$invocation"

    File_Path=$DDTV_Path/DDTV_Config.ini
    if [ ! -e "$File_Path" ]; then
# CoreConfigClass.cs Start
        echo "
[Core]
${RoomListConfig:+"RoomListConfig=$RoomListConfig"}
${GUI_FirstStart:+"GUI_FirstStart=$GUI_FirstStart"}
${WEB_FirstStart:+"WEB_FirstStart=$WEB_FirstStart"}
${IsAutoTranscod:+"IsAutoTranscod=$IsAutoTranscod"}
${TranscodParmetrs:+"TranscodParmetrs=$TranscodParmetrs"}
${WebHookUrl:+"WebHookUrl=$WebHookUrl"}
${ClientAID:+"ClientAID=$ClientAID"}
[WEB_API]
${WEB_API_SSL:+"WEB_API_SSL=$WEB_API_SSL"}
${pfxFileName:+"pfxFileName=$pfxFileName"}
${pfxPasswordFileName:+"pfxPasswordFileName=$pfxPasswordFileName"}
${WebUserName:+"WebUserName=$WebUserName"}
${WebPassword:+"WebPassword=$WebPassword"}
${AccessKeyId:+"AccessKeyId=$AccessKeyId"}
${AccessKeySecret:+"AccessKeySecret=$AccessKeySecret"}
${ServerAID:+"ServerAID=$ServerAID"}
${ServerName:+"ServerName=$ServerName"}
${AccessControlAllowOrigin:+"AccessControlAllowOrigin=$AccessControlAllowOrigin"}
${AccessControlAllowCredentials:+"AccessControlAllowCredentials=$AccessControlAllowCredentials"}
${CookieDomain:+"CookieDomain=$CookieDomain"}
[Download]
${DownloadPath:+"DownloadPath=$DownloadPath"}
${TmpPath:+"TmpPath=$TmpPath"}
${DownloadDirectoryName:+"DownloadDirectoryName=$DownloadDirectoryName"}
${DownloadFileName:+"DownloadFileName=$DownloadFileName"}
${RecQuality:+"RecQuality=$RecQuality"}
${IsRecDanmu:+"IsRecDanmu=$IsRecDanmu"}
${IsRecGift:+"IsRecGift=$IsRecGift"}
${IsRecGuard:+"IsRecGuard=$IsRecGuard"}
${IsRecSC:+"IsRecSC=$IsRecSC"}
${IsFlvSplit:+"IsFlvSplit=$IsFlvSplit"}
${FlvSplitSize:+"FlvSplitSize=$FlvSplitSize"}
${DoNotSleepWhileDownloading:+"DoNotSleepWhileDownloading=$DoNotSleepWhileDownloading"}
${Shell:+"Shell=$Shell"}
" > "$File_Path" &&
# CoreConfigClass.cs End
    echo "已写入 $File_Path 。"
    say_verbose "$File_Path:\n$(
        cat < "$File_Path" | grep -v '^$'
    )"
    fi
}

# 第一次启动配置前端文件config.js
# 可用参数有: 
#   $apiUrl
#   $mount
check_file_config_js() {
    eval "$invocation"

    File_Path=$WEBUI_Config_Path/config.js
    if [ -n "$apiUrl" ]; then
        if [[ "$apiUrl" == "false" ]]; then
            sed -i "/apiUrl/s|:.*,|: $apiUrl,|"     "$File_Path"
        else
            sed -i "/apiUrl/s|:.*,|: \"$apiUrl\",|" "$File_Path"
        fi
    fi
    if [ -n "$mount" ]; then
        sed -i "/mount/s|'.*'|'$mount'|" "$File_Path"
    fi

    echo "已写入 $File_Path 。"
    say_verbose "$File_Path:\n$(
        cat "$File_Path"
    )"
}

# 第一次启动配置前端文件barinfo.js
# 可用参数有: 
#   $show
#   $infoshow $ICPshow $GAshow
#   $infotext $ICPtext $GAtext
#   $infolink $ICPlink $GAlink
check_file_barinfo_js() {
    eval "$invocation"

    File_Path=$WEBUI_Config_Path/barinfo.js
    if [ -n "$show"     ]; then
        sed -i "/    show/s|: .*,|: $show,|"                         "$File_Path" ; fi
    if [ -n "$infoshow" ]; then
        sed -i "/info.*show/s|show:.*text|show: $infotext, text|"    "$File_Path" ; fi
    if [ -n "$infotext" ]; then
        sed -i "/info.*text/s|text.*link|text: \"$infotext\", link|" "$File_Path" ; fi
    if [ -n "$infolink" ]; then
        sed -i "/info.*link/s|link.*}|link: \"$infolink\" }|"        "$File_Path" ; fi
    if [ -n "$ICPshow"  ]; then
        sed -i "/ICP.*show/s|show.*text|show: $ICPshow, text|"       "$File_Path" ; fi
    if [ -n "$ICPtext"  ]; then
        sed -i "/ICP.*show/s|text.*link|show: \"$ICPtext\", link|"   "$File_Path" ; fi
    if [ -n "$ICPlink"  ]; then
        sed -i "/ICP.*link/s|link.*}|link: \"$ICPlink\" }|"          "$File_Path" ; fi
    if [ -n "$GAshow"   ]; then
        sed -i "/GA.*show/s|show.*text|show: $GAshow, text|"         "$File_Path" ; fi
    if [ -n "$GAtext"   ]; then
        sed -i "/GA.*show/s|text.*link|show: \"$GAtext\", link|"     "$File_Path" ; fi
    if [ -n "$GAlink"   ]; then
        sed -i "/GA.*link/s|link.*}|link: \"$GAlink\" }|"            "$File_Path" ; fi

    echo "已写入 $File_Path 。"
    say_verbose "$File_Path:\n$(
        cat "$File_Path"
    )"
}

checkup
