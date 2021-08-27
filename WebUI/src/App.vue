<template>
<!-- 这个是整个app的容器 -->
  <div class="app" v-if="$route.meta.keepAlive">
    <!-- 导航栏 -->
    <div class="project-nav">
      <el-menu class="el-menu-open" :default-active="$route.path" :collapse="isCollapse" :router="true">
        <div class="logo" >
          <el-image src="../static/logo.png" style="width:90px" v-show="!isCollapse"></el-image>
          <el-image src="../static/logo_mini.png" style="width:25px" v-show="isCollapse"></el-image>
          
        </div>
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
        <el-menu-item index="/tasks">
          <i class="el-icon-s-order"></i>
          <span slot="title">任务详情</span>
        </el-menu-item>
        <el-menu-item  onclick="window.open('https://ddtv.pro')">
          <i class="el-icon-s-management"></i>
          <span slot="title">查看文档</span>
        </el-menu-item>
      </el-menu>
      <div class="about">
        <i class="el-icon-more"></i>
      </div>
    </div>
    <!-- 网页主体 -->
    <div class="content">
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
      <div class="router-view">
        <router-view></router-view>
      </div>

    </div>
  </div>
    <router-view v-else></router-view>
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
    window.onresize = () => {
      return (() => {
        window.screenWidth = document.body.clientWidth;
        this.screenWidth = window.screenWidth;
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
    this.screenWidth = document.body.clientWidth
    if(this.screenWidth < 1300) this.isCollapse = true
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
  background-color: #effdff;
}
.app {
  flex: auto 1 1;
  height: 100%;
  display: flex;
  overflow: hidden;
  flex-direction: row;
}
.project-nav {
  background: #fff;
  box-shadow: 2px 0 10px rgba(0,0,0,.1);
  position: relative;
  z-index: 2;
  display: flex;
  flex-direction: column;
  padding-bottom: 30px;
}
.about {
  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: center;
  flex-wrap: nowrap;
}
.router-view {
  flex: 1;
  height: 0;
  overflow: auto;
  /* padding: 0 20px 20px 20px */
}
.router-view::-webkit-scrollbar {
    width: 10px;
    height: 10px;
}
.router-view::-webkit-scrollbar-thumb {
    background-color: #b4c7d0;
    border: 3px solid transparent;
    background-clip: padding-box;
    border-radius: 5px;
}
.router-view::-webkit-scrollbar-track-piece {
    background: 0 0;
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
  display: flex;
  flex-direction:column;
}
.el-menu-open:not(.el-menu--collapse) {
    width: 220px;
}
.content {
  flex: auto 1 1;
  width: 0;
  overflow-x: hidden;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}
.top-bar {
  padding: 16px;
  display: flex;
  align-items: center;
  height: 32px;
  background: #ffffff;
  justify-content: space-between;
  border-bottom: 1px solid #e6e4e4;
  z-index: 99;
}
.top-bar-title {
  font-size: 28px;
  font-weight: 300;
}
</style>
