<template>
  <div class="room">
    <div class = "tools">
      <div class="tools-icon">
        <i class="el-icon-s-tools" style="font-size:18px;color:#fff"></i>
      </div>
      <el-button icon="el-icon-search" circle></el-button>
      <el-button class="tools-item" icon="el-icon-plus"  circle @click="dialogFormVisible = true"></el-button>
      <el-switch class="tools-item" v-model="stopall" active-color="#46d485" inactive-color="#efe3e3"></el-switch>
    </div>

    <el-dialog title="添加房间" :visible.sync="dialogFormVisible">
        <el-form label-position="right" :model="AddRoom" :rules="addroomrules" ref="AddRoom">
          <el-form-item label="房间号" prop="roomid">
            <el-input v-model="AddRoom.roomid" autocomplete="off"></el-input>
          </el-form-item>

          <el-form-item label="名称" prop="orgin">
            <el-input v-model="AddRoom.orgin" autocomplete="off"></el-input>
          </el-form-item>

          <el-form-item label="自定义名称" prop="name">
            <el-input v-model="AddRoom.name" :placeholder="AddRoom.orgin" autocomplete="off"></el-input>
          </el-form-item>

          <el-form-item label="自动录制" prop="autoLive">
            <el-switch v-model="AddRoom.autoLive" active-color="#13ce66" inactive-color="#ff4949"></el-switch>
          </el-form-item>
        </el-form>
        <div slot="footer" class="dialog-footer">
          <el-button @click="closed_Addroom">取 消</el-button>
          <el-button type="primary" @click="to_AddRoom">确 定</el-button>
        </div>
      </el-dialog>

    <div class="roomsbox" v-if="room_list.length !=0">
      <transition name="slide" v-for="(item,index) in room_list" :key="item.roomid">
        <div v-loading="room_list[index].loading" v-show="room_list[index].show" class="float-up roomcard" :class="item.table" >
          <el-badge  class="livebadge" value="直播中" :hidden="!item.islive"></el-badge>
          <div class="room-card-config">
            <el-popover trigger="hover">
              <div style="text-align: right; margin: 0">
                <el-button  size="mini" @click="displaybox(index)" >锁定配置</el-button>
                <el-button type="danger" size="mini" @click="process_room_delete(index)">删除房间</el-button>
              </div>
              <i class="el-icon-setting config-ico" slot="reference" style="font-size:20px"></i>
            </el-popover>
          </div>
          <div class="room-card-item">
            <div class="username" >{{ item.name }} </div>
            <div class="originname">{{ item.orname }} </div>
            <div>
                <i class="el-icon-video-camera" v-if="item.rec"></i>
                <i class="el-icon-video-camera" v-else style="color: #d6c3c3"></i>
                <i class="el-icon-loading" v-if="item.rec && item.islive"></i>
            </div>
          </div>
          <el-switch class="live-switch" v-model="item.rec" active-color="#46d485" inactive-color="#efe3e3" @change="process_room_status(index)"></el-switch>
          <div class="set">
            <i @click="room_like_pross(index)" class="float-up" :class="room_list[index].like ? 'el-icon-star-on like-on':'el-icon-star-off like-off'"></i>
            <i class="float-up el-icon-folder"></i>
          </div>
          <div class="roomid originname">ID:{{ item.roomid }}</div>
        </div>
      </transition>
    </div>
    <el-empty v-else description="啥都木有"></el-empty>
  </div>
</template>

<script>
import {postFormAPI,pubBody} from '../api'
import {room_data} from '../utils/data_cli'
export default {
  data() {
    return {
      stopall:false,
      AddRoom: { roomid: null, name:null, orgin:null, autoLive: true },
      addroomrules: {
        roomid: [
          { required: true, message: "房间号不能为空", trigger: "blur" },
          { pattern: /^[0-9]+$/, message: "房间号只能为数字" },
        ],
        orgin: [
          { required: true, message: "名称不能为空", trigger: "blur" },
          {pattern:/^[^\s]*$/,message: "名称不能含有空格" }
        ],
        name: [{pattern:/^[^\s]*$/,message: "名称不能含有空格" }],
      },
      dialogFormVisible: false,
      room_list: [],
    };
  },
  // watch: {
  //   room_list(newValue, oldValue){
  //     for (let i = 0; i < newValue.length; i++) {
  //       if (oldValue[i] != newValue[i]) {
  //         console.log(newValue)
  //       }
  //     }
  //     console.log("room 数据发送变化")
  //     console.log(newValue,oldValue)
  //   },
  //   deep: true
  // },
  created:async function() {
    console.log("初始化数据")
    await postFormAPI('room_list',pubBody('room_list'),true).then(result => {
      room_data(this,result.data.Package)
    })
  },
  mounted() {

    console.log("挂载页面")
    this.timer = window.setInterval(() => {
      setTimeout(() => {
        if (sessionStorage.getItem("token")) {
          this.getList();
        }
      }, 0);
    }, 60000);
  },
  methods: {
    resetForm(formName) {
      this.$refs[formName].resetFields();
    },
    displaybox(i){
      this.room_list[i].show = !this.room_list[i].show
      console.debug(this.room_list[i].show)
    },
  /**
    * 自动录制状态管理
    * 
    * @param {roomid} 房间号
    * @param {status} 状态
    * @return 接口返回的数据
    */
    room_status: async function (roomid, status) {
      let param = pubBody('room_status')
      param.roomId = roomid
      param.recStatus = status

      console.debug(param)
      let response = await postFormAPI('room_status',param,true)
      console.debug(response)
      return response.data

    },
    msggo:function (res){
      console.log(res)
      let typ = "success",msg = "操作成功"
      if (res.code != 1001) {typ = "error",msg = res.message}
      this.$message({message: msg,type: typ});
    },
    // 自动录制按钮 弹窗
    process_room_status: async function (index) {
      this.room_list[index].loading = true
      let roomid = this.room_list[index].roomid
      let status = this.room_list[index].rec
      await this.$confirm(
        "此操作将设更改房间设置, 是否继续?",
        !status ? "关闭自动录制" : "开启自动录制",
        {
          confirmButtonText: "确定",
          cancelButtonText: "取消",
          type: "warning"
        }).then(() => {
          console.debug("确定")
          this.room_status(roomid,status).then(result => {
            this.msggo(result)
            if(result.code != 1001) this.room_list[index].rec = !status
          })
        })
        .catch(() => {
          console.debug("取消")
          this.room_list[index].rec = !status
        });
        this.room_list[index].loading=false
        
    },

    // 删除房间弹窗
    process_room_delete: async function (index) {
      this.room_list[index].loading = true
      let roomid = this.room_list[index].roomid

      await this.$confirm("此操作将从配置中删除, 是否继续?", "删除房间", {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning",
      })
        .then(() => {
          this.room_delete(roomid).then(result => {
            this.msggo(result)
            if(result.code != 1001) this.room_list[index].rec = !status;
            if(result.code == 1001) this.room_list.splice(index,1);
          })
        })
        .catch(() => {
          console.log("取消")
        });
        this.room_list[index].loading=false
    },

    /**
    * like 房间
    * 
    * @param {roomid} 房间号
    * @param {like} like接口的状态
    * @return 接口返回的数据
    */
    room_like:async function(roomid,like){
      let param = pubBody('room_like')
      param.roomId = roomid
      param.likeStatus = like
      let response = await postFormAPI('room_like',param,true)
      return response.data
    },

    room_like_pross : function(index){
      let like = this.room_list[index].like,
          roomid = this.room_list[index].roomid
      this.room_like(roomid,!like).then(result =>{
        this.msggo(result)
        if(result.code == 1001) this.room_list[index].like = !like
      })

    },

    /**
    * 删除房间
    * 
    * @param {roomid} 房间号
    * @return 接口返回的数据
    */
    room_delete: async function (roomid) {
      let param = pubBody('room_delete')
      param.roomId = roomid
      console.debug(param)
      let response = await postFormAPI('room_delete',param,true)
      console.debug(response)
      return response.data;

    },

  /**
    * 添加房间
    * 
    * @param {roomid} 房间号
    * @param {username} 用户可以自定义的用户名
    * @param {originname} 原名
    * @param {rec} 自动录制
    * @return 接口返回的数据
    */
    room_add: async function (roomid, username, originname, rec) {
      let param = pubBody('room_add')
      param.roomId = roomid
      param.name = username
      param.officialName = originname
      param.recStatus = rec
      console.debug(param)
      let response = await postFormAPI('room_add',param,true)
      console.debug(response)
      return response.data;

    },

    // 取消添加房间对话框
    closed_Addroom() {
      this.resetForm("AddRoom");
      this.dialogFormVisible = false;
    },
    // 进行 添加房间的操作
    to_AddRoom: async function () {
      this.$refs["AddRoom"].validate((valid) => {
        if (valid) {
          if (this.AddRoom.name == null) {
            this.AddRoom.name = this.AddRoom.orgin;
          }
          this.room_add(this.AddRoom.roomid,this.AddRoom.orgin,this.AddRoom.name,this.AddRoom.autoLive).then(result => {
            this.msggo(result)
            if(result.code == 1001){
              let pushdata = { "islive":false,
                                "table":"bilibili",
                                "name":this.AddRoom.name,
                                "orname":this.AddRoom.orgin,
                                "rec":this.AddRoom.autoLive,
                                "roomid":this.AddRoom.roomid,
                                "show":true,
                                "loading":false
                            }
              console.debug(pushdata)
              this.room_list.push(pushdata);
              this.dialogFormVisible = false
            }
          })
        } else {
          console.debug("error submit!!");
          return false;
        }
      });
    },
      /**
    * 房间列表
    * 
    * @return 接口返回的数据
    */

  get_roomsetting: async function () {
    let param = pubBody('room_list')
    console.debug(param)
    let response = await postFormAPI('room_list',param,true)
    console.debug(response)
    room_data(this,response.data.Package)
    return response.data;
  },
    // 用来发轮询请求的
  getList: async function () {
      this.get_roomsetting();
    }

  },
  destroyed() {
    window.clearInterval(this.timer);
  },
};
</script>

<style scoped>
.slide-enter-active{
 transition:all .5s linear;
}
.slide-leave-active{
  transition:all .1s linear;
}
.slide-enter{
  transform: translateX(-100%);
  opacity: 0;
}
.slide-leave-to{
  transform: translateX(110%);
  opacity: 0;
}
.tools {
  position: fixed;
  background-color: #ff6a6a;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  border-radius: 5px;
  padding: 10px 10px 10px 0px;
  border: 1px solid #e2d0d0;
  display: flex;
  z-index: 2004;
  max-width: 20px;
  max-height: 8px;
  transition-duration: 0.8s;
  transition-timing-function:cubic-bezier(0.39, 0.575, 0.565, 1);
  overflow: hidden;
  flex-direction: row;
  justify-content: flex-start;
  align-items: center;
  align-content: center;
}
.tools:hover{
  padding: 10px 10px 10px 10px;
  max-width: 300px;
  max-height: 300px;
  border-radius: 18px;
  background-color: #fff;
}
.tools:hover .tools-icon{
  display: none;
}
.tools-icon {
  width: 30px;
  flex-shrink: 0;
  height: 30px;
  display: flex;
  align-items: center;
  justify-content: center;
}
.tools-item{
  margin-left: 15px;
}
.roomsbox {
  display: grid;
  justify-content: space-evenly;
  grid-template-columns: repeat(auto-fill, 280px);
  grid-gap: 20px;
}
.roomcard {
  height: 155px;
  width: 280px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  display: grid;
  grid-template-columns: 10px 50px 45px 95px 50px 20px 10px;
  grid-template-rows: 10px 20px 95px  20px 10px;
  grid-template-areas:  '. .      .    .    .    .         .'
                        '. live   .    .    .    config    .'
                        '. item   item item item item      .'
                        '. switch set  .    rid  rid       .'
                        '. .      .    .    .    .         .';
}
.set {
  grid-area: set;
  display: flex;
  font-size: 20px;
  flex-direction: row;
  justify-content: space-between; 
}
.like-on{
  color: #ffd700;
  text-shadow: 0 0px 6px #ffd700c4;
  transition:color 0.2s;
  transition-timing-function: cubic-bezier(0.075, 0.82, 0.165, 1);
}
.like-on:hover{
  color: #dbbe16;
}
.like-off{
  color: #383838;
  transition:color 0.2s;
  transition-timing-function: cubic-bezier(0.075, 0.82, 0.165, 1);
}
.like-off:hover{
  color: #6e6b6b;
}
.float-up{
  position: relative;
  transition:top 0.3s;
  top:0px
}
.float-up:hover{
  top:-4px;
}
.bilibili{
  background-image:  url("../../public/static/bilibili.png");
}
.roomid{
  grid-area: rid;
}
.room-card-item {
  grid-area: item;
  padding-left: 10px;
}

.livebadge {
  grid-area: live;
}
.live-switch {
  grid-area: switch;
}
.room-card-config {
  grid-area: config;
  display: flex;
  flex-direction: column;
  justify-content: center;
}
.config-ico{
  color: rgba(0, 0, 0, 0.342);
  transition: color 0.2s;
}
.config-ico:hover{
  color: #000;
  
}
.username {
  font-size: 28px;
  font-weight: 300;
  max-width: 251px;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}
.originname {
  font-size: 10px;
  color: #af8585;
}
</style>

