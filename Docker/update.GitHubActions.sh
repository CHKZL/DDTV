#!/bin/bash
set -e;set -u
CoreConfigClass_path=./tmp.d/CoreConfigClass.cs
# 获取'public enum Key'所在行数
num_key_start=$(grep -n 'public enum Key' $CoreConfigClass_path | awk -F ":" '{print $1}')
# 获取'public enum Key'至结束的行数
num_key_start_to_end=$(grep -A 10000 'public enum Key' $CoreConfigClass_path | grep -n '    }' | awk -F ":" '{print $1;exit}')
# 获取结束所在行数
(( num_key_end = num_key_start + num_key_start_to_end - 1 ))

# 获取'public enum Group'至结束的行数（带注释）
tmp_vault=$(grep -A 10000 'public enum Group' $CoreConfigClass_path | grep -n 'public enum Key' | awk -F ":" '{print $1;exit}')
(( num_group_start_to_end = tmp_vault + num_key_start_to_end - 1 + 3 ))
# 获取'public class Config'至结束的行数
tmp_vault=$(grep -A 10000 'public class Config' $CoreConfigClass_path | grep -n 'public enum Key' | awk -F ":" '{print $1;exit}')
(( num_config_start_to_end = tmp_vault + num_key_start_to_end - 1 ))


# 获取组名数组
mapfile -t array_group_name    < <(grep -A "$num_key_start_to_end" 'public enum Key' $CoreConfigClass_path | sed 's/\r//g' | grep '组' | awk -F "：" '/默认值/{print $2}' | awk '{print $1}')
# 获取默认值数组
mapfile -t array_default_value < <(grep -A "$num_key_start_to_end" 'public enum Key' $CoreConfigClass_path | sed 's/\r//g' | grep '组' | awk -F "：" '/默认值/{print $3}' | sed 's/可选值//g' | sed 's/随机字符串/string.Empty/g' | awk '{$1=$1}1')
# 获取键名数组
mapfile -t array_key_name      < <(grep -A "$num_key_start_to_end" 'public enum Key' $CoreConfigClass_path | sed 's/\r//g' | awk '/,/{print $1}' | sed 's/,//g')
if [[ "${#array_group_name[*]}" != "${#array_default_value[*]}" || "${#array_group_name[*]}" != "${#array_key_name[*]}" ]]; then
    echo "数组长度不对等。" 
    exit 1
fi


# 输出至 ./Doc/docs/config/DDTV_Config.md
file_path=./Doc/docs/config/DDTV_Config.md
echo "file_path=$file_path"
tmp=$(grep -A 10000 "\[CoreConfigClass.cs\]: End" $file_path)
sed -i '/\[CoreConfigClass.cs\]: Start/Q' $file_path
echo "[CoreConfigClass.cs]: Start

\`\`\`C#
$(head -n $num_key_end $CoreConfigClass_path | tail -n $num_group_start_to_end)
\`\`\`

$tmp" >> $file_path


# 输出至 ./Doc/docs/API/README.md
file_path=./Doc/docs/API/README.md
echo "file_path=$file_path"
tmp=$(grep -A 10000 "\[CoreConfigClass.cs\]: End" $file_path)
sed -i '/\[CoreConfigClass.cs\]: Start/Q' $file_path
echo "[CoreConfigClass.cs]: Start

\`\`\`C#
$(head -n $num_key_end $CoreConfigClass_path | tail -n $num_config_start_to_end)
\`\`\`

$tmp" >> $file_path


# 输出至 ./docker-ddtv.env
file_path=./docker-ddtv.env
echo "file_path=$file_path"
sed -i '/# CLI 或 WEB_Server 配置文件变量/Q' $file_path
echo '# CLI 或 WEB_Server 配置文件变量' >>  $file_path
for (( i=0; i<${#array_group_name[*]}; i++ )); do
    echo "# ${array_key_name[$i]}=${array_default_value[$i]//string.Empty/}" >> $file_path
done

# 输出至 ./Docker/01-checkup.sh
rm -f "$CoreConfigClass_path"
file_path=./Docker/01-checkup.sh
echo "file_path=$file_path"
for (( i=0; i<${#array_group_name[*]}; i++ )); do
    echo "\${${array_key_name[$i]}:+\"${array_key_name[$i]}=\$${array_key_name[$i]}\"}" >> "./tmp.d/${array_group_name[$i]}"
done
tmp=$(grep -A 10000 "# CoreConfigClass.cs End" $file_path)
sed -i '/# CoreConfigClass.cs Start/Q' $file_path
echo '# CoreConfigClass.cs Start
        echo "'            >> $file_path
for i in $(ls ./tmp.d/); do
    echo "[$i]"            >> $file_path
    cat "./tmp.d/$i"       >> $file_path
done
echo '" > "$File_Path" &&' >> $file_path
echo "$tmp"                >> $file_path
rm -rf ./tmp.d
