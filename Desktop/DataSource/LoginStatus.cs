using Core;
using Core.LogModule;
using Core.RuntimeObject;
using Desktop.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;
using static Core.Network.Methods.Room;

namespace Desktop.DataSource
{
    internal class LoginStatus
    {
        /// <summary>
        /// 登陆失效事件
        /// </summary>
        public static event EventHandler<EventArgs> LoginFailureEvent;
        /// <summary>
        /// 登陆窗展示状态
        /// </summary>
        public static bool LoginWindowDisplayStatus = false;
        public static Timer Timer_LoginStatus;
        public static void RefreshLoginStatus(object state)
        {
            Task.Run(() =>
            {
                if (!NetWork.Post.PostBody<bool>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/login/get_login_status").Result)
                {
                    Thread.Sleep(1000);
                    if (!NetWork.Post.PostBody<bool>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/login/get_login_status").Result)
                    {
                        LoginFailureEvent?.Invoke(null, new EventArgs());
                    }
                }
            });
        }
    }
}
