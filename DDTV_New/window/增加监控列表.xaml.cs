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
    public partial class 增加监控列表 : Window
    {
        public 增加监控列表(int 所选页=0)
        {
            InitializeComponent();
            选项卡.SelectedIndex = 所选页;
            监控非VTB直播间使能按钮.IsChecked = 是否启动WSS连接组;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(名称.Text)||string.IsNullOrEmpty(唯一码.Text))
            {
                提示.Content = "名称或唯一码不能为空！";
            }
            else
            {
                int roomId = 0;
                try
                {
                    roomId = int.Parse(唯一码.Text);

                    string roomDD = bilibili.根据房间号获取房间信息.获取真实房间号(roomId.ToString());
                    if (!string.IsNullOrEmpty(roomDD))
                    {
                        roomId = int.Parse(roomDD);
                    }
                }
                catch (Exception)
                {

                    MessageBox.Show("输入的直播间房间号不符合房间号规则(数字)");
                    return;
                }
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
                long UID = 0;
                if(DataCache.读缓存(DataCache.缓存头.通过房间号获取UID + roomId, 0, out string GETUID))
                {
                    try
                    {
                        UID = long.Parse(GETUID);
                    }
                    catch (Exception){}
                }
                if(UID<1)
                {
                    try
                    {
                        UID = long.Parse(bilibili.根据房间号获取房间信息.通过房间号获取UID(roomId.ToString()));
                    }
                    catch (Exception){}
                }
               
                RB.data.Add(new RoomCadr() { Name = 名称.Text , OfficialName = 名称.Text , RoomNumber = roomId.ToString(),UID= UID });
                string JOO = JsonConvert.SerializeObject(RB);
                MMPU.储存文本(JOO, RoomConfigFile);
                提示.Content = 名称.Text + "["+ 唯一码.Text + "]添加完成";
                bilibili.已连接的直播间状态.Add(new 直播间状态() { 房间号= roomId });
                
                bilibili.RoomList.Add(new RoomInfo
                {
                    房间号 = roomId.ToString(),
                    标题 = "",
                    是否录制弹幕 = false,
                    是否录制视频 = false,
                    UID = UID.ToString(),
                    直播开始时间 = "",
                    名称 = 名称.Text ,
                    直播状态 = false,
                    原名 = 名称.Text ,
                    是否提醒 = false,
                    平台 = "bilibili"
                });
                名称.Text = "";
                唯一码.Text = "";
            }                
        }

        private void 监控非VTB直播间使能按钮开关点击事件(object sender, RoutedEventArgs e)
        {
            if (监控非VTB直播间使能按钮.IsChecked == true)
            {
                //NewThreadTask.Run(() =>
                //{
                //    持续连接获取阿B房间信息类.初始化所有房间连接();
                //});

                bilibili.是否启动WSS连接组 = true;
                MMPU.setFiles("NotVTBStatus", "1");
            }
            else
            {
                bilibili.是否启动WSS连接组 = false;
                MMPU.setFiles("NotVTBStatus", "0");
            }
            提示.Content = "监控非VTB直播间状态使能发生变化，需手动重启DDTV生效";
            MessageBox.Show("!!重要功能更改!!\r需手动重启DDTV生效，重要配置变化，不重启可能会出现未知错误");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("配置文件发生变化，请手动重启DDTV以加载新的配置");
            this.Close();
        }

        private void 一键导入账号关注VTB和VUP_Click(object sender, RoutedEventArgs e)
        {
            增加房间提示信息.Content = $"正在导入，请勿关闭该窗口，请稍后";
            AddList.导入VTBVUP((TEXT) =>
            {
                try
                {
                    增加房间提示信息.Content = "导入完成,新增:"+TEXT.Split('：')[TEXT.Split('：').Length - 1]+"个";
                }
                catch (Exception)
                {
                    增加房间提示信息.Content = "导入完成";
                }
               
            },this,false);
        }
    }
}
