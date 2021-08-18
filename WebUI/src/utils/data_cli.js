/**
* ddtv data_cli 用于进行api数据更新 隔离视图层和api请求的数据的一类函数
* (c) 2021 禾咕咕
* @param {arr} 源数据数组
*/
export async function room_data(self,arr) {
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
                    "like":adddata.Like,
                    "show": true,
                    "loading": false
                }
            console.debug(pushdata)
            self.room_list.push(pushdata);
        }
    }
    console.debug(self.room_list)
}
