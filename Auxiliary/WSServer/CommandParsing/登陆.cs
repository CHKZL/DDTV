using Auxiliary.RequestMessage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.WSServer.WSServer;

namespace Auxiliary.WSServer.CommandParsing
{
    internal class 登陆
    {
        internal static string wss登陆处理(string mess, WebSocket连接封装 webSocketConnection)
        {
            LoginMessage Login = new LoginMessage();
            try
            {
                JObject JO = (JObject)JsonConvert.DeserializeObject(mess);
                Login.UserName = JO["UserName"].ToString();
                Login.Password = JO["Password"].ToString();
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak((int)MessageClass.ServerSendMessageCode.请求成功但出现了错误, new List<ServerClass.Login>() { new ServerClass.Login() { message = "服务器收到的数据不符合消息解析的必要条件，请检查数据格式", result = false } });
            }
            if (Login.UserName == MMPU.WebSocketUserName && Login.Password == MMPU.WebSocketPassword)
            {
                webSocketConnection.Token = Guid.NewGuid().ToString();
                return ReturnInfoPackage.InfoPkak((int)MessageClass.ServerSendMessageCode.请求成功, new List<ServerClass.Login>() { new ServerClass.Login() { message = "请求成功，返回WS用Token，之后请求请附带该Token", result = true, WebToken = webSocketConnection.Token } });
            }
            else
            {
                return ReturnInfoPackage.InfoPkak((int)MessageClass.ServerSendMessageCode.鉴权失败, new List<ServerClass.Login>() { new ServerClass.Login() { message = "账号或密码有误", result = false } });
            }
        }
        internal class LoginMessage
        {
            internal string UserName { set; get; }
            internal string Password { set; get; }
        }
    }
}
