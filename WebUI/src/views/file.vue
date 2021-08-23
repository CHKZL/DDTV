<template>
  <div class="file">
    <div class="tool-bar">
      <div class="but-bar">
        <el-button-group style="width: 100%;">
          <el-button size="mini" disabled icon="el-icon-download">下载</el-button>
          <el-button size="mini" disabled type="danger" icon="el-icon-delete">删除</el-button>
        </el-button-group>
        <!-- <el-input size="mini"  placeholder="搜索您的文件" v-model="input1">
          <template slot="prepend">
            <i class="el-icon-search"></i>
          </template>
        </el-input> -->
      </div>
      <div class="path">
        <div class="pathinfo">
          <el-button type="text" style="padding: 0 10px 0 0;" @click="filesShow = files" :disabled="filesShow.name == '/'">返回上一层</el-button> 
          <div class="originname" style="font-size: 14px;">tmp {{filesShow.name != "/" ? `> ${filesShow.name}`:""}}</div>
        </div>
        <div style="color:#969a94;font-size: 14px;">已全部加载，共{{Object.keys(filesShow.Obj).length}}个文件/目录</div>
      </div>
      <div class="path" style="justify-content: flex-start;">
        <el-checkbox style="padding: 0 10px 0 0;" disabled></el-checkbox>
        <div style="color:#969a94;font-size: 14px;">开发中</div>
      </div>
    </div>
    <div class="file_list">
      <div v-for="(item,key) in filesShow.Obj" :key="key" class="file_list_item">
        <div style="display:flex;flex-direction: row;align-items: center;">
          <el-checkbox  :disabled="item.type == 0 ? true:false"></el-checkbox>
          <img v-if="item.type == 0" src="../../public/static/dict.png" class="fileIcon" />
          <img v-else-if="item.type == 1" src="../../public/static/mp4.png" class="fileIcon" />
          <img v-else src="../../public/static/flv.png" class="fileIcon" />
          
          <el-button v-if="item.type == 0" type="text" style="padding-left: 20px;" @click="filesShow = item" >{{key}}</el-button> 
          <div v-else style="padding-left: 20px;font-size: 14px;">{{key}}</div>
        </div>
        <div>
          <div class="originname" style="padding-left: 20px;float: left;">{{item.type != 0 ? change(item.Obj.Size):"--"}}</div>
          <div class="originname" style="padding-left: 20px;float: right;">{{item.type != 0 ? transformTimestamp(item.Obj.ModifiedTime):''}}</div>
        </div>
        <div style="justify-self: center;">
          <el-button-group style="width: 100%;" v-show="item.type != 0">
            <el-button size="mini" icon="el-icon-video-play">播放</el-button>
            <el-button size="mini" icon="el-icon-download">下载</el-button>
            <el-button size="mini" type="danger" icon="el-icon-delete" @click="process_file_delete(item.Obj.Directory, key)">删除</el-button>
          </el-button-group>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { postFormAPI, pubBody } from "../api";
export default {
  data() {
    return {
      files:{},
      filesShow:{},
      userlook:null
    };
  },

  created: async function(){

    let data = await this.file_lists(),
        filesdict = this.render_manage(data),
        roomid = this.$route.query.rooid
    if(roomid) {
      this.file_range(roomid).then(result => {
        this.files = filesdict
        this.filesShow = this.render_manage(result)
      })
    }else{
    this.files = filesdict
    this.filesShow = filesdict}
  },

  mounted: async function () {
    // 发一次请求 确定用户凭据的有效性
    this.getList();
    // 进行轮询，定时间隔10秒一次
    this.timer = window.setInterval(() => {
      setTimeout(() => {
        if (sessionStorage.getItem("token")) {
          // 轮询 的逻辑
          this.getList();
        }
      }, 0);
    }, 30000);
  },
  methods: {
    // 字节计算
    change: function (limit) {
      var size = "";
      if (limit < 0.1 * 1024) {
        //小于0.1KB，则转化成B
        size = limit.toFixed(2) + "B";
      } else if (limit < 0.1 * 1024 * 1024) {
        //小于0.1MB，则转化成KB
        size = (limit / 1024).toFixed(2) + "KB";
      } else if (limit < 0.1 * 1024 * 1024 * 1024) {
        //小于0.1GB，则转化成MB
        size = (limit / (1024 * 1024)).toFixed(2) + "MB";
      } else {
        //其他转化成GB
        size = (limit / (1024 * 1024 * 1024)).toFixed(2) + "GB";
      }

      var sizeStr = size + ""; //转成字符串
      var index = sizeStr.indexOf("."); //获取小数点处的索引
      var dou = sizeStr.substr(index + 1, 2); //获取小数点后两位的值
      if (dou == "00") {
        //判断后两位是否为00，如果是则删除00
        return sizeStr.substring(0, index) + sizeStr.substr(index + 3, 2);
      }
      return size;
    },
    // 时间戳字符串
    transformTimestamp(timestamp) {
      let a = new Date(timestamp).getTime();
      const date = new Date(a);
      const Y = date.getFullYear() + "-";
      const M =
        (date.getMonth() + 1 < 10
          ? "0" + (date.getMonth() + 1)
          : date.getMonth() + 1) + "-";
      const D =
        (date.getDate() < 10 ? "0" + date.getDate() : date.getDate()) + "  ";
      const h =
        (date.getHours() < 10 ? "0" + date.getHours() : date.getHours()) + ":";
      const m =
        date.getMinutes() < 10 ? "0" + date.getMinutes() : date.getMinutes();
      // const s = date.getSeconds(); // 秒
      const dateString = Y + M + D + h + m;
      // console.log('dateString', dateString); // > dateString 2021-07-06 14:23
      return dateString;
    },

    /**
     * 本函数封装了获取文件列表的功能
     */
    file_lists:async function(){
      let param = pubBody('file_lists'),
          response = await postFormAPI('file_lists',param,true)
      return response
    },

    // 渲染管理

    render_manage:function(data){
      let res = data.data.Package,
          reslen = res.length,
          filesdict = {"type":0,"name":'/',"Obj":{}},
          pathdit = {}

      for (var i = 0; i < reslen; i++) { 
        let dictName = res[i].Directory
        if(pathdit[dictName]) pathdit[dictName].push(res[i])
        else pathdit[dictName] = [res[i]]
      }
      for(var key in pathdit){
        let fileitem = {},
            itemlen = pathdit[key].length
        for (var j = 0; j < itemlen; j++) {
          let fileInfo = pathdit[key][j],
              fileName = fileInfo.Name
          fileitem[fileName] = {"type":1,"name":key,"Obj":fileInfo}
        }
        filesdict.Obj[key] = {"type":0,"name":key,"Obj":fileitem}
      }
      console.log(filesdict)
      return filesdict
    },


    // 去视频播放页

    getDescribe(path1, name, flv) {
      let rpath = "";
      if (flv) {
        rpath = "/playflv";
      } else {
        rpath = "/play";
      }
      let newpage = this.$router.resolve({
        path: rpath,
        query: {
          path: path1,
          name: name,
        },
      });
      window.open(newpage.href, "_blank");
    },
    msggo:function (res){
      console.log(res)
      let typ = "success",msg = "操作成功"
      if (res.code != 1001) {typ = "error",msg = res.message}
      this.$message({message: msg,type: typ});
    },

    /**
     * 删除文件
     *
     * @param {Directory} 文件路径
     * @param {Name} 文件名
     * @return 接口返回的数据
     */
    file_delete: async function (Directory, Name) {
      let param = pubBody("file_delete");
      param.directory = Directory;
      param.name = Name;
      console.debug(param);
      let response = await postFormAPI("file_delete", param, true);
      console.debug(response);
      return response.data;
    },
    process_file_delete: async function (Directory, Name) {
      await this.$confirm("此操作不可恢复, 是否继续?", "删除文件", {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning",
      })
        .then(() => {
          this.file_delete(Directory, Name).then(result => {
            this.msggo(result)
            // 更新视图
            this.file_lists().then(result => {
            let userlook = this.filesShow.name,
                countlist = this.render_manage(result)

            this.files = countlist
            if(userlook!='/') this.filesShow = countlist.Obj[userlook]
            else this.filesShow = countlist
            console.log(userlook)
            console.log(countlist)
            })
          })
        })
        .catch(() => {
          this.$message({
            type: "info",
            message: "已取消删除",
          });
        });
    },

    // 根据房间获取文件列表 弹窗
    useroom_file: async function (roomid) {
      this.roomid = roomid;
      let res = await this.file_range(roomid);
      // this.file_tab = res.Package;
      // var len = this.file_tab.length;
      // for (var i = 0; i < len; i++) {
      //   let type = this.file_tab[i].Name.slice(-3);
      //   if (type == "flv") type = true;
      //   else type = false;
      //   this.file_tab[i].Type = type;
      // }
      return res
    },

    getList: async function () {
      // 获取全部房间
      this.room_list();
    },

    // 根据房间号 获取文件
    /**
     * 删除文件
     *
     * @param {roomid} 房间号
     * @return 接口返回的数据
     */
    file_range: async function (roomid) {
      let param = pubBody("file_range");
      param.roomId = roomid;
      console.debug(param);
      let response = await postFormAPI("file_range", param, true);
      console.debug(response);
      return response;
    },

    // 获取当前所有房间
    room_list: async function () {
      let param = pubBody("room_list");
      console.debug(param);
      let response = await postFormAPI("room_list", param, true);
      console.debug(response);
      this.room = response.data.Package;
      return response.data;
    },
  },
  // 销毁定时器
  destroyed() {
    window.clearInterval(this.timer);
  },
};
</script>

<style scoped>
.file {
  /*概览的主要元素的容器 全局布局的对象 */
  display: flex;
  flex-direction: column;
  position: relative;
}
.fileIcon{height: 38px;padding-left: 30px;}
.tool-bar{
  display: flex;
  flex-direction: column;
  justify-content: space-around;
  border-bottom: 1px solid #e6e4e4;
  height: 94px;
  background-color: #ffffff;
  padding: 10px 10px 10px 10px;
  position: -webkit-sticky; /* Safari */
  position: sticky;
  top: 0;
  z-index: 99;
}
.but-bar {
  display: flex;
  flex-direction: row;
  flex-wrap: nowrap;
  align-items: center;
  justify-content: space-between;
}
.path{
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.pathinfo{
  display: flex;
  align-items: center;
}
.file_list_item{
  /* background-color: rgb(255 255 255); */
  height: 55px;
  border-bottom: 1px solid rgb(143 239 255);
  padding-left: 10px;
  padding-right: 10px;
  display: grid;
  grid-template-columns: 4fr 1fr 2fr;
  align-items: center;
  background-color: #f0e7e700;
}
.file_list_item:hover{
  background-color: #f0e7e785;
}
.username {
  font-size: 28px;
  font-weight: 300;
  display: flex;
  align-items: center;
  white-space: nowrap;
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
}
.originname {
  font-size: 10px;
  color: #af8585;
}
</style>

