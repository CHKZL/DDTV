<template>
  <el-table :data="rec_all" style="width: 100%" height="100%">
    <el-table-column prop="GUID" label="GUID" width="300"> </el-table-column>
    <el-table-column prop="RoomId" sortable label="房间号"> </el-table-column>
    <el-table-column prop="Name" sortable label="昵称"> </el-table-column>
    <el-table-column prop="Downloaded_bit" sortable label="已录制文件大小">
        <template slot-scope="scope">
            {{scope.row.Downloaded_str}}
        </template>
    </el-table-column>
    <el-table-column sortable prop="StartTime" label="开始时间">
      <template slot-scope="scope">
        {{ toDate(scope.row.StartTime) }}
      </template>
    </el-table-column>
    <el-table-column sortable prop="EndTime"  label="结束时间">
      <template slot-scope="scope">
        {{scope.row.EndTime == 0 ? "任务未结束":toDate(scope.row.EndTime) }}
      </template>
    </el-table-column>
    <el-table-column prop="Remark" label="备注"> </el-table-column>
  </el-table>
</template>
<script>
import {postFormAPI,pubBody} from '../api'
import {formatDate} from '../reunit'
export default {
  data() {
    return {
      rec_all:[]
    };
  },
  created:async function() {
    console.log("初始化数据")
    this.rec_all_list()
  },
  mounted() {
    console.log("挂载页面")
    this.timer = window.setInterval(() => {
      setTimeout(() => {
        if (sessionStorage.getItem("token")) {
          this.rec_all_list()
        }
      }, 0);
    }, 10000);
  },
  methods: {
    toDate(v){
      return formatDate(v)
    },
      /**
    * 本函数封装了请求后端接口获取 所有录制队列
    * 
    * @return 接口返回的数据
    */
    rec_all_list: async function () {
      let param = pubBody('rec_all_list')
      console.debug(param)
      let response = await postFormAPI('rec_all_list',param,true)
      console.debug(response)
      let resarr =  response.data.Package
      this.rec_all = resarr

      return response.data;
    },
  },
  destroyed() {
    window.clearInterval(this.timer);
  },
};
</script>

<style scoped>
</style>
