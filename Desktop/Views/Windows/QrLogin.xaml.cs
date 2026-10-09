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
        public QrLogin()
        {
            InitializeComponent();
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            if (await NetWork.Post.PostBody<bool>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/login/re_login"))
            {
                Log.Info(nameof(QrLogin), "调用Core的API[re_login]成功");
                try
                {
                    string URL = string.Empty;
                    do
                    {
                        if (Core.Config.Core_RunConfig._DesktopRemoteServer || Core.Config.Core_RunConfig._LocalHTTPMode)
                        {
                            URL = await NetWork.Get.GetBodyAsync<string>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/login/get_login_url");
                        }
                        else
                        {
                            URL = await Core.RuntimeObject.Login.get_login_urlAsync();
                        }

                        if (string.IsNullOrEmpty(URL))
                        {
                            Log.Warn(nameof(QrLogin), "调用Core的API[get_login_url]失败，获取到的信息为空，请检查Core日志");
                            await Task.Delay(500);
                        }
                        else
                            Log.Info(nameof(QrLogin), "调用Core的API[get_login_url]成功");
                    } while (string.IsNullOrEmpty(URL));

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
                        Log.Info(nameof(QrLogin), "更新QR码UI内容");
                    }

                    bool loggedIn = false;
                    do
                    {
                        if (!this.IsLoaded)
                        {
                            break;
                        }
                        await Task.Delay(500);
                        loggedIn = (Core.Config.Core_RunConfig._DesktopRemoteServer || Core.Config.Core_RunConfig._LocalHTTPMode)
                            ? await NetWork.Post.PostBody<bool>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/login/get_login_status")
                            : Core.RuntimeObject.Account.GetLoginStatus();
                    } while (!loggedIn);

                    if (loggedIn)
                    {
                        Desktop.Views.Pages.DefaultPage.SetWarningMessage("");
                        Log.Info(nameof(QrLogin), "登陆完成，关闭QR扫码窗口");
                        DataSource.LoginStatus.LoginWindowDisplayStatus = false;
                        this.Close();
                    }
                }
            catch (Exception ex)
            {
                Log.Error(nameof(QrLogin), "扫码登陆出现意外重大错误，错误堆栈请查看txt文本内容", ex);
                System.Windows.MessageBox.Show("登录初始化失败，请检查网络连接或服务器状态。", "网络请求失败", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
            }
            else
            {
                Log.Warn(nameof(QrLogin), "调用Core的API[re_login]失败");
                System.Windows.MessageBox.Show("登录初始化失败，请检查网络连接或服务器状态。", "网络请求失败", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataSource.LoginStatus.LoginWindowDisplayStatus = false;
        }
    }
}
