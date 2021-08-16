import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)
// 这是 vuex 的对象 本项目会用其管理大多数的状态
export default new Vuex.Store({
  state: {
    token:null
  },
  mutations: {
    settoken (state,token){
      state.token=token//将新传过来的token赋值给state.token
    }
  },
  actions: {
    //这里的 token 就是从组件中dispatch传过来的,ctx为上下文对象
    settoken(ctx, token) {
      ctx.commit("settoken", token);//用commit来调用Mutations
    },
  },
  modules: {
  }
})
