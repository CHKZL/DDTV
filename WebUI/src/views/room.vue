<template>
  <div class="room">
    <div class = "tools">
      <div class="tools-icon">
        <i class="el-icon-s-tools" style="font-size:18px;color:#fff"></i>
      </div>
      <el-button icon="el-icon-search" circle></el-button>
      <el-button class="tools-item" icon="el-icon-plus"  circle @click="dialogFormVisible = true"></el-button>
      <el-switch class="tools-item" active-color="#13ce66" inactive-color="#ff4949" ></el-switch>
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

    <div class="roomsbox">
        <div  class="roomcard" v-for="item in room_list" :key="item.唯一码" :class="item.平台">
          <el-badge  class="livebadge" value="直播中" :hidden="!item.直播状态"></el-badge>
          <div class="room-card-config">
            <el-popover  width="150" trigger="click">
              <div style="text-align: right; margin: 0">
                <el-button type="danger" size="mini" @click="process_room_delete(item.唯一码)">删除房间</el-button>
              </div>
              <el-button type="text"  slot="reference" style="color:#000">
                <i class="el-icon-setting" style="font-size:18px"></i>
              </el-button>
            </el-popover>
          </div>
          <div class="room-card-item">
            <div class="username" >{{ item.名称 }} </div>
            <div class="originname">{{ item.原名 }} </div>
            <div>
                <i class="el-icon-video-camera" v-if="item.是否录制"></i>
                <i class="el-icon-video-camera" v-else style="color: #d6c3c3"></i>
                <i class="el-icon-loading" v-if="item.是否录制 && item.直播状态"></i>
            </div>
          </div>
          <el-switch class="live-switch" v-model="item.是否录制" active-color="#13ce66" inactive-color="#ff4949" @change="process_room_status(item.唯一码, item.是否录制)"></el-switch>
          <div class="roomid originname">ID:{{ item.唯一码 }}</div>

        </div>
    </div>
  </div>
</template>

<script>
import {postFormAPI,pubBody} from '../api'
export default {
  data() {
    return {
      loading: true,
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
  mounted: async function () {
    // 发一次请求 确定用户凭据的有效性
    await this.getList();
    this.loading = false;
    
    // 进行轮询，定时间隔10秒一次
    this.timer = window.setInterval(() => {
      setTimeout(() => {
        if (sessionStorage.getItem("token")) {
          // 轮询 的逻辑
          this.getList();
        }
      }, 0);
    }, 10000);
  },
  methods: {
    resetForm(formName) {
      this.$refs[formName].resetFields();
    },
    // 自动录制按钮 弹窗
    process_room_status: async function (roomid, status) {
      let that = this;
      await this.$confirm(
        "此操作将设更改房间设置, 是否继续?",
        status ? "关闭自动录制" : "开启自动录制",
        {
          confirmButtonText: "确定",
          cancelButtonText: "取消",
          type: "warning",
        }
      )
        .then(() => {
          console.log(status);
          that.room_status(roomid, !status);
          this.$message({
            type: "success",
            message: "操作成功!",
          });
        })
        .catch(() => {
          this.$message({
            type: "info",
            message: "已取消操作",
          });
        });
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
      return response.data;

    },

    // 删除房间弹窗
    process_room_delete: async function (roomid) {
      let that = this;
      await this.$confirm("此操作将从配置中删除, 是否继续?", "删除房间", {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning",
      })
        .then(() => {
          that.room_delete(roomid);
          this.$message({
            type: "success",
            message: "删除成功!",
          });
        })
        .catch(() => {
          this.$message({
            type: "info",
            message: "已取消删除",
          });
        });
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
    // 取消添加房间对话框
    closed_Addroom() {
      this.resetForm("AddRoom");
      this.dialogFormVisible = false;
    },
    // 进行 添加房间的操作
    to_AddRoom: async function () {
      let that = this;
      this.$refs["AddRoom"].validate((valid) => {
        if (valid) {
          that.proess_room_add();
        } else {
          console.log("error submit!!");
          return false;
        }
      });
    },
    // 处理添加房间
    proess_room_add: async function () {
      if (this.AddRoom.name == null) {
        this.AddRoom.name = this.AddRoom.orgin;
      }
      var res = await this.room_add(
        this.AddRoom.roomid,
        this.AddRoom.orgin,
        this.AddRoom.name,
        this.AddRoom.autoLive
      );
      if (res.code != 1001) {
        console.error(res.message);
        this.$alert(res.message, "错误", { confirmButtonText: "确定" });
      } else {
        this.resetForm("AddRoom");
        this.dialogFormVisible = false;
      }
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


  /**
    * 添加房间
    * 
    * @param {roomid} 房间号
    * @param {username} 用户可以自定义的用户名
    * @param {originname} 原名
    * @param {rec} 自动录制
    * @return 接口返回的数据
    */

    get_roomsetting: async function () {
      let param = pubBody('room_list')
      console.debug(param)
      let response = await postFormAPI('room_list',param,true)
      console.debug(response)
      this.room_list = response.data.Package
      return response.data;

    },
    // 用来发轮询请求的
    getList: async function () {
      this.get_roomsetting();
      }
    },
  // 销毁定时器
  destroyed() {
    window.clearInterval(this.timer);
  },
};
</script>

<style scoped>
.tools {
  position: fixed;
  background-color: #ff6a6a;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  border-radius: 5px;
  padding: 10px 10px 10px 0px;
  border: 1px solid #e2d0d0;
  display: flex;
  z-index: 1;
  max-width: 20px;
  max-height: 8px;
  transition-property: padding,max-width,max-height,border-radius,display,background-color;
  transition-duration: 0.5s;
  transition-timing-function:ease-out;
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
  grid-template-columns: 10px 50px 140px 50px 20px 10px;
  grid-template-rows: 10px 20px 95px  20px 10px;
  grid-template-areas:  '. . . . . .'
                        '. live . . config .'
                        '. item item item item .'
                        '. switch . rid rid .'
                        '. . . . . .';
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

