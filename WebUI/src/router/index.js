// router > index.js VueRouter 配置 BY @NGWORKS
import Vue from 'vue'
import VueRouter from 'vue-router'
import Home from '../views/Home.vue'
import Login from '../views/Login.vue'
import File from '../views/file.vue'
import Room from '../views/room.vue'
import Play from '../views/play.vue'

Vue.use(VueRouter)

const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home,
    meta: {
      keepAlive: true,
      title:'概览'
    }
  },
  {
    path: '/file',
    name: 'File',
    component: File,
    meta: {
      keepAlive: true,
      title:'文件管理'
    }
  },
  {
    path: '/play',
    name: 'Play',
    component: Play,
    meta: {
      keepAlive: true,
      title:'播放'
    }
  },
  {
    path: '/room',
    name: 'Room',
    component: Room,
    meta: {
      keepAlive: true,
      title:'房间管理'
    }
  },
  {
    path: '/login',
    name: 'Login',
    component: Login,
    meta:{
      title:"登录",
      keepAlive: false
      
    }
  }
]

const router = new VueRouter({
  mode: 'history',
  routes
})

export default router
// 注册一个全局前置守卫 主要对用户的登录状态进行管理
router.beforeEach((to, from, next) => {
  // 动态添加 title 后面拼接一个名称
  document.title =  to.meta.title.concat(" - 你的地表最强B站录播机")

  // 判断鉴权  逻辑:如果在除了 Login 的其他页面 且没有登录状态的 定向到登录页
  // TODO 用 VUEX 对全局登录状态进行管理
  let isAuthenticated = sessionStorage.getItem("token")
  if (to.name !== 'Login' && !isAuthenticated) next({ name: 'Login' })  

  // 如果用户登录了还想要回到登录页 取消跳转
  if (to.name == 'Login' && isAuthenticated) next(false)
  else next()

})