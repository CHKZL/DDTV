<template>
  <div class="home">
    <div class="sys">

      <div class="systemInfo">
          <div class="card-title">
            系统状态
            <i class="el-icon-tickets"></i>
          </div>

          <div class="bataGroup">
              <el-card shadow="hover">
                <div class="card-title-litter">监听房间数</div>
                <div class="big-number">
                  {{ system_info_data.Room_Quantity }}
                </div>
              </el-card>

              <el-card shadow="hover">
                <div class="card-title-litter">正在录制</div>
                <div class="big-number">
                  {{ system_info_data.download_Info.Downloading }}
                </div>
              </el-card>

              <el-card shadow="hover">
                <div class="card-title-litter">正在上传</div>
                <div class="big-number">{{uoload.length}}</div>
              </el-card>
          </div>

          <el-descriptions style="padding-top: 20px">
            <el-descriptions-item label="更新">
                <i
                  v-if="system_info_data.ver_Info.IsNewVer"
                  class="el-icon-warning"
                  style="color: #909399"
                ></i>
                <i v-else class="el-icon-success" style="color: #67c23a"></i>
                {{ system_info_data.ver_Info.IsNewVer ? "有新版本" : "最新" }}
            </el-descriptions-item>
            <el-descriptions-item label="DDTV版本">{{
              system_info_data.DDTVCore_Ver
            }}</el-descriptions-item>
            <el-descriptions-item label=".NET版本">{{
              system_info_data.os_Info.AppCore_Ver
            }}</el-descriptions-item>
            <el-descriptions-item label="WEB版本">{{
              system_info_data.os_Info.WebCore_Ver
            }}</el-descriptions-item>
              <el-descriptions-item label="系统">
                <el-tooltip class="item" effect="dark" :content="system_info_data.os_Info.OS_Ver" placement="top-start">
                  <div>
                {{
                  system_info_data.os_Info.OS_Ver.length > 15
                    ? system_info_data.os_Info.OS_Ver.slice(0, 15) + "..."
                    : system_info_data.os_Info.OS_Ver
                }}
                  </div>
                </el-tooltip>
              </el-descriptions-item>

            <el-descriptions-item label="平台架构">
              {{ system_info_data.os_Info.OS_Tpye }}
            </el-descriptions-item>
            <el-descriptions-item label="是否交互模式运行">
              {{ system_info_data.os_Info.UserInteractive }}
            </el-descriptions-item>
            <el-descriptions-item label="当前关联的用户名">
              {{ system_info_data.os_Info.Associated_Users }}
            </el-descriptions-item>
          </el-descriptions>
      </div>

      <div class="systemInfo">
          <div class="card-title">
            设备状态
            <i class="el-icon-warning-outline"></i>
          </div>
          <el-row :gutter="10">
            <el-col :span="8">
              <el-card shadow="hover">
                <div class="card-title-litter">CPU</div>
                <div class="big-number">{{system_monitor.CPU_usage}}%</div>
              </el-card>
            </el-col>
            <el-col :span="8">
              <el-card shadow="hover">
                <div class="card-title-litter">内存</div>
                <div class="big-number">
                  <span>{{((((system_monitor.Total_memory - system_monitor.Available_memory)/1024)/1024)/1024).toFixed(1)}}G</span>
                  <span>/{{(((system_monitor.Total_memory/1024)/1024)/1024).toFixed(0)}}G</span>
                </div>
              </el-card>
            </el-col>
            <el-col :span="8">
              <el-card shadow="hover">
                <div class="card-title-litter">磁盘</div>
                <div class="big-number">
                  <span>{{sys_dish.Usage}}</span>
                  <span>/{{sys_dish.Size}}</span>
                </div>
              </el-card>
            </el-col>
          </el-row>
          <el-descriptions style="padding-top: 20px">
            <el-descriptions-item label="服务器名称">{{system_info_data.ServerName}}</el-descriptions-item>
            <el-descriptions-item label="UUID">
              <el-tooltip class="item" effect="dark" :content="system_info_data.ServerAID" placement="top-start">
                  <div>
                {{
                  system_info_data.ServerAID.length > 15
                    ? system_info_data.ServerAID.slice(0, 15) + "..."
                    : system_info_data.ServerAID
                }}
                  </div>
                </el-tooltip>
              
            </el-descriptions-item>
            <el-descriptions-item label="分组">
              <el-tag size="small">{{system_info_data.ServerGroup}}</el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="CPU">{{system_monitor.CPU_usage}}%</el-descriptions-item>
            <el-descriptions-item label="内存">{{(((system_monitor.Total_memory/1024)/1024)/1024).toFixed(0)}}G</el-descriptions-item>
            <el-descriptions-item label="磁盘">{{sys_dish.Size}}</el-descriptions-item>
            <el-descriptions-item label="DDTV占用内存">{{((system_monitor.DDTV_use_memory/1024)/1024).toFixed(2)}}MB</el-descriptions-item>
          </el-descriptions>
      </div>

    </div>
    
    <div class="systemInfo">
        <div class="card-title">
          任务概览
          <i class="el-icon-tickets"></i>
        </div>
        <el-row :gutter="10">
          <el-col :span="8">
            <el-card shadow="hover">
              <div class="card-title-litter">正在进行</div>
              <div class="big-number">{{rec_tab.length}}</div>
            </el-card>
          </el-col>
          <el-col :span="8">
            <el-card shadow="hover">
              <div class="card-title-litter">下载量</div>
              <div class="big-number">{{dl_all > 1000000000 ? (dl_all/1000000000).toFixed(2) + "GB":(dl_all/1000000).toFixed(2)+ "MB" }}</div>
            </el-card>
          </el-col>
          <el-col :span="8">
            <el-card shadow="hover">
              <div class="card-title-litter">出错</div>
              <div class="big-number">0</div>
              <div></div>
            </el-card>
          </el-col>
        </el-row>

        <el-table :data="rec_tab" style="width: 100%">
          <el-table-column prop="GUID" label="GUID" width="300"> </el-table-column>
          <el-table-column prop="RoomId" label="房间号">
          </el-table-column>
          <el-table-column prop="Name" label="昵称">
          </el-table-column>
          <el-table-column prop="Downloaded_str" label="已录制文件大小">
          </el-table-column>
          <el-table-column label="开始时间">
            <template slot-scope="scope">
            {{toDate(scope.row.StartTime)}}
            </template>
          </el-table-column>
          <el-table-column prop="Remark" label="备注">
          </el-table-column>
          <el-table-column label="操作">
            <template slot-scope="scope">
              <el-button size="mini" type="danger" @click="press_stop_rec(scope.row.GUID)">停止录制</el-button>
            </template>
          </el-table-column>
        </el-table>
    </div>

  </div>
</template>

<script>
import {formatDate} from '../reunit'
import {postFormAPI,pubBody} from '../api'
export default {
  data() {
    return {
      system_info_data: [],
      tableData: [],
      rec_tab: [],
      rec_all:[],
      dl_all:0,
      system_monitor:[],
      sys_dish:{},
      uoload:[]
    };
  },
  created: async function(){
    await this.getList()
  },
  mounted: async function () {
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
    toDate(v){
      return formatDate(v)
    },
    // 停止本场录制 弹窗
    press_stop_rec: async function (id) {
      let that = this;
      await this.$confirm("此操作将会停止录制直到下次开播，确定继续?", "停止录制", {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning",
      })
        .then(() => {
          that.rec_cancel(id);
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
    * 根据GUID取消录制任务
    * 
    * @return 接口返回的数据
    */
    rec_cancel:async function(id){
      let param = pubBody('rec_cancel')
      param.gUID = id
      let response = await postFormAPI('rec_cancel',param,true)
      this.getList()
      return response;
    },
    // 用来发轮询请求的
    getList: async function () {
      // 获取系统信息
      this.system_info();
      // 获取队列简报（录制）
      this.rec_processing_list();
      // 获取所有的录制队列 （含有历史）
      this.rec_all_list()
      // 获取队列简报 （上传）
      this.upload_ing()
      // 获取系统监控
      this.system_resource_monitoring()
    },

    /**
    * 本函数封装了请求后端接口获取 获取上传中的队列简报 （全部）
    * 
    * @return 接口返回的数据
    */
    upload_ing: async function(){
      let param = pubBody('upload_ing')
      console.debug(param)
      let response = await postFormAPI('upload_ing',param,true)
      console.debug(response)
      this.uoload  =  response.data.Package
      return response.data;

    },

    /**
    * 本函数封装了请求后端接口获取 获取系统资源监控
    * 
    * @return 接口返回的数据
    */
    system_resource_monitoring:async function(){
      let param = pubBody('system_resource_monitoring')
      console.debug(param)
      let response = await postFormAPI('system_resource_monitoring',param,true)
      console.debug(response)

      this.system_monitor = response.data.Package[0]
      let dish = this.system_monitor.HDDInfo
      var dishlen = dish.length
      for(var j = 0; j < dishlen; j++) {
        if(dish[j].MountPath == "/"){
          this.sys_dish = dish[j]
        }
      }

    },

    /**
    * 本函数封装了请求后端接口获取 获取上传的队列简报 （全部）
    * 
    * @return 接口返回的数据
    */
    upload_list: async function(){
      let param = pubBody('upload_list')
      console.debug(param)
      let response = await postFormAPI('upload_list',param,true)
      console.debug(response)
      return response.data;

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

       // 计算下载量
      var len = resarr.length
      this.dl_all = 0
      for(var i = 0; i < len; i++) {
        this.dl_all += resarr[i].Downloaded_bit
      }
      return response.data;
    },

    /**
    * 本函数封装了请求后端接口获取 获取录制中的队列简报 (进行中的)
    * 
    * @return 接口返回的数据
    */
    rec_processing_list: async function () {
      let param = pubBody('rec_processing_list')
      console.debug(param)
      let response = await postFormAPI('rec_processing_list',param,true)
      console.debug(response)
      this.rec_tab = response.data.Package;
      return response.data;
    },


    /**
    * 本函数封装了请求后端接口获取 system_info 的方法
    * 
    * @return 接口返回的数据
    */
    system_info: async function () {
      let param = pubBody('system_info')
      console.debug(param)
      let response = await postFormAPI('system_info',param,true)
      console.debug(response)
      this.system_info_data = response.data.Package[0];
      console.debug(this.system_info_data)
      return response.data;
    },
  },
  // 销毁定时器
  destroyed() {
    window.clearInterval(this.timer);
  },
};
</script>

<style>
.home{
  /*概览的主要元素的容器 全局布局的对象 */
  display: grid;
  grid-gap: 20px;
  grid-template-columns: repeat(auto-fit, minmax(540px, 100%));
}
.sys {
  display: grid;
  grid-gap: 20px;
  grid-template-columns: repeat(auto-fit, minmax(540px, 1fr));
  grid-template-rows: repeat(1, 300px);
}
.systemInfo {
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  padding: 20px 20px 20px 20px;
  display: flex;
  flex-direction: column;
  align-items: stretch;
  align-content: center;
}
.bataGroup{
  display:grid;
  grid-gap: 10px;
  grid-template-columns: 1fr 1fr 1fr;

}
.card-title {
  /* line-height: 54px; */
  font-size: 14px;
  font-weight: 600;
  color: #333;
  padding-bottom: 14px;
}
.card-title-litter {
  font-size: 10px;
  color: #333;
  padding-bottom: 8px;
}
.big-number {
  font-size: 25px;
  font-weight: 600;
  color: #333;
}
</style>