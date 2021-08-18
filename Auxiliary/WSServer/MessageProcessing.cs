using Auxiliary.RequestMessage;
using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Auxiliary.RequestMessage.MessageClass;
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
                return ReturnInfoPackage.InfoPkak((int)MessageClass.ServerSendMessageCode.请求成功但出现了错误, new List<ServerClass.Login>() { new ServerClass.Login() { message = "服务器收到的数据不符合消息解析的必要条件，请检查数据格式", result = false } });
            }
            if (rlc.code == (int)MessageClass.ClientSendMessageCode.请求WebSocketWToken)
            {
                return CommandParsing.登陆.wss登陆处理(rlc.message, webSocketConnection);
            }
            else//修改房间Like配置
            {
                if(rlc.Token==webSocketConnection.Token)
                {
                    switch (rlc.code)
                    {
                        case (int)ClientSendMessageCode.获取系统运行情况:
                            return RequestMessage.封装消息.消息_获取系统消息.系统消息();
                        case (int)ClientSendMessageCode.查看当前配置文件:
                            return RequestMessage.封装消息.消息_获取配置文件信息.配置文件信息();
                        case (int)ClientSendMessageCode.检查更新:
                            return RequestMessage.封装消息.消息_获取检查更新信息.检查更新信息();
                        case (int)ClientSendMessageCode.获取系统运行日志:
                            return CommandParsing.获取日志信息.获取日志(rlc.message);
                        case (int)ClientSendMessageCode.获取当前录制中的队列简报:
                            return RequestMessage.封装消息.下载_获取当前录制中的任务队列简报信息.当前录制中的任务队列简报信息();
                        case (int)ClientSendMessageCode.获取所有下载任务的队列简报:
                            return RequestMessage.封装消息.下载_获取所有下载任务的简报队列信息.所有下载任务的简报队列信息();
                        case (int)ClientSendMessageCode.根据录制任务GUID获取任务详情:
                            return CommandParsing.获取录制任务详情.wss获取录制任务详情处理(rlc.message);
                        case (int)ClientSendMessageCode.根据录制任务GUID取消相应任务:
                            return CommandParsing.取消录制任务.wss取消录制任务(rlc.message);
                        case (int)ClientSendMessageCode.增加配置文件中监听的房间:
                            return CommandParsing.增加配置中监听的房间.增加房间(rlc.message);
                        case (int)ClientSendMessageCode.删除配置文件中监听的房间:
                            return CommandParsing.删除配置中的房间.删除房间配置(rlc.message);
                        case (int)ClientSendMessageCode.修改房间的自动录制开关配置:
                            return CommandParsing.修改房间自动录制配置.修改房间录制配置(rlc.message);
                        case (int)ClientSendMessageCode.修改房间Like配置:
                            return CommandParsing.修改房间Like配置.修改房间Liek(rlc.message);
                        case (int)ClientSendMessageCode.获取当前房间配置列表总览:
                            return RequestMessage.封装消息.消息_获取当前房间配置列表总览信息.当前房间配置列表总览信息();
                        case (int)ClientSendMessageCode.获取当前录制文件夹中的所有文件的列表:
                            return RequestMessage.封装消息.下载_获取当前录制文件夹中的所有文件的列表信息.当前录制文件夹中的所有文件的列表信息();
                        case (int)ClientSendMessageCode.删除某个录制完成的文件:
                            return CommandParsing.删除录制完成的文件.删除文件(rlc.message);
                        case (int)ClientSendMessageCode.根据房间号获得相关录制文件:
                            return CommandParsing.根据房间号获得录制的文件列表.获得文件列表(rlc.message);
                        case (int)ClientSendMessageCode.获取上传任务信息列表:
                            return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功, Upload.Uploader.UploadList);
                        case (int)ClientSendMessageCode.获取上传中的任务信息列表:
                            return RequestMessage.封装消息.上传_获取上传中的任务信息列表信息.上传中的任务信息列表信息();
                        case (int)ClientSendMessageCode.修改配置_自动转码设置:
                            return CommandParsing.修改设置_自动转码设置.转码设置(rlc.message);
                        default:
                            return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功但出现了错误, new List<ServerClass.Login>() { new ServerClass.Login() { message = "未适配的Code", result = false } });
                    }
                }
                else
                {
                    return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.鉴权失败, new List<ServerClass.Login>() { new ServerClass.Login() { message = "Token验证失败，请检查或重新获取Token", result = false } });
                }
            }        
        }
        internal class Pack
        {
            public int code { set; get; }
            public string Token { set; get; }
            public string message { set; get; }
        }
    }
}
