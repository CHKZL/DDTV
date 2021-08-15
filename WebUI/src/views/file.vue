<template>
  <div class="file">
    <el-row :gutter="20">
      <el-col :xs="24" :sm="12" :md="8" :lg="6" v-for="item in room" :key="item.唯一码" style="padding-top: 20px">
        <transition name="el-zoom-in-center">
          <el-card style="height: 155px">
            <div>
              <div class="username">
                {{ item.名称 }}
                <el-tag size="mini">{{ item.平台 }}</el-tag>
              </div>
              <div class="originname">{{ item.原名 }} ID:{{ item.唯一码 }}</div>
              <el-button style="margin-top: 20px" size="mini" @click="useroom_file(item.唯一码)">文件管理</el-button>
            </div>
          </el-card>
        </transition>
      </el-col>
    </el-row>

    <el-dialog title="文件管理" :visible.sync="dialogshow">
        <el-table stripe max-height="700" :data="file_tab">
            <el-table-column sortable property="ModifiedTime"  label="生成日期">
              <template slot-scope="scope">
                {{transformTimestamp(scope.row.ModifiedTime)}}
              </template>
            </el-table-column>
            <el-table-column property="Name" label="文件名"></el-table-column>
            <el-table-column sortable property="Size" label="大小">
              <template slot-scope="scope">
                {{change(scope.row.Size)}}
              </template>
            </el-table-column>
            <el-table-column sortable property="Type" label="类型">
              <template slot-scope="scope">
                {{scope.row.Type ? "flv":"mp4"}}
              </template>
            </el-table-column>
            <el-table-column label="操作">
                <template slot-scope="scope">
                    <el-button size="mini" type="danger" @click="process_file_delete(scope.row.Directory, scope.row.Name)">删除</el-button>
                    <el-button size="mini" :disabled="scope.row.Type" @click="getDescribe(scope.row.Directory,scope.row.Name,scope.row.Type)">播放</el-button>
                </template>
            </el-table-column>
        </el-table>
    </el-dialog>
  </div>

  
</template>

<script>
import {postFormAPI,pubBody} from '../api'
export default {
  data() {
    return {
        roomid:"",
        room: [],
        dialogshow:false,
        file_tab:[]
    };
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
    change:function (limit){
        var size = "";
        if(limit < 0.1 * 1024){                            //小于0.1KB，则转化成B
            size = limit.toFixed(2) + "B"
        }else if(limit < 0.1 * 1024 * 1024){            //小于0.1MB，则转化成KB
            size = (limit/1024).toFixed(2) + "KB"
        }else if(limit < 0.1 * 1024 * 1024 * 1024){        //小于0.1GB，则转化成MB
            size = (limit/(1024 * 1024)).toFixed(2) + "MB"
        }else{                                            //其他转化成GB
            size = (limit/(1024 * 1024 * 1024)).toFixed(2) + "GB"
        }
    
        var sizeStr = size + "";                        //转成字符串
        var index = sizeStr.indexOf(".");                    //获取小数点处的索引
        var dou = sizeStr.substr(index + 1 ,2)            //获取小数点后两位的值
        if(dou == "00"){                                //判断后两位是否为00，如果是则删除00
            return sizeStr.substring(0, index) + sizeStr.substr(index + 3, 2)
        }
        return size;
    },
    // 时间戳字符串
    transformTimestamp(timestamp){
      let a = new Date(timestamp).getTime();
      const date = new Date(a);
      const Y = date.getFullYear() + '-';
      const M = (date.getMonth() + 1 < 10 ? '0' + (date.getMonth() + 1) : date.getMonth() + 1) + '-';
      const D = (date.getDate() < 10 ? '0'+date.getDate() : date.getDate()) + '  ';
      const h = (date.getHours() < 10 ? '0'+date.getHours() : date.getHours()) + ':';
      const m = (date.getMinutes() <10 ? '0'+date.getMinutes() : date.getMinutes()) ;
      // const s = date.getSeconds(); // 秒
      const dateString = Y + M + D + h + m;
      // console.log('dateString', dateString); // > dateString 2021-07-06 14:23
      return dateString;
    },
    // 去视频播放页
    getDescribe(path1,name,flv) {
      let rpath = ""
      if(flv){
        rpath = "/playflv"
      }else{
        rpath = "/play"
      }
      let newpage = this.$router.resolve({
          path: rpath,
          query: {
            path: path1,
            name:name
          }
        })
      window.open(newpage.href,'_blank')
    },

    /**
    * 删除文件
    * 
    * @param {Directory} 文件路径
    * @param {Name} 文件名
    * @return 接口返回的数据
    */
    file_delete:async function(Directory,Name){      
      let param = pubBody('file_delete')
      param.directory = Directory
      param.name = Name
      console.debug(param)
      let response = await postFormAPI('file_delete',param,true)
      console.debug(response)
      return response.data;
    },
    process_file_delete: async function (Directory,Name) {
      let that = this;
      await this.$confirm("此操作不可恢复, 是否继续?", "删除文件", {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning",
      })
        .then(() => {
          that.file_delete(Directory,Name);
          this.$message({
            type: "success",
            message: "删除成功!",
          });
          that.useroom_file(that.roomid)
        })
        .catch(() => {
          this.$message({
            type: "info",
            message: "已取消删除",
          });
        });
    
    },

    // 根据房间获取文件列表 弹窗
    useroom_file:async function(roomid){
        this.roomid = roomid
        let res = await this.file_range(roomid)
        this.file_tab = res.Package
        var len = this.file_tab.length
        for(var i = 0; i < len; i++) {
          let type = this.file_tab[i].Name.slice(-3)
          if(type=="flv") type = true
          else type = false
          this.file_tab[i].Type = type
        }
        this.dialogshow = true
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
      let param = pubBody('file_range')
      param.roomId = roomid
      console.debug(param)
      let response = await postFormAPI('file_range',param,true)
      console.debug(response)
      return response.data;
    },

    // 获取当前所有房间
    room_list: async function () {
      let param = pubBody('room_list')
      console.debug(param)
      let response = await postFormAPI('room_list',param,true)
      console.debug(response)
      this.room = response.data.Package
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
.file{
  /*概览的主要元素的容器 全局布局的对象 */
  display: flex;
  flex-direction:column;
  padding: 20px 20px 20px 20px;
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

