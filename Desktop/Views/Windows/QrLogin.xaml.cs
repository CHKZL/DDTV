using Desktop.Models;
using Net.Codecrete.QrCodeGenerator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
using static Desktop.Views.Pages.DataPage;
using static System.Net.Mime.MediaTypeNames;

namespace Desktop.Views.Windows
{
    /// <summary>
    /// QrLogin.xaml 的交互逻辑
    /// </summary>
    public partial class QrLogin : FluentWindow
    {
        bool _Close = false;
        public QrLogin()
        {
            InitializeComponent();
            if (NetWork.Post.PostBody<bool>("http://127.0.0.1:11419/api/login/re_login"))
            {
                try
                {
                    string URL = NetWork.Get.GetBody<string>("http://127.0.0.1:11419/api/login/get_login_url");
                    var qr = QrCode.EncodeText(URL, QrCode.Ecc.Quartile);
                    byte[] bmpBytes = qr.ToBmpBitmap(5, 10);
                    using (var ms = new MemoryStream(bmpBytes))
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = ms;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        Loading.Visibility = Visibility.Collapsed;
                        QR.Source = bitmapImage;
                    }
                    CancellationTokenSource cts = new CancellationTokenSource();
                    Task.Run(() =>
                    {
                        do
                        {
                            Dispatcher.Invoke(() =>
                            {
                                if (!this.IsLoaded)
                                {
                                    cts.Cancel();
                                }
                            });
                            if (cts.Token.IsCancellationRequested)
                            {
                                break;
                            }
                            Thread.Sleep(100);
                        } while (!NetWork.Post.PostBody<bool>("http://127.0.0.1:11419/api/login/get_login_status"));
                        Dispatcher.Invoke(() =>
                        {
                            DataSource.LoginStatus.LoginWindowDisplayStatus = false;
                            this.Close();
                        });
                    }, cts.Token);
                }
                catch (Exception)
                {
                }
            }
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataSource.LoginStatus.LoginWindowDisplayStatus = false;
        }
    }
}
