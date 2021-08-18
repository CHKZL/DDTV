/**
* ddtv data_cli 用于进行api数据更新 隔离视图层和api请求的数据的一类函数
* (c) 2021 禾咕咕
* @param {arr or obj} 源数据
*/

export function isObjectEqual(obj1, obj2) {
    let o1 = obj1 instanceof Object;
    let o2 = obj2 instanceof Object;
    if (!o1 || !o2) {    // 如果不是对象 直接判断数据是否相等
        return obj1 === obj2
    }
    // 判断对象的可枚举属性组成的数组长度
    if (Object.keys(obj1).length !== Object.keys(obj2).length) {
        return false;
    }
    for (let attr in obj1) {
        let a1 = Object.prototype.toString.call(obj1[attr]) == '[object Object]'
        let a2 = Object.prototype.toString.call(obj2[attr]) == '[object Object]'
        let arr1 = Object.prototype.toString.call(obj1[attr]) == '[object Array]'
        if (a1 && a2) {
            // 如果是对象继续判断
            return isObjectEqual(obj1[attr], obj2[attr])
        } else if (arr1) {
            // 如果是对象 判断
            if (obj1[attr].toString() != obj2[attr].toString()) {
                return false;
            }
        } else if (obj1[attr] !== obj2[attr]) {
            // 不是对象的就判断数值是否相等
            return false
        }
    }
    return true
}

export async function room_data(self, arr) {
    // 遍历本地数据 初始化 roomid 和 index 对应的索引
    let datalen = self.room_list.length,
        dataslent = []
    // 开始生成本地渲染列表的索引
    if (datalen != 0) { for (var i = 0; i < datalen; i++) { dataslent.push(self.room_list[i].roomid) } }
    // 遍历API数据 初始化 roomid 和 index 对应的索引
    let arrlen = arr.length,
        arrslent = []
    // 开始生成API列表的索引
    if (arrlen != 0) { for (var j = 0; j < arrlen; j++) { arrslent.push(arr[j].唯一码) } }
    // 拷贝数组 得到两个数组 独有roomid的新数组
    let _dataSet = new Set(dataslent),
        _arrSet = new Set(arrslent),
        onlyData = dataslent.filter(item => !_arrSet.has(item)),
        onlyArr = arrslent.filter(item => !_dataSet.has(item))
    // 删除本地渲染列表 中过期的数据
    let onlydatalen = onlyData.length
    if (onlydatalen != 0) {
        for (var k = 0; k < onlydatalen; k++) {
            let dleindex = dataslent.indexOf(onlyData[k]);
            self.room_list.splice(dleindex, 1);
            dataslent.splice(dleindex, 1);
        }
    }
    // 删除掉相同的数据后，进行数据更新
    let dataslentlen = dataslent.length
    if (dataslentlen != 0) {
        for (var p = 0; p < dataslentlen; p++) {
            let updatindex = arrslent.indexOf(dataslent[p]),
                ordata = arr[updatindex],
                ondata = self.room_list[p]
            if (ordata.直播状态 != ondata.islive) { self.room_list[p].islive = ordata.直播状态 }
            if (ordata.平台 != ondata.table) { self.room_list[p].table = ordata.平台 }
            if (ordata.名称 != ondata.name) { self.room_list[p].name = ordata.名称 }
            if (ordata.原名 != ondata.orname) { self.room_list[p].orname = ordata.原名 }
            if (ordata.是否录制 != ondata.rec) { self.room_list[p].rec = ordata.是否录制 }
        }
    }
    // 新增 onlyArr
    let onlyArrlen = onlyArr.length;
    if (onlyArrlen != 0) {
        for (var l = 0; l < onlyArrlen; l++) {
            let addindex = arrslent.indexOf(onlyArr[l]),
                adddata = arr[addindex],
                pushdata = {
                    "islive": adddata.直播状态,
                    "table": adddata.平台,
                    "name": adddata.名称,
                    "orname": adddata.原名,
                    "rec": adddata.是否录制,
                    "roomid": adddata.唯一码,
                    "like": adddata.Like,
                    "show": true,
                    "loading": false
                }
            console.debug(pushdata)
            self.room_list.push(pushdata);
        }
    }
    console.debug(self.room_list)
}

export function sys_info_pr(self, obj) {
    console.log(self.system_info_data)
    if (!isObjectEqual(self.system_info_data, obj)) { self.system_info_data = obj }
    else console.log("数据没有发生改变")
}

export const sys_data_ex = {
    "DDTVCore_Ver": "--",
    "Room_Quantity": "--",
    "ServerName": "--",
    "ServerAID": "--",
    "ServerGroup": "--",
    "os_Info": {
        "OS_Ver": "--",
        "OS_Tpye": "--",
        "Memory_Usage": 0,
        "Runtime_Ver": "--",
        "UserInteractive": "--",
        "Associated_Users": "--",
        "Current_Directory": "--",
        "AppCore_Ver": "--",
        "WebCore_Ver": "--"
    },
    "download_Info": {
        "Downloading": "--",
        "Completed_Downloads": "--"
    },
    "ver_Info": {
        "IsNewVer": "--",
        "NewVer": "--",
        "Update_Log": "--"
    }
}
export const sys_mon_ex = {
    "reload":true,
    "Platform": "p",
    "DDTV_use_memory": 0,
    "CPU_usage": 0,
    "Available_memory": 0,
    "Total_memory": 0,
    "HDDInfo": [
        {
            "FileSystem": "",
            "Size": "0T",
            "Used": "66%",
            "Avail": "0T",
            "Usage": "0T",
            "MountPath": "/"
        }
    ]
}


