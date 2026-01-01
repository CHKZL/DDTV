using Masuit.Tools.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MailKit.Net.Smtp;
using Core.LogModule;
using MailKit.Security;
using MimeKit;
using System.Text;
using System.Threading.Tasks;

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

        public static void TriggerEvent(object? sender, SMTP_EventType e)
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
                SMTP_EventType.LoginFailureReminder when Config.Core_RunConfig._Email_LoginFailureReminder_Enable => (
                    "登陆失效",
                    "DDTV的登陆状态可能已失效，如果稳定出现该提示邮件，请确保登录态有效或者网络连接稳定"
                ),
                SMTP_EventType.StartLive when sender is RoomCardClass room && Config.Core_RunConfig._Email_StartLive_Enable => (
                    "开播提醒",
                    $"你设置了开播提醒的主播【{room.Name}({room.RoomId})）】开始主直播啦！"
                ),
                SMTP_EventType.RecEnd when sender is RoomCardClass room && Config.Core_RunConfig._Email_RecEnd_Enable=> (
                    "录制结束提示",
                    $"你设置的录制直播间【{room.Name}({room.RoomId})）】录制任务结束"
                ),
                SMTP_EventType.TranscodingFail when sender is (RoomCardClass room,string ErrorStr)  && Config.Core_RunConfig._Email_TranscodingFail_Enable=> (
                    "修复或转码失败",
                    $"直播间【{room.Name}({room.RoomId})）】的录制后转码或修复任务失败！，详细错误：\r\n{ErrorStr}"
                ),
                SMTP_EventType.AbandonTranscod when sender is RoomCardClass room && Config.Core_RunConfig._Email_TranscodingFail_Enable=> (
                    "放弃转码",
                    $"直播间【{room.Name}({room.RoomId})）】的录制后转码或修复的输出文件小于预期，放弃删除源文件！"
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
        public static void Send(string subject, string body)
        {
            if (!Config.Core_RunConfig._Email_EnableSmtp)
                return;
            else if (Config.Core_RunConfig._Email_EnableSmtp)
            {
                var toEmail = Config.Core_RunConfig._Email_SmtpTo;
                var fromEmail = Config.Core_RunConfig._Email_SmtpFrom;
                var fromName = Config.Core_RunConfig._Email_SmtpFromName;
                var smtpUser = Config.Core_RunConfig._Email_SmtpUserName;
                var smtpPassword = Config.Core_RunConfig._Email_SmtpPassword;
                var smtpHost = Config.Core_RunConfig._Email_SmtpServer;
                var smtpPort = int.Parse(Config.Core_RunConfig._Email_SmtpPort);
                var smtpSecurity = Config.Core_RunConfig._Email_SmtpSecurity;
                Task.Run(() => _SendEmail(toEmail,
                                          subject,
                                          body,
                                          fromEmail,
                                          fromName,
                                          smtpHost,
                                          smtpPort,
                                          smtpSecurity,
                                          smtpUser,
                                          smtpPassword));
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
        /// <param name="smtpHost">smtp服务器</param>
        /// <param name="smtpPort">smtp端口</param>
        /// <param name="smtpSecurity">加密选项</param>
        /// <param name="smtpUser">smtp用户名</param>
        /// <param name="smtpPassword">密码</param>
        public static void _SendEmail(string toEmail, string subject, string body, string fromEmail, string fromName, string smtpHost, int smtpPort, string smtpSecurity, string smtpUser, string smtpPassword)
        {

            try
            {
                string b = MAIL_STR.Replace("${Event}", subject).Replace("${Msg}", body);
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress(fromName, fromEmail));
                mailMessage.To.Add(MailboxAddress.Parse(toEmail));
                mailMessage.Subject = subject;
                mailMessage.Body = new BodyBuilder { HtmlBody = b }.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                var options = ParseSecureSocketOptions(smtpSecurity);
                client.Connect(smtpHost, smtpPort, options);
                client.Authenticate(smtpUser, smtpPassword);
                client.Send(mailMessage);
                client.Disconnect(true);

                Log.Info(nameof(_SendEmail), $"发送邮件成功: {subject}");

            }
            catch (Exception ex)
            {
                Log.Warn(nameof(_SendEmail), $"发送邮件失败: {ex.Message}");

            }

        }

        private static SecureSocketOptions ParseSecureSocketOptions(string? value)
        {
            /// <summary>
            /// 加密选项
            /// SMTP 连接安全模式（字符串）
            /// 可选值：auto / none / tls / starttls / opportunistic
            /// - none：不加密
            /// - tls：连接即 TLS（常见于 465，但你可自由组合端口）
            /// - starttls：先明文连接再升级 TLS（常见于 587，但你可自由组合端口）
            /// - opportunistic：服务器支持则升级，否则明文
            /// - auto：让 MailKit 自动判断（仍允许端口自由组合）
            /// 默认值：auto
            /// </summary>
            if (string.IsNullOrWhiteSpace(value))
                return SecureSocketOptions.Auto;

            switch (value.Trim().ToLowerInvariant())
            {
                case "none":
                    return SecureSocketOptions.None;

                case "tls":
                    return SecureSocketOptions.SslOnConnect;

                case "starttls":
                    return SecureSocketOptions.StartTls;

                case "opportunistic":
                    return SecureSocketOptions.StartTlsWhenAvailable;

                case "auto":
                default:
                    return SecureSocketOptions.Auto;
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
            TranscodingFail,
            /// <summary>
            /// 放弃转码
            /// </summary>
            AbandonTranscod
        }
    }


}
