/****   request.js   ****/
import axios from 'axios'
import { Message} from 'element-ui';
import Router from '../router' 

const service = axios.create({
  // 起始连接
  baseURL: window.apiObj.apiUrl,
  // 超时时间 单位是ms，这里设置了3s的超时时间
  timeout: 3 * 1000
})

// 添加请求拦截器
// axios.interceptors.request.use(function (config) {
//     console.log("http")
//     if(config.method =='post'){
//         let param = new window.FormData();
//         for(var key in config.data){
//             param.append(key, config.data[key]);
//         }
//         config.data=param
//     }
//     config.headers['Content-Type'] = 'application/x-www-form-urlencoded';
//     return config;
// },function (error) {
//     return Promise.reject(error)
// })

service.interceptors.response.use(response => {
  if(response.config.url != "/api/weblogin" && response.data.code != 1001) {
    sessionStorage.clear();
    Router.push("/login");
  }
  return response
}, error => {
   /***** 接收到异常响应的处理开始 *****/
  if (error && error.response) {
    // 1.公共错误处理
    // 2.根据响应码具体处理
    switch (error.response.status) {
      case 400:
        error.message = '错误请求'
        break;
      case 401:
        error.message = '未授权，请重新登录'
        break;
      case 403:
        error.message = '拒绝访问'
        break;
      case 404:
        error.message = '请求错误,未找到该资源'
        window.location.href = "/NotFound"
        break;
      case 405:
        error.message = '请求方法未允许'
        break;
      case 408:
        error.message = '请求超时'
        break;
      case 500:
        error.message = '服务器端出错'
        break;
      case 501:
        error.message = '网络未实现'
        break;
      case 502:
        error.message = '网络错误'
        break;
      case 503:
        error.message = '服务不可用'
        break;
      case 504:
        error.message = '网络超时'
        break;
      case 505:
        error.message = 'http版本不支持该请求'
        break;
      default:
        error.message = `连接错误${error.response.status}`
    }
  } else {
    // 超时处理
    if (JSON.stringify(error).includes('timeout')) {
      Message.error('服务器响应超时，请刷新当前页')
    }
    error.message = '连接服务器失败'
  }

  Message.error(error.message)
  /***** 处理结束 *****/
  //如果不需要错误处理，以上的处理过程都可省略
  return Promise.resolve(error.response)
})
//4.导入文件
export default service
