<template>
<!-- 这个是整个app的容器 -->
  <div class="app" v-if="$route.meta.keepAlive">
    <!-- 导航栏 -->
    <div class="LeftMenu">
      <el-menu class="el-menu-open" :default-active="$route.path" :collapse="isCollapse" :router="true">
        <il class="logo" >
          <el-image src="./logo.png" style="width:25px"></el-image>
          <transition name="el-zoom-in-center" v-if="!isCollapse">
          <div class="logotitle" >DDTV</div>
          </transition>
        </il>
        <el-menu-item index="/">
          <i class="el-icon-location"></i>
          <span slot="title">概览</span>
        </el-menu-item>
        <el-menu-item index="/room">
          <i class="el-icon-menu"></i>
          <span slot="title">房间管理</span>
        </el-menu-item>
        <el-menu-item index="/file">
          <i class="el-icon-document"></i>
          <span slot="title">文件管理</span>
        </el-menu-item>
        <el-menu-item  onclick="window.open('https://ddtv.pro')">
          <i class="el-icon-s-management"></i>
          <span slot="title">查看文档</span>
        </el-menu-item>
      </el-menu>
    </div>
    <!-- 网页主体 -->
    <div class="MainBox">
      <!-- 标题栏 -->
      <div class="top-bar">
        <div class="top-bar-title" v-if="$route.meta.title">{{ $route.meta.title }}</div>
        <a href="https://github.com/CHKZL/DDTV2" target="_blank" rel="noopener noreferrer" style="font-weight: 500;color: #000;text-decoration: none;font-weight: 500;margin-left;text-decoration: none;">
          GitHub
          <span>
            <svg xmlns="http://www.w3.org/2000/svg" aria-hidden="true" focusable="false" x="0px" y="0px" viewBox="0 0 100 100" width="15" height="15" class="icon outbound"><path fill="currentColor" d="M18.8,85.1h56l0,0c2.2,0,4-1.8,4-4v-32h-8v28h-48v-48h28v-8h-32l0,0c-2.2,0-4,1.8-4,4v56C14.8,83.3,16.6,85.1,18.8,85.1z"></path> <polygon fill="currentColor" points="45.7,48.7 51.3,54.3 77.2,28.5 77.2,37.2 85.2,37.2 85.2,14.9 62.8,14.9 62.8,22.9 71.5,22.9"></polygon>
            </svg>
          </span>
        </a>
          </div>
      
      <!-- 对应路由的内容 -->
      <router-view></router-view>

    </div>
  </div>
  <div v-else class="app loginBackground">
    <router-view></router-view>
  </div>
</template>

<script>
export default {
  data() {
    return {
      isCollapse: false,
      screenWidth: document.body.clientWidth,
    };
  },
  mounted() {
    console.log(document.body.clientWidth)
    this.screenWidth = document.body.clientWidth
    if(this.screenWidth < 1300) this.isCollapse = true
    const that = this;
    window.onresize = () => {
      return (() => {
        window.screenWidth = document.body.clientWidth;
        that.screenWidth = window.screenWidth;
      })();
    };
  },
  watch: {
    screenWidth(val) {
      // 为了避免频繁触发resize函数导致页面卡顿，使用定时器
      if (!this.timer) {
        // 一旦监听到的screenWidth值改变，就将其重新赋给data里的screenWidth
        this.screenWidth = val;
        this.timer = true;
        let that = this;
        setTimeout(function () {
          // 打印screenWidth变化的值
          console.log(that.screenWidth);
          that.timer = false;
        }, 400);
        if (that.screenWidth < 1300) {
          this.isCollapse = true;
        } else {
          this.isCollapse = false;
        }
      }
    },
  },
  methods: {
    handleOpen(key, keyPath) {
      console.log(key, keyPath);
    },
    handleClose(key, keyPath) {
      console.log(key, keyPath);
    },
    getwindth() {
      this.screenWidth = window.innerWidth;
    },
  },
  created() {
    window.addEventListener("resize", this.getwindth);
    this.getwindth();
  },
  destroyed() {
    window.removeEventListener("resize", this.getwindth);
  },
};
</script>



<style>
html,body{
  height: 100%;
}
body {
  margin: 0;
  display: flex;
  flex-direction: column;
  background-color: #effdff;
}
.loginBackground{
  /*登录页背景 */
  background-color: #ffdcdc;
}
.app{
  /* 这个是本项目最外层容器 */
  display: flex;
  /* flex 对象 从左向右排列，且向两端对齐 
  高度未定时则占满整个容器 */
  align-items: stretch;
  /* height: 100%; */
  align-content: stretch;
  flex-wrap: nowrap;
  flex-direction: row;
  justify-content: space-between;
  flex-grow: 1;
}
.LeftMenu{
  /*这个是左侧的菜单栏 在一个容器中排序永远靠前 故为-1*/
  order:-1;
  /*即使空间不足 也不缩小 */
  flex-shrink:0
}
.icon.outbound {
    color: #aaa;
    display: inline-block;
    vertical-align: middle;
    position: relative;
    top: -1px;
}
.logo{
  height: 65px;
  display: flex;
  flex-direction: row;
  align-items: center;
  /* padding: 20px 20px 20px 20px; */
  justify-content: center;
}
.logotitle{
  font-size: 28px;
  font-weight: 200;
  margin-left: 15px;
  color: rgb(117, 112, 112);
}
.el-menu-open{
  height: 100%;
  /*菜单栏 flex 布局 */
  display: flex;
  /*从上至下排列 */
  flex-direction:column;
  /*投影 */
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1)
}
.el-menu-open:not(.el-menu--collapse) {
  /*这里规定了在非拉伸状态下的菜单栏宽度 */
    width: 220px;
}
.MainBox{
  /*这个是主要内容的对象*/
  display: flex;
  flex-direction: column;
  /* width: 100%; */
  justify-content: flex-start;
  align-content: stretch;
  align-items: stretch;
  flex-grow: 1
}
.top-bar {
  padding: 16px;
  display: flex;
  flex-direction: row;
  align-items: center;
  position: relative;
  height: 32px;
  z-index: 1;
  /* background: #bbe6d6; */
  border-block: solid 1px #e6e6e6;
  background-color: #ffffff;
  align-content: stretch;
  justify-content: space-between;
}
.top-bar-title {
  font-size: 28px;
  font-weight: 300;
}
</style>
