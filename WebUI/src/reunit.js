/**
* 将 UTC 时间戳换算为本地的时间字符串 yy-mm-dd hh:mm:ss
*
* @param {value} UTC时间戳（秒）
* @return 字符串
*/
export function formatDate(value) {
    let date = new Date(value  * 1000);
    let y = date.getFullYear();
    let MM = date.getMonth() + 1;
    MM = MM < 10 ? ('0' + MM) : MM;
    let d = date.getDate();
    d = d < 10 ? ('0' + d) : d;
    let h = date.getHours();
    h = h < 10 ? ('0' + h) : h;
    let m = date.getMinutes();
    m = m < 10 ? ('0' + m) : m;
    let s = date.getSeconds();
    s = s < 10 ? ('0' + s) : s;
    return y + '-' + MM + '-' + d + ' ' + h + ':' + m + ':' + s;
}
