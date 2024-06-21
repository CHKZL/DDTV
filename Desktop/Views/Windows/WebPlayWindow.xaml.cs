using Core.LogModule;
using Core.RuntimeObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace Desktop.Views.Windows
{
    /// <summary>
    /// PlayWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WebPlayWindow : FluentWindow
    {
        long _room_id = 0;
        long _uid = 0;
        string _nickname = string.Empty;
        //RoomCardClass _roomCard;
        public WebPlayWindow(long room_id)
        {
            _room_id = room_id;
            InitializeComponent();
            Task.Run(() =>
            {
                _uid = RoomInfo.GetUid(_room_id);
               
                _nickname = RoomInfo.GetNickname(_uid);
                Dispatcher.Invoke(() =>
                {
                     this.Title = RoomInfo.GetTitle(_uid);
                    UI_TitleBar.Title = $"{_nickname}({_room_id}) - {this.Title}(该直播间只有FLV流，使用WEB兼容模式播放)";
                });
            });



            Log.Info(nameof(WebPlayWindow), $"房间号:[{room_id}],打开播放器");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.WV2.Dispose();
                this.WV2 = null;
            });
            Log.Info(nameof(WebPlayWindow), $"房间号:[{_room_id}],关闭播放器");
        }

        private async void WV2_Loaded(object sender, RoutedEventArgs e)
        {
            await WV2.EnsureCoreWebView2Async(null);
            try
            {
                string C = NetWork.Get.GetBody<string>("http://127.0.0.1:11419/api/system/get_c").Replace(" ", "");
                if (string.IsNullOrEmpty(C))
                {
                    C = NetWork.Get.GetBody<string>("http://127.0.0.1:11419/api/system/get_c").Replace(" ", "");
                }
                foreach (var item in C.Split(';'))
                {
                    if (item != null && item.Split('=').Length == 2)
                    {
                        string name = item.Split('=')[0];
                        string value = item.Split('=')[1];
                        string D = ".bilibili.com";

                        var cookie = WV2.CoreWebView2.CookieManager.CreateCookie(name, value, D, "/");
                        WV2.CoreWebView2.CookieManager.AddOrUpdateCookie(cookie);
                    }
                }
                string uc = test.UC;
                WV2.CoreWebView2.Navigate($"{uc}{_room_id}&send=0&recommend=0&fullscreen=0");
            }
            catch (Exception EX)
            {
                Log.Error(nameof(WebPlayWindow), $"房间号:[{_room_id}],打开错误", EX, true);
            }
        }
        internal class test
        {
            public static string UC
            {
                get
                {
                    string t = string.Empty;
                    t += (char)104;
                    t += (char)116;
                    t += (char)116;
                    t += (char)112;
                    t += (char)115;
                    t += (char)58;
                    t += (char)47;
                    t += (char)47;
                    t += (char)119;
                    t += (char)119;
                    t += (char)119;
                    t += (char)46;
                    t += (char)98;
                    t += (char)105;
                    t += (char)108;
                    t += (char)105;
                    t += (char)98;
                    t += (char)105;
                    t += (char)108;
                    t += (char)105;
                    t += (char)46;
                    t += (char)99;
                    t += (char)111;
                    t += (char)109;
                    t += (char)47;
                    t += (char)98;
                    t += (char)108;
                    t += (char)97;
                    t += (char)99;
                    t += (char)107;
                    t += (char)98;
                    t += (char)111;
                    t += (char)97;
                    t += (char)114;
                    t += (char)100;
                    t += (char)47;
                    t += (char)108;
                    t += (char)105;
                    t += (char)118;
                    t += (char)101;
                    t += (char)47;
                    t += (char)108;
                    t += (char)105;
                    t += (char)118;
                    t += (char)101;
                    t += (char)45;
                    t += (char)97;
                    t += (char)99;
                    t += (char)116;
                    t += (char)105;
                    t += (char)118;
                    t += (char)105;
                    t += (char)116;
                    t += (char)121;
                    t += (char)45;
                    t += (char)112;
                    t += (char)108;
                    t += (char)97;
                    t += (char)121;
                    t += (char)101;
                    t += (char)114;
                    t += (char)46;
                    t += (char)104;
                    t += (char)116;
                    t += (char)109;
                    t += (char)108;
                    t += (char)63;
                    t += (char)99;
                    t += (char)105;
                    t += (char)100;
                    t += (char)61;
                    return t;
                }
            }
        }
    }
}