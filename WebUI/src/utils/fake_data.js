/*
 * 用于开发的假数据
 */

export const fake_up_data = {
    "code": 1001,
    "message": "请求成功",
    "queue": 1,
    "Package": [
        {
            "files": {
                "flv": {
                    "tasks": {
                        "BaiduPan": {
                            "fileType": 0,
                            "taskType": 2,
                            "fileName": "1.flv",
                            "localPath": "E://",
                            "remotePath": "UplaodTest_SteamerName_UplaodTest_RoomChannel/19700101_UplaodTest_StreamTitle/",
                            "startTime": 1629235345, // -1 还没开始
                            "endTime": -1,
                            "retries": 1,
                            "statusCode": 3,
                            "comments": "[1] ↑ 432.00MB/880.02MB 916.94KB/s in 9s ............",
                            "progress": 50
                        }
                    },
                    "fileName": "1.flv",
                    "localPath": "E://",
                    "remotePath": "UplaodTest_SteamerName_UplaodTest_RoomChannel/19700101_UplaodTest_StreamTitle/",
                    "fileType": 0,
                    "currentTask": 0,
                    "startTime": 1629235345, // 开始时间
                    "endTime": -1,      // 结束时间
                    "fileSize": 922768190.0, // 文件大小
                    "statusCode": 3
                },
                "gift": {
                    "tasks": {
                        "BaiduPan": {
                            "fileType": 1,
                            "taskType": 2,
                            "fileName": "1.flv.txt",
                            "localPath": "E://",
                            "remotePath": "UplaodTest_SteamerName_UplaodTest_RoomChannel/19700101_UplaodTest_StreamTitle/",
                            "startTime": -1,
                            "endTime": -1,
                            "retries": -1,
                            "statusCode": 2,
                            "comments": "",
                            "progress": -1
                        }
                    },
                    "fileName": "1.flv.txt",
                    "localPath": "E://",
                    "remotePath": "UplaodTest_SteamerName_UplaodTest_RoomChannel/19700101_UplaodTest_StreamTitle/",
                    "fileType": 1,
                    "currentTask": 0,
                    "startTime": -1,
                    "endTime": -1,
                    "fileSize": 19016843.0,
                    "statusCode": 2
                },
                "danmu": {
                    "tasks": {
                        "BaiduPan": {
                            "fileType": 2,
                            "taskType": 2,
                            "fileName": "1.xml",   // 文件名
                            "localPath": "E://",   // 本地文件夹
                            "remotePath": "UplaodTest_SteamerName_UplaodTest_RoomChannel/19700101_UplaodTest_StreamTitle/", // 上传文件夹
                            "startTime": -1,
                            "endTime": -1,
                            "retries": -1,
                            "statusCode": 2,
                            "comments": "", //
                            "progress": -1  // 进度
                        }
                    },
                    "fileName": "1.xml",
                    "localPath": "E://",
                    "remotePath": "UplaodTest_SteamerName_UplaodTest_RoomChannel/19700101_UplaodTest_StreamTitle/",
                    "fileType": 2,
                    "currentTask": 0,
                    "startTime": -1,
                    "endTime": -1,
                    "fileSize": 19016843.0,
                    "statusCode": 2
                },
                "mp4": {
                    "tasks": {
                        "BaiduPan": {
                            "fileType": 3,
                            "taskType": 2,
                            "fileName": "1.mp4",
                            "localPath": "E://",
                            "remotePath": "UplaodTest_SteamerName_UplaodTest_RoomChannel/19700101_UplaodTest_StreamTitle/",
                            "startTime": -1,
                            "endTime": -1,
                            "retries": -1,
                            "statusCode": 2,
                            "comments": "",
                            "progress": -1
                        }
                    },
                    "fileName": "1.mp4",
                    "localPath": "E://",
                    "remotePath": "UplaodTest_SteamerName_UplaodTest_RoomChannel/19700101_UplaodTest_StreamTitle/",
                    "fileType": 3,
                    "currentTask": 0,
                    "startTime": -1,
                    "endTime": -1,
                    "fileSize": 19016843.0,
                    "statusCode": 2
                }
            },
            "streamerName": "主播名字",
            "streamTitle": "这是这次直播的名字",
            "currentFile": 0,
            "startTime": 1629235345,
            "endTime": -1,
            "statusCode": 3
        }
    ]
}

// 信息处理一下 只给我当前在动的 查询接口轻量一点 要详细信息单独开接口
// 这个是用来展示当前列表的
export const fake_up_son = {
    "streamerName": "主播名字",
    "streamTitle": "这是这次直播的名字",
    "startTime": 1629235345,
    "endTime": -1,
    "currentFile": 0,       // 只返回 正在处理的 文件type
    "filesList": [0, 1, 2, 3], // 只返回要传什么文件的type
    "done": [],            // 只返回处理结束 且成功 的 文件的type
    // 下面两个数据 如果上传的文件是一个个传的话 看看下面两个配置
    "comments": "[1] ↑ 432.00MB/880.02MB 916.94KB/s in 9s ", // 当前正在执行任务的消息
    "progress": 50,  // 正在执行任务的进度
    "task": { // 如果你这里可以传多个平台 且不是同时传 只放正在进行的
            "table":"BaiduPan", // 平台的类就不用做键了 直接给个值
            "taskType": 2,     // 当然 上传平台的类型给 数字最好了
            "startTime": 1629235345,
            "endTime": -1,
            "retries": 1,
            "statusCode": 3,
        },
    "taskInof": {
        "fileName": "1.flv",
        "localPath": "E://",
        "remotePath": "UplaodTest_SteamerName_UplaodTest_RoomChannel/19700101_UplaodTest_StreamTitle/",
        "fileType": 0,
        "currentTask": 0,
        "startTime": 1629235345, // 当前执行任务的开始时间
        "endTime": -1,      // 当前执行任务的结束时间
        "fileSize": 922768190.0, // 当前执行任务的文件大小
        "statusCode": 3
    }
}

export const fake_up_data_full = {
    "code": 1001,
    "message": "请求成功",
    "queue": 1, 
    "Package": [fake_up_son] // 如果上传一次只能处理一个对象 就直接给一个 长度为1 的数组
                 // 里面就是上面说的
                  // （这里讲的是一个上传任务的对象 包含了 flv 弹幕 礼物 mp4）
}
