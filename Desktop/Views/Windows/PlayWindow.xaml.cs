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

namespace Desktop.Views.Windows
{
    /// <summary>
    /// PlayWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PlayWindow : Window
    {
        long _room_id = 0;
        public PlayWindow(long room_id)
        {
            _room_id = room_id;
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.WV2.Dispose();
                this.WV2 = null;
            });
        }

        private async void WV2_Loaded(object sender, RoutedEventArgs e)
        {
            await WV2.EnsureCoreWebView2Async(null);
            string C = NetWork.Get.GetBody<string>("http://127.0.0.1:11419/api/system/get_c").Replace(" ", "");
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
            string B = test.UC;
            WV2.CoreWebView2.Navigate($"{B}{_room_id}&fullscreen=0&send=1&recommend=0");
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
