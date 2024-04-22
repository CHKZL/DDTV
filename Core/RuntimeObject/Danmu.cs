using Core.LogModule;
using Masuit.Tools;
using Microsoft.AspNetCore;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Network.Methods.Room;
using static Core.RuntimeObject.Danmu;

namespace Core.RuntimeObject
{
    public class Danmu
    {
        #region Private Propertiess

        #endregion

        #region Public Method
        /// <summary>
        /// 保存弹幕相关文件
        /// </summary>
        /// <param name="liveChatListener"></param>
        /// <param name="DeleteCurrentContent">只删除当前内容，不保存</param>
        /// <param name="card"></param>
        public static void SevaDanmu(LiveChat.LiveChatListener liveChatListener,bool DeleteCurrentContent, ref RoomCardClass card)
        {
            string Message = "保存弹幕相关文件";
            if (!DeleteCurrentContent)
            {
                string File = liveChatListener.File + $"_{liveChatListener.SaveCount}";
                if (liveChatListener.DanmuMessage.Danmu != null && liveChatListener.DanmuMessage.Danmu.Count > 0)
                {
                    FileInfo fileInfo = SevaDanmu(liveChatListener.DanmuMessage.Danmu, File, liveChatListener.Name, liveChatListener.RoomId);

                    Log.Info(nameof(SevaDanmu), $"{liveChatListener.Name}({liveChatListener.RoomId})保存弹幕相关文件为{File}");
                    card.DownInfo.DownloadFileList.DanmuFile.Add(fileInfo.FullName);
                }
                if (liveChatListener.DanmuMessage.Gift != null && liveChatListener.DanmuMessage.Gift.Count > 0)
                {
                    FileInfo fileInfo = SevaGift(liveChatListener.DanmuMessage.Gift, File);
                    Log.Info(nameof(SevaDanmu), $"{liveChatListener.Name}({liveChatListener.RoomId})保存送礼记录相关文件为{File}");
                    card.DownInfo.DownloadFileList.GiftFile.Add(fileInfo.FullName);
                }
                if (liveChatListener.DanmuMessage.GuardBuy != null && liveChatListener.DanmuMessage.GuardBuy.Count > 0)
                {
                    FileInfo fileInfo = SevaGuardBuy(liveChatListener.DanmuMessage.GuardBuy, File);
                    Log.Info(nameof(SevaDanmu), $"{liveChatListener.Name}({liveChatListener.RoomId})保存上舰记录相关文件为{File}");
                    card.DownInfo.DownloadFileList.GuardFile.Add(fileInfo.FullName);
                }
                if (liveChatListener.DanmuMessage.SuperChat != null && liveChatListener.DanmuMessage.SuperChat.Count > 0)
                {
                    FileInfo fileInfo = SevaSuperChat(liveChatListener.DanmuMessage.SuperChat, File);
                    Log.Info(nameof(SevaDanmu), $"{liveChatListener.Name}({liveChatListener.RoomId})保存SC记录相关文件为{File}");
                    card.DownInfo.DownloadFileList.SCFile.Add(fileInfo.FullName);
                }

            }
            else
            {
                Message = "视频文件不存在，清理多余弹幕文件";
            }
            if (liveChatListener.TimeStopwatch != null)
            {
                liveChatListener.TimeStopwatch.Restart();
            }
            liveChatListener.DanmuMessage = new();

            liveChatListener.SaveCount++;

            
            OperationQueue.Add(Opcode.Download.SaveBulletScreenFile, Message, card.UID);
            Log.Info(nameof(SevaDanmu), Message);
        }
        #endregion

        #region Private Method
        /// <summary>
        /// 储存原始弹幕信息到xml文件
        /// </summary>
        /// <param name="danmuInfo"></param>
        /// <param name="FileName"></param>
        /// <param name="Name"></param>
        /// <param name="roomId"></param>
        private static FileInfo SevaDanmu(List<DanmuInfo> danmuInfo, string FileName, string Name, long roomId)
        {
            string XML = string.Empty;
            XML = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<i>" +
            "<chatserver>chat.bilibili.com</chatserver>" +
            "<chatid>0</chatid>" +
            "<mission>0</mission>" +
            "<maxlimit>2147483647</maxlimit>" +
            "<state>0</state>" +
            $"<app>{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "-" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}</app>" +
            $"<real_name>{Name}</real_name>" +
            $"<roomid>{roomId}</roomid>" +
            $"<source>k-v</source>";
            int i = 1;
            List<DanmuInfo> DanmaList = danmuInfo.DeepClone();
            foreach (var item in DanmaList)
            {
                XML += $"<d p=\"{item.time:f4},{item.type},{item.size},{item.color},{item.timestamp / 1000},{item.pool},{item.uid},{i}\">{XMLEscape(item.Message)}</d>\r\n";
                i++;
            }
            
            XML += "</i>";
            File.WriteAllText(FileName + ".xml", XML);
            if (true)
            {
                string Gift = "时间,昵称,Uid,弹幕内容";
                foreach (var item in DanmaList)
                {
                    Gift += $"\r\n{item.time},{item.Nickname},{item.uid},{item.Message}";
                }
            
                File.WriteAllText(FileName + "_礼物.csv", Gift, Encoding.UTF8);
            }
            DanmaList = null;
            return new FileInfo(FileName + ".xml");
        }
        /// <summary>
        /// 对XML特殊字符进行转义
        /// </summary>
        /// <param name="Message">待转义消息</param>
        /// <returns></returns>
        private static string XMLEscape(string Message)
        {
            return Message.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("'", "&apos;")
                .Replace("\"", "&quot;");
            //.Replace(" ", "&nbsp;")
            //.Replace("×", "&times;")
            //.Replace("÷", "&divde;");
        }
        /// <summary>
        /// 储存原始礼物信息到文件
        /// </summary>
        /// <param name="GiftInfo"></param>
        /// <param name="FileName"></param>
        public static FileInfo SevaGift(List<GiftInfo> GiftInfo, string FileName)
        {
            string Gift = "视频时间,送礼人昵称,送礼人Uid,礼物名称,礼物数量,礼物单价,时间戳";
            List<GiftInfo> A = GiftInfo.DeepClone();
            foreach (var item in A)
            {
                Gift += $"\r\n{item.Time},{item.UserName},{item.UserId},{item.GiftName},{item.Amount},{item.Price},{item.Timestamp}";
            }
            A = null;
            File.WriteAllText(FileName + "_礼物.csv", Gift, Encoding.UTF8);
            return new FileInfo(FileName + "_礼物.csv");
        }
        /// <summary>
        /// 储存原始舰队信息到文件
        /// </summary>
        /// <param name="guardBuyInfos"></param>
        /// <param name="FileName"></param>
        public static FileInfo SevaGuardBuy(List<GuardBuyInfo> guardBuyInfos, string FileName)
        {
            string Gift = "视频时间,送礼人昵称,送礼人Uid,上舰类型,上舰时间,每月价格,时间戳";
            List<GuardBuyInfo> A = guardBuyInfos.DeepClone();
            foreach (var item in A)
            {
                string Level = item.GuardLevel == 1 ? "总督" : item.GuardLevel == 2 ? "提督" : item.GuardLevel == 3 ? "舰长" : item.GuardLevel.ToString();
                Gift += $"\r\n{item.Time},{item.UserName},{item.UserId},{Level},{item.Number},{item.Price},{item.Timestamp}";
            }
            A = null;
            File.WriteAllText(FileName + "_舰队.csv", Gift, Encoding.UTF8);
            return new FileInfo(FileName + "_舰队.csv");
        }
        /// <summary>
        /// 储存原始SC信息到文件
        /// </summary>
        /// <param name="superChatInfos"></param>
        /// <param name="FileName"></param>
        public static FileInfo SevaSuperChat(List<SuperChatInfo> superChatInfos, string FileName)
        {
            string Gift = "视频时间,送礼人昵称,送礼人Uid,SC金额,消息原文,翻译消息,时间戳";
            List<SuperChatInfo> A = superChatInfos.DeepClone();
            foreach (var item in A)
            {
                Gift += $"\r\n{item.Time},{item.UserName},{item.UserId},{item.Price},{item.Message},{item.MessageTrans},{item.Timestamp}";
            }
            A = null;
            File.WriteAllText(FileName + "_SC.csv", Gift, Encoding.UTF8);
            return new FileInfo(FileName + "_SC.csv");
        }
        #endregion

        #region Public Class
        public class DanmuMessage
        {
            public string FileName { set; get; }
            public Stopwatch TimeStopwatch { set; get; }
            /// <summary>
            /// 弹幕信息
            /// </summary>
            public List<DanmuInfo> Danmu { set; get; } = new();
            /// <summary>
            /// SC信息
            /// </summary>
            public List<SuperChatInfo> SuperChat { set; get; } = new();
            /// <summary>
            /// 礼物信息
            /// </summary>
            public List<GiftInfo> Gift { set; get; } = new();
            /// <summary>
            /// 舰队信息
            /// </summary>
            public List<GuardBuyInfo> GuardBuy { set; get; } = new();
        }
        public class DanmuInfo
        {
            /// <summary>
            /// 弹幕在视频里的时间
            /// </summary>
            public double time { set; get; }
            /// <summary>
            /// 弹幕类型
            /// </summary>
            public int type { set; get; }
            /// <summary>
            /// 弹幕大小
            /// </summary>
            public int size { set; get; }
            /// <summary>
            /// 弹幕颜色
            /// </summary>
            public int color { set; get; }
            /// <summary>
            /// 时间戳
            /// </summary>
            public long timestamp { set; get; }
            /// <summary>
            /// 弹幕池
            /// </summary>
            public int pool { set; get; }
            /// <summary>
            /// 发送者UID
            /// </summary>
            public long uid { set; get; }
            /// <summary>
            /// 弹幕信息
            /// </summary>
            public string Message { set; get; }
            /// <summary>
            /// 发送人昵称
            /// </summary>
            public string Nickname { set; get; }
            /// <summary>
            /// 发送人舰队等级
            /// </summary>
            public int LV { set; get; }
        }
        public class SuperChatInfo
        {
            /// <summary>
            /// 送礼的时候在视频里的时间
            /// </summary>
            public double Time { set; get; }
            /// <summary>
            /// 时间戳
            /// </summary>
            public long Timestamp { set; get; }
            /// <summary>
            /// 打赏人UID
            /// </summary>
            public long UserId { set; get; }
            /// <summary>
            /// 打赏人昵称
            /// </summary>
            public string UserName { set; get; }
            /// <summary>
            /// SC金额
            /// </summary>
            public double Price { set; get; }
            /// <summary>
            /// SC消息内容
            /// </summary>
            public string Message { set; get; }
            /// <summary>
            /// SC消息内容_翻译后
            /// </summary>
            public string MessageTrans { set; get; }
            /// <summary>
            /// SC消息的持续时间
            /// </summary>
            public int TimeLength { set; get; }
        }
        public class GiftInfo
        {
            /// <summary>
            /// 送礼的时候在视频里的时间
            /// </summary>
            public double Time { set; get; }
            /// <summary>
            /// 时间戳
            /// </summary>
            public long Timestamp { set; get; }
            /// <summary>
            /// 送礼人UID
            /// </summary>
            public long UserId { set; get; }
            /// <summary>
            /// 送礼人昵称
            /// </summary>
            public string UserName { set; get; }
            /// <summary>
            /// 礼物数量
            /// </summary>
            public int Amount { set; get; }
            /// <summary>
            /// 花费：单位金瓜子
            /// </summary>
            public float Price { set; get; }
            /// <summary>
            /// 礼物名称
            /// </summary>
            public string GiftName { set; get; }
        }
        public class GuardBuyInfo
        {
            /// <summary>
            /// 送礼的时候在视频里的时间
            /// </summary>
            public double Time { set; get; }
            /// <summary>
            /// 时间戳
            /// </summary>
            public long Timestamp { set; get; }
            /// <summary>
            /// 上舰人UID
            /// </summary>
            public long UserId { set; get; }
            /// <summary>
            /// 上舰人昵称
            /// </summary>
            public string UserName { set; get; }
            /// <summary>
            /// 开通了几个月
            /// </summary>
            public int Number { set; get; }
            /// <summary>
            /// 开通的舰队名称
            /// </summary>
            public string GuradName { set; get; }
            /// <summary>
            /// 舰队等级：1-总督 2-提督 3-舰长
            /// </summary>
            public int GuardLevel { set; get; }
            /// <summary>
            /// 花费：单位金瓜子
            /// </summary>
            public int Price { set; get; }
        }
        #endregion

    }
}
