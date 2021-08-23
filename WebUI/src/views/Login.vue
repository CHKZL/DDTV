<template>
  <div class="login login-background" v-loading="load">
    <div style="width: 391px;z-index:99">
      <el-image src="../static/logo.png" class="login-logo-img"></el-image>
      <div class="login-title">登录</div>
      <el-form :model="loginForm" :rules="rules" ref="loginForm" v-show="!show">
        <el-form-item label="用户名" prop="user">
          <el-input type="text" v-model="loginForm.user" autocomplete="off"></el-input>
        </el-form-item>

        <el-form-item label="密码" prop="pass">
          <el-input type="password" v-model="loginForm.pass" autocomplete="off"></el-input>
        </el-form-item>
      </el-form>
      <el-form  :model="tokenForm" :rules="tokenrules" ref="tokenForm" v-show="show">
        <el-form-item label="TOKEN 密钥" prop="token">
          <el-input v-model="tokenForm.token" autocomplete="off"></el-input>
        </el-form-item>
      </el-form>

      <div class="login-bt blue-bg" @click="!show ? submitForm('loginForm'):tokenlogin()">{{show ? '验证登录':'登录'}}</div>
      <div class="login-or">-OR-</div>
      <div class="login-bt blue-border" @click="show = !show">{{!show ? 'TOKEN USER':'账号密码登录'}}</div>
    </div>
  </div>
</template>
 
<script>
import {postFormAPI,pubBody} from '../api'
export default {
  name: "Login",
  components: {},
  data() {
    return {
      show:false,
      load:false,
      // 存放表单数据的对象
      loginForm: {user: "",pass: ""},
      tokenForm:{token:""},
      // 验证规则
      rules: {
        user: [{ required: true, message: "用户名不能为空"}],
        pass: [{ required: true, message: "密码不能为空"}],
      },
      tokenrules:{
        token:[{required: true, message: "token不能为空"}]
      }

    };
  },
  methods: {
    /**
    * 本函数会使用所给的title和message构造一个全屏黑底弹窗
    * 
    * @param {massage} 弹窗展示的消息  str
    * @param {title}  弹窗展示的标题  str
    */
    openWindows(message, title) {
      // message 与 title 对弹窗进行描述
      this.$alert(message, title, {
        confirmButtonText: "确定",
      });
    },
    
    /**
    * 本函数会进行登录请求，成功会保存token至session
    * 失败会弹窗提示
    * @param {loginname} 登录名  str
    * @param {password}  密码  str
    */
    userlogin:async function(loginname,password) {
      // 构建请求参数
      let param = {
        "WebUserName": loginname,
        "WebPassword": password
      }
      // 发请求
      let res = await postFormAPI('weblogin',param,false)
      // 解析请求数据
      if(!res.data.result) this.openWindows(res.data.message,'登录出现问题')
      else {
        sessionStorage.setItem("token",res.data.WebToken)
        sessionStorage.setItem("ver",1)
        this.$router.push('/')
        }
      this.load = false
    },

    /**
    * 本函数封装了请求后端接口获取 system_info 的方法
    * 
    * @return 接口返回的数据
    */
    system_info: async function () {
      let param = pubBody('system_info')
      param.ver = 2
      param.token = this.tokenForm.token
      console.log(param)
      let response = await postFormAPI('system_info',param,true)
      return response;
    },

    tokenlogin: async function (){
      this.load = true
      this.system_info().then(result => {
        if(result.data.code != 1001) this.openWindows(result.data.message,'登录出现问题')
        else{
          sessionStorage.setItem("token",this.tokenForm.token)
          sessionStorage.setItem("ver",2)
          this.$router.push('/')
        }
      })
      this.load = false
    },

    /**
    * 本函数会进行表单检查
    * 
    * @param {formName} 表单名  str
    */
    submitForm(formName) {
      this.$refs[formName].validate((valid) => {
        if (valid) {
          // 发送登录请求
          this.load = true
          this.userlogin(this.loginForm.user,this.loginForm.pass);
        } else {
          console.debug("用户还没有通过表单验证");
          return false;
        }
      });
    },
  },
};
</script>
 
<style scoped>
.login {
  /* 布局容器设定居中 */
  display: flex;
  flex-direction: column;
  align-content: stretch;
  justify-content: space-evenly;
  align-items: center;
  background: rgb(255, 255, 255);
  /* 设置定位和左右上下 撑开 */
  position: absolute;
  left: 0;
  right: 0;
  bottom: 0;
  top: 0;
}
.login-background {
  /* background: linear-gradient(to top,rgb(0 0 0 / 59%),rgb(0 0 0 / 62%)),url("../../public/static/loginBack.jpg"); */
  background-size: cover;
  background-position: center;
}
.login-bt {
  height: 40px;
  border-radius: 8px;
  text-align: center;
  display: flex;
  flex-direction: column;
  justify-content: center;
}
.blue-bg{
  color: #fff;
  background-color: #5688ff;
}
.blue-bg:hover{
  background-color: #83a6f6;
}
.blue-border{
  color: #5688ff;
  border: 1px solid #c083e4;
}
.blue-border:hover{
  border: 1px solid #e2afff;
}
.login-or {
  display: flex;
  flex-direction: row;
  justify-content: center;
  color: rgb(134, 134, 134);
  padding: 10px 0 10px 0;
}
.login-title {
  color: #000;
  font-size: 22px;
  padding: 10px 0 5px 0;
}
.login-logo-img{
  width: 75px;
}
</style>