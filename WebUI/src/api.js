import http from './utils/http'
import sha1 from "sha1"

let resquest = "/api"

/**
* 生成一个含有公共参数的请求体
*
* @param {type} 接口类型  str
* @return 一个json对象
*/
export function pubBody(type) {
    let time = parseInt(new Date().getTime() / 1000)
    let body = {
        "time":time,
        "cmd":type,
        "ver":1,
    }
    return body
}

/**
* 对json对象进行排序
*
* @param {obj} 需要排序的json对象  obj
* @return 排序好的json对象
*/
export function objKeySort(obj) {
    //先用Object内置类的keys方法获取要排序对象的属性名，
    //再利用Array原型上的sort方法对获取的属性名进行排序，newkey是一个数组
    var newkey = Object.keys(obj).sort();
    //创建一个新的对象，用于存放排好序的键值对
    var newObj = {};
    //遍历newkey数组
    for (var i = 0; i < newkey.length; i++) {
        //向新创建的对象中按照排好的顺序依次增加键值对
        newObj[newkey[i]] = obj[newkey[i]];
    }
    console.debug(newObj)
    return newObj;//返回排好序的新对象
}

/**
* 根据请求对象进行sig的计算
*
* @param {params} 请求对象  obj
* @return sig
*/
export function sig(params) {
    // 因为签名需要 token 的参与 下面先加入token
    let token = sessionStorage.getItem("token");
    params.token = token
    // 根据 json 数据的键进行排序
    let newparams = objKeySort(params)
    // 生成签名字符串
    let sigstr = ""
    let i = 0
    for(var key in newparams){
        if(i!=0) sigstr = sigstr + '&'
        sigstr = sigstr + `${key}=${newparams[key]}`
        i+=1
    }
    console.debug(sigstr)
    // 返回的表单中不能有 token
    delete params['token']
    // 对sig字符串进行加密
    let sig = sha1(sigstr).toLocaleUpperCase();
    // 写入 params
    params.sig = sig
    return params
}

/**
* 本函数实现了一个封装好的POST请求
* 
* @param {url} api接口名称  str
* @param {param} 请求体 obj
* @param {havesig} 是否加签 bool
* @return 请求返回的数据
*/
export function postFormAPI(url,params,havesig){
    if(havesig) params = sig(params)
    console.debug(params)
    let param = new window.FormData();
    for(var n in params){
        param.append(n, params[n]);
    }
    return http.post(`${resquest}/${url}`,param)
}




// get请求
export function getListAPI(url,params){
    return http.get(`${resquest}/${url}`,params)
}

// put 请求
export function putSomeAPI(url,params){
    return http.put(`${resquest}/${url}`,params)
}
// delete 请求
export function deleteListAPI(url,params){
    return http.delete(`${resquest}/${url}`,params)
}