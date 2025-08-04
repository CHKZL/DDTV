using Core;
using Core.LogModule;
using Net.Codecrete.QrCodeGenerator;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;
using static Core.Tools.SystemResource.Overview;
using static System.Windows.Forms.AxHost;

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
            Task.Run(() =>
            {
                if (NetWork.Post.PostBody<bool>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/login/re_login").Result)
                {
                    Log.Info(nameof(QrLogin), "调用Core的API[re_login]成功");
                    try
                    {
                        Task.Run(() =>
                        {
                            string URL = string.Empty;
                            do
                            {
                                if (Core.Config.Core_RunConfig._DesktopRemoteServer || Core.Config.Core_RunConfig._LocalHTTPMode)
                                {
                                    URL = NetWork.Get.GetBody<string>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/login/get_login_url");
                                }
                                else
                                {
                                    URL = Core.RuntimeObject.Login.get_login_urlAsync().Result;
                                }           

                                if (string.IsNullOrEmpty(URL))
                                {
                                    Log.Warn(nameof(QrLogin), "调用Core的API[get_login_url]失败，获取到的信息为空，请检查Core日志");
                                    Thread.Sleep(500);
                                }
                                else
                                    Log.Info(nameof(QrLogin), "调用Core的API[get_login_url]成功");
                            } while (string.IsNullOrEmpty(URL));

                            var qr = QrCode.EncodeText(URL, QrCode.Ecc.Quartile);
                            byte[] bmpBytes = qr.ToBmpBitmap(5, 10);
                            using (var ms = new MemoryStream(bmpBytes))
                            {
                                Dispatcher.Invoke(() =>
                               {
                                   var bitmapImage = new BitmapImage();
                                   bitmapImage.BeginInit();
                                   bitmapImage.StreamSource = ms;
                                   bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                   bitmapImage.EndInit();
                                   Loading.Visibility = Visibility.Collapsed;
                                   QR.Source = bitmapImage;
                                   Log.Info(nameof(QrLogin), "更新QR码UI内容");
                               });

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
                                    Thread.Sleep(500);
                                } while (
                                (Core.Config.Core_RunConfig._DesktopRemoteServer || Core.Config.Core_RunConfig._LocalHTTPMode)
                                ?
                                !NetWork.Post.PostBody<bool>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/login/get_login_status").Result
                                :
                                !Core.RuntimeObject.Account.GetLoginStatus()
                                );


                                Dispatcher.Invoke(() =>
                                {
                                    Desktop.Views.Pages.DefaultPage.SetWarningMessage("");
                                    Log.Info(nameof(QrLogin), "登陆完成，关闭QR扫码窗口");
                                    DataSource.LoginStatus.LoginWindowDisplayStatus = false;
                                    this.Close();
                                });
                            }, cts.Token);
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.Error(nameof(QrLogin), "扫码登陆出现意外重大错误，错误堆栈请查看txt文本内容", ex);
                    }
                }
                else
                {
                    Log.Warn(nameof(QrLogin), "调用Core的API[re_login]失败");
                }
            });
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataSource.LoginStatus.LoginWindowDisplayStatus = false;
        }
    }
}
