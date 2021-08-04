using Auxiliary.RequestMessge;
using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Auxiliary.RequestMessge.MessgeClass;
using static Auxiliary.WSServer.WSServer;

namespace Auxiliary.WSServer
{
    class MessageProcessing
    {
        public static string 消息解析(string mess, WebSocket连接封装 webSocketConnection)
        {
            Pack rlc = new Pack();
            try
            {
                rlc = JsonConvert.DeserializeObject<Pack>(mess);
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak((int)MessgeClass.ServerSendMessgeCode.请求成功但出现了错误, new List<ServerClass.Login>() { new ServerClass.Login() { messge = "服务器收到的数据不符合消息解析的必要条件，请检查数据格式", result = false } });
            }
            if (rlc.code == (int)MessgeClass.ClientSendMessgeCode.请求WebSocketWToken)
            {
                return CommandParsing.登陆.wss登陆处理(rlc.messge, webSocketConnection);
            }
            else
            {
                if(rlc.Token==webSocketConnection.Token)
                {
                    switch (rlc.code)
                    {
                        case (int)ClientSendMessgeCode.获取系统运行情况:
                            return RequestMessge.封装消息.获取系统消息.系统消息();
                        case (int)ClientSendMessgeCode.查看当前配置文件:
                            return RequestMessge.封装消息.获取配置文件信息.配置文件信息();
                        case (int)ClientSendMessgeCode.检查更新:
                            return RequestMessge.封装消息.获取检查更新信息.检查更新信息();
                        case (int)ClientSendMessgeCode.获取系统运行日志:
                            return "";
                        case (int)ClientSendMessgeCode.获取当前录制中的队列简报:
                            return RequestMessge.封装消息.获取当前录制中的任务队列简报信息.当前录制中的任务队列简报信息();
                        case (int)ClientSendMessgeCode.获取所有下载任务的队列简报:
                            return RequestMessge.封装消息.获取所有下载任务的简报队列信息.所有下载任务的简报队列信息();
                        case (int)ClientSendMessgeCode.根据录制任务GUID获取任务详情:
                            return CommandParsing.获取录制任务详情.wss获取录制任务详情处理(rlc.messge);
                        case (int)ClientSendMessgeCode.根据录制任务GUID取消相应任务:
                            return CommandParsing.取消录制任务.wss取消录制任务(rlc.messge);
                        case (int)ClientSendMessgeCode.增加配置文件中监听的房间:
                            return CommandParsing.增加配置中监听的房间.增加房间(rlc.messge);
                        case (int)ClientSendMessgeCode.删除配置文件中监听的房间:
                            return CommandParsing.删除配置中的房间.删除房间配置(rlc.messge);
                        case (int)ClientSendMessgeCode.修改房间的自动录制开关配置:
                            return CommandParsing.修改房间自动录制配置.修改房间录制配置(rlc.messge);
                        case (int)ClientSendMessgeCode.获取当前房间配置列表总览:
                            return RequestMessge.封装消息.获取当前房间配置列表总览信息.当前房间配置列表总览信息();
                        case (int)ClientSendMessgeCode.获取当前录制文件夹中的所有文件的列表:
                            return RequestMessge.封装消息.获取当前录制文件夹中的所有文件的列表信息.当前录制文件夹中的所有文件的列表信息();
                        case (int)ClientSendMessgeCode.删除某个录制完成的文件:
                            return CommandParsing.删除录制完成的文件.删除文件(rlc.messge);
                        case (int)ClientSendMessgeCode.根据房间号获得相关录制文件:
                            return CommandParsing.根据房间号获得录制的文件列表.获得文件列表(rlc.messge);
                        case (int)ClientSendMessgeCode.获取上传任务信息列表:
                            return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功, Upload.Uploader.UploadList);
                        case (int)ClientSendMessgeCode.获取上传中的任务信息列表:
                            return RequestMessge.封装消息.获取上传中的任务信息列表信息.上传中的任务信息列表信息();
                        default:
                            return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.请求成功但出现了错误, new List<ServerClass.Login>() { new ServerClass.Login() { messge = "未适配的Code", result = false } });
                    }
                }
                else
                {
                    return ReturnInfoPackage.InfoPkak((int)ServerSendMessgeCode.鉴权失败, new List<ServerClass.Login>() { new ServerClass.Login() { messge = "Token验证失败，请检查或重新获取Token", result = false } });
                }
            }        
        }
        internal class Pack
        {
            public int code { set; get; }
            public string Token { set; get; }
            public string messge { set; get; }
        }
    }
}
