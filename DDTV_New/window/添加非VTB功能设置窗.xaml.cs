using Auxiliary;
using DDTV_New.Utility;
using Newtonsoft.Json;
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
using static Auxiliary.bilibili;
using static Auxiliary.RoomInit;

namespace DDTV_New.window
{
    /// <summary>
    /// 添加非VTB功能设置窗.xaml 的交互逻辑
    /// </summary>
    public partial class 添加非VTB功能设置窗 : Window
    {
        public 添加非VTB功能设置窗()
        {
            InitializeComponent();
            监控非VTB直播间使能按钮.IsChecked = 是否启动WS连接组;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(名称.Text)||string.IsNullOrEmpty(唯一码.Text))
            {
                提示.Content = "名称或唯一码不能为空！";
            }
            else
            {
                try
                {
                    long roomId = long.Parse(唯一码.Text);
                }
                catch (Exception){}
                RoomBox rlc = JsonConvert.DeserializeObject<RoomBox>(ReadConfigFile(RoomConfigFile));
                RoomBox RB = new RoomBox
                {
                    data = new List<RoomCadr>()
                };
                if (rlc.data != null)
                {
                    foreach (var item in rlc.data)
                    {
                        if(item.RoomNumber==唯一码.Text)
                        {
                            提示.Content = "配置文件中已有改房间号存在!";
                            return;
                        }
                        RB.data.Add(item);
                    }
                }
                RB.data.Add(new RoomCadr() { Name = 名称.Text + "-NV", OfficialName = 名称.Text + "-NV", RoomNumber = 唯一码.Text });
                string JOO = JsonConvert.SerializeObject(RB);
                MMPU.储存文本(JOO, RoomConfigFile);
                提示.Content = 名称.Text + "-NV["+ 唯一码.Text + "]添加完成";
                名称.Text = "";
                唯一码.Text = "";
            }                
        }

        private void 监控非VTB直播间使能按钮开关点击事件(object sender, RoutedEventArgs e)
        {
            if (监控非VTB直播间使能按钮.IsChecked == true)
            {
                NewThreadTask.Run(() =>
                {
                    持续连接获取阿B房间信息类.初始化所有房间连接();
                });

                bilibili.是否启动WS连接组 = true;
                MMPU.setFiles("NotVTBStatus", "1");
            }
            else
            {
                bilibili.是否启动WS连接组 = false;
                MMPU.setFiles("NotVTBStatus", "0");
            }
            提示.Content = "监控非VTB直播间状态使能发生变化，需手动重启DDTV生效";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("配置文件发生变化，请手动重启DDTV以加载新的配置");
            this.Close();
        }
    }
}
