using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.ConfigModule;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace DDTV_GUI.DDTV_Window
{
    /// <summary>
    /// InitialBoot.xaml 的交互逻辑
    /// </summary>
    public partial class InitialBoot : GlowWindow
    {
        int page=0;
        public event EventHandler<EventArgs> LoginDialogDispose;
        WPFControl.LoginQRDialog LoginQRDialog;
        public InitialBoot()
        {
            InitializeComponent();
            T0.Visibility = Visibility.Visible;
            ProgressAssembly.StepIndex = 0;
            T1.Visibility = Visibility.Collapsed;
            T2.Visibility = Visibility.Collapsed;
            T3.Visibility = Visibility.Collapsed;
        }
        public void SetT(int i)
        {
            ProgressAssembly.StepIndex = i;
            switch (i)
            {
                case 0:
                    T0.Visibility = Visibility.Visible;
                    T1.Visibility = Visibility.Collapsed;
                    T2.Visibility = Visibility.Collapsed;
                    T3.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    NextStep.IsEnabled = false;
                    T0.Visibility = Visibility.Collapsed;
                    T1.Visibility = Visibility.Visible;
                    T2.Visibility = Visibility.Collapsed;
                    T3.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    NextStep.IsEnabled = false;
                    T0.Visibility = Visibility.Collapsed;
                    T1.Visibility = Visibility.Collapsed;
                    T2.Visibility = Visibility.Visible;
                    T3.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    NextStep.Visibility = Visibility.Collapsed;
                    T0.Visibility = Visibility.Collapsed;
                    T1.Visibility = Visibility.Collapsed;
                    T2.Visibility = Visibility.Collapsed;
                    T3.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void PreviousStep_Click(object sender, RoutedEventArgs e)
        {
            page--;
            SetT(page);
        }

        private void NextStep_Click(object sender, RoutedEventArgs e)
        {
            page++;
            SetT(page);
        }

        private void GlowWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private Dialog DG = null;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoginDialogDispose += InitialBoot_LoginDialogDispose;
            LoginQRDialog = new WPFControl.LoginQRDialog(LoginDialogDispose, "使用哔哩哔哩手机客户端扫码登陆");
            DG = Dialog.Show(LoginQRDialog);
        }

        private void InitialBoot_LoginDialogDispose(object? sender, EventArgs e)
        {
            DG.Dispatcher.BeginInvoke(new Action(() => DG.Close()));
            NextStep.Dispatcher.BeginInvoke(new Action(() => NextStep.IsEnabled = true));
            DRButton.Dispatcher.BeginInvoke(new Action(() => {
                DRButton.Content = "已登录";
                DRButton.IsEnabled= false;
            }));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Rooms.UpdateRoomInfo();
            DDTV_Core.SystemAssembly.RoomPatrolModule.RoomPatrol.Init();
            CoreConfig.SetValue(CoreConfigClass.Key.FirstStart, "false", CoreConfigClass.Group.Core);
            this.Close();
        }

        private void AddRoomButton_Click(object sender, RoutedEventArgs e)
        {
            int AddConut = DDTV_Core.SystemAssembly.BilibiliModule.API.UserInfo.follow(long.Parse(BilibiliUserConfig.account.uid));
            AddRoomButton.Content = $"已导入{AddConut}个";
            AddRoomButton.IsEnabled= false;
            Growl.Success($"成功导入{AddConut}个关注列表中的V到配置");
            NextStep.IsEnabled = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            AddRoomButton.Content = $"已跳过";
            AddRoomButton.IsEnabled = false;
            NextStep.IsEnabled = true;
        }
    }
}
