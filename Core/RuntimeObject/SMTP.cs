using Masuit.Tools.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.LogModule;

namespace Core.RuntimeObject
{
    public class SMTP
    {
        private static bool Enable = false;

        private static string MAIL_STR = "<h1>DDTV</h1><p>有新的信息通知</p><hr><h4>${Event}</h4><p>${Msg}</p>";

        /// <summary>
        /// 邮件事件
        /// </summary>
        public static event EventHandler<SMTP_EventType> MailEvent;

        public static void TriggerEvent(object? sender,SMTP_EventType e)
        {
            if (Enable)
            {
                MailEvent?.Invoke(sender, e);
            }
        }

        private static void SMTP_MailEvent(object? sender, SMTP_EventType e)
        {
            var (subject, body) = e switch
            {
                SMTP_EventType.LoginFailureReminder => (
                    "登陆失效",
                    "DDTV的登陆状态可能已失效，如果稳定出现该提示邮件，请确保登录态有效或者网络连接稳定"
                ),
                SMTP_EventType.StartLive when sender is RoomCardClass room => (
                    "开播提醒",
                    $"你设置了开播提醒的主播【{room.Name}({room.RoomId})）】开始主直播啦！"
                ),
                SMTP_EventType.RecEnd when sender is RoomCardClass room => (
                    "录制结束提示",
                    $"你设置的录制直播间【{room.Name}({room.RoomId})）】录制任务结束"
                ),
                SMTP_EventType.TranscodingFail when sender is RoomCardClass room => (
                    "修复或转码失败",
                    $"直播间【{room.Name}({room.RoomId})）】的录制后转码或修复任务失败！"
                ),
                _ => (string.Empty, string.Empty)
            };

            if (!string.IsNullOrEmpty(subject))
            {
                Send(subject, body);
            }
        }


        public static void Init()
        {
            if (!Enable)
            {
                Enable = true;
                MailEvent += SMTP_MailEvent;
            }
        }

       


        /// <summary>
        /// 使用QQ邮箱SMTP发送邮件
        /// </summary>
        /// <param name="subject">邮件主题</param>
        /// <param name="body">邮件正文</param>
        /// <returns>是否发送成功</returns>
        public static void Send(string subject,string body)
        {
            if(Config.Core_RunConfig._Email_EnableSmtp)
            {
                _SendEmail(Config.Core_RunConfig._Email_SmtpTo, subject, body, Config.Core_RunConfig._Email_SmtpFrom, Config.Core_RunConfig._Email_SmtpUserName, Config.Core_RunConfig._Email_SmtpPassword);
            }
        }

        /// <summary>
        /// 使用QQ邮箱SMTP发送邮件
        /// </summary>
        /// <param name="toEmail">收件人邮箱地址</param>
        /// <param name="subject">邮件主题</param>
        /// <param name="body">邮件正文</param>
        /// <param name="fromEmail">发件人邮箱地址(QQ邮箱)</param>
        /// <param name="fromName">发件人显示名称</param>
        /// <param name="smtpPassword">密码</param>
        /// <returns>是否发送成功</returns>
        public static bool _SendEmail(string toEmail, string subject, string body, string fromEmail, string fromName, string smtpPassword)
        {
            try
            {
                string b = MAIL_STR.Replace("${Event}",subject).Replace("${Msg}",body);
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = b,
                    IsBodyHtml = true,
                    Priority = MailPriority.Normal
                };

                mailMessage.To.Add(toEmail);

                var smtpClient = new SmtpClient(Config.Core_RunConfig._Email_SmtpServer, int.Parse(Config.Core_RunConfig._Email_SmtpPort))
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromEmail, smtpPassword),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(nameof(_SendEmail),$"发送邮件失败: {ex.Message}");
                return false;
            }
        }
        public enum SMTP_EventType
        {
            /// <summary>
            /// 登陆失效
            /// </summary>
            LoginFailureReminder,
            /// <summary>
            /// 开始直播
            /// </summary>
            StartLive,
            /// <summary>
            /// 录制结束
            /// </summary>
            RecEnd,
            /// <summary>
            /// 转码失败
            /// </summary>
            TranscodingFail
        }
    }


}
