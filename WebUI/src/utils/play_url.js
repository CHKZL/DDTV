import sha1 from "sha1"


/**
* 生成播放链接
*
* @param {name} 文件名
* @param {path} 文件路径
* @return 字符串
*/
export function play_str(name,path) {
    let token = sessionStorage.getItem("token"),
        ver = sessionStorage.getItem("ver"),
        time = parseInt(new Date().getTime() / 1000),
        sigstr = `cmd=file_steam&Directory=${path}&File=${name}&time=${time}&token=${token}&ver=${ver}`,
        ps = `cmd=file_steam&Directory=${path}&File=${name}&time=${time}&ver=${ver}`,
        shadata = sha1(sigstr).toLocaleUpperCase(),
        url = `${window.apiObj.apiUrl}/tmp/${path}/${name}?${ps}&sig=${shadata}`

    return url
}