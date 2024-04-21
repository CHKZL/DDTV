using Core.RuntimeObject;
using System;
using System.Collections.Generic;
using System.Linq;
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
        string url = "https://www.bilibili.com/blackboard/live/live-activity-player.html?cid=";
        long _room_id = 0;
        public PlayWindow(long room_id)
        {
            _room_id= room_id;
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

        private async  void WV2_Loaded(object sender, RoutedEventArgs e)
        {
            await WV2.EnsureCoreWebView2Async(null);
            string C = NetWork.Get.GetBody<string>("http://127.0.0.1:11419/api/system/get_c");
            foreach (var item in C.Split(';'))
            {
                if (item != null && item.Split('=').Length == 2)
                {
                    var cookie = WV2.CoreWebView2.CookieManager.CreateCookie(item.Split('=')[0], item.Split('=')[1], url.Substring(12, 12), "/");
                    WV2.CoreWebView2.CookieManager.AddOrUpdateCookie(cookie);
                }
            }
            WV2.CoreWebView2.Navigate($"url{_room_id}&fullscreen=0&send=1&recommend=0");
        }
    }
}
