<template>
  <div class="login login-background">
    <div class="login-logo">
      <el-image src="../static/logo.png" class="login-logo-img"></el-image>
      <div class="login-logo-title"></div>
    </div>
    <el-card style="width: 391px;">
      <el-form :model="loginForm" :rules="rules" ref="loginForm">
        <el-form-item label="用户名" prop="user">
          <el-input type="text" v-model="loginForm.user" autocomplete="off"></el-input>
        </el-form-item>
        <el-form-item label="密码" prop="pass">
          <el-input type="password" v-model="loginForm.pass" autocomplete="off"></el-input>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="submitForm('loginForm')">登录</el-button>
          <el-button @click="resetForm('loginForm')">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>
 
<script>
import {postFormAPI} from '../api'
export default {
  name: "Login",
  components: {},
  data() {
    return {
      // 存放表单数据的对象
      loginForm: {user: "",pass: "",},
      // 验证规则
      rules: {
        user: [{ required: true, message: "用户名不能为空", trigger: "blur" }],
        pass: [{ required: true, message: "密码不能为空", trigger: "blur" }],
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
        this.$router.push('/')
        }
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
          this.userlogin(this.loginForm.user,this.loginForm.pass);
        } else {
          console.debug("用户还没有通过表单验证");
          return false;
        }
      });
    },

    /**
    * 调用本函数将清空指定表单
    * 
    * @param {formName} 表单名 str
    */
    resetForm(formName) {
      this.$refs[formName].resetFields();
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
  background: rgb(251, 202, 202);
  /* 设置定位和左右上下 撑开 */
  position: absolute;
  left: 0;
  right: 0;
  bottom: 0;
  top: 0;
}
.login-background {
  background: linear-gradient(to top,rgb(0 0 0 / 59%),rgb(0 0 0 / 62%)),url("../../public/static/loginBack.jpg");
  background-size: cover;
  background-position: center;
}
.login-logo {
  position: fixed;
  top: 20px;
  display: flex;
  left: 20px;
  justify-content: space-between;
}
.login-logo-title{
  color: #fff;
  font-size: 28px;
}
.login-logo-img{
  width: 83px;
}
</style>