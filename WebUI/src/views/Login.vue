<template>
  <div class="login login-background" v-loading="load">
    <div class="login-box">
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

      <div class="login-bt blue-bg" @click="!show ? userlogin():tokenlogin()">{{show ? '验证登录':'登录'}}</div>
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
      windth:window.screenWidth,
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
    userlogin:async function() {
      // 构建请求参数
      let ts = this.submitForm('loginForm')
      if(ts == false) return false
      this.load = true
      let param = {
        "WebUserName": this.loginForm.user,
        "WebPassword": this.loginForm.pass
      }
      // 发请求
      try {
        let res = await postFormAPI('weblogin',param,false)
        if(!res.data.result) this.openWindows(res.data.message,'登录出现问题')
        else {
          sessionStorage.setItem("token",res.data.WebToken)
          sessionStorage.setItem("ver",1)
          this.$router.push('/')
          }
      }
      catch(err) {
        console.error("登录请求出错，请检查网络连接与网站配置。")
        this.openWindows('登录出现问题,请检查网络连接与网站配置。','网络连接失败')
      }
      finally {
        this.load = false
      }
    },

    /**
    * 本函数封装了请求后端接口获取 system_info 的方法
    * 
    * @return 接口返回的数据
    */
    system_info: async function () {
      let param = pubBody('system_info')
      let response = await postFormAPI('system_info',param,true)
      this.system_info_data = response.data.Package[0];
      return response.data;
    },

    tokenlogin: async function (){
      let ts = this.submitForm('tokenForm')
      if(ts == false) return false
      this.load = true
      try {
        let result = await this.system_info()
        if(result.data.code != 1001) this.openWindows(result.data.message,'登录出现问题')
        else{
          sessionStorage.setItem("token",this.tokenForm.token)
          sessionStorage.setItem("ver",2)
          this.$router.push('/')
        }
      }
      catch(err) {
        console.error("登录请求出错，请检查网络连接与网站配置。")
        this.openWindows('登录出现问题,请检查网络连接与网站配置。','网络连接失败')
      }
      finally {
        this.load = false
      }
    },

    /**
    * 本函数会进行表单检查
    * 
    * @param {formName} 表单名  str
    */
    submitForm: function(formName) {
      let submit = false
      this.$refs[formName].validate((valid) => {
        if (valid) {
          submit = true
        } else {
          console.debug("用户还没有通过表单验证");
        }
      });
      return submit
    },
  },
};
</script>
 
<style scoped>
.login {
  display: flex;
  flex-direction: column;
  background: rgb(255, 255, 255);
  position: absolute;
  left: 0;
  right: 0;
  bottom: 0;
  top: 0;
  padding-left: 40px;
  padding-right: 40px;
  justify-content: space-evenly;
  align-items: center;
}
.login-box{
  min-width: 320px;
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