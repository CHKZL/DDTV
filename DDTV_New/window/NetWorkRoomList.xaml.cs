using Auxiliary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DDTV_New.window
{
    /// <summary>
    /// NetRoomList.xaml 的交互逻辑
    /// </summary>
    public partial class NetRoomList : Window
    {
       
        public MMPU.加载网络房间方法.选中的网络房间 选中内容 = new MMPU.加载网络房间方法.选中的网络房间();
       
        public NetRoomList()
        {
            InitializeComponent();
            //更新网络房间列表.IsEnabled = false;
            //选中内容展示.Content = "更新中";
          
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            更新网络房间列表.IsEnabled = false;
            选中内容展示.Content = "更新中";
            new Task((() => 
            {
                if (MMPU.加载网络房间方法.列表缓存.Count < 1)
                {
                    MessageBox.Show("网络缓存同步未完成，请稍后再试");
                    MMPU.加载网络房间方法.更新网络房间缓存();
                    return;
                }
                this.Dispatcher.Invoke(new Action(delegate
                {
                    NetWorkRoomList.Items.Clear();
                    foreach (var item in MMPU.加载网络房间方法.列表缓存)
                    {
                        NetWorkRoomList.Items.Add(new
                        {
                            编号 = item.编号,
                            名称 = item.名称,
                            官方名称 = item.官方名称,
                            平台 = item.平台,
                            账号UID = item.UID,
                            类型 = item.类型
                        });
                    }
                    选中内容展示.Content = "";
                    更新网络房间列表.IsEnabled = true;
                }));
            })).Start();

        }
     
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (选中内容.UID == null)
            {
                MessageBox.Show("还未选择内容");
                return;
            }
            string 房间号 = Auxiliary.bilibili.通过UID获取房间号(选中内容.UID);
            foreach (var item in Auxiliary.bilibili.RoomList)
            {
                if (item.房间号 == 房间号)
                {
                    MessageBox.Show("该房间本地已经存在于监控列表");
                    return;
                }
            }
            AddMonitoringList AML = new AddMonitoringList("从网络添加房间", 选中内容.名称, 选中内容.官方名称, 选中内容.平台, 房间号, false);
            AML.ShowDialog();
            选中内容 = new MMPU.加载网络房间方法.选中的网络房间();
            选中内容展示.Content = "";
        }

        private void 网络房间数据列表单击事件(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                System.Windows.Controls.ListView LV = (System.Windows.Controls.ListView)sender;
                if (LV.Items.Count != 0)
                {
                    选中内容.编号 = int.Parse(LV.SelectedItems[0].ToString().Split(',')[0].Split('=')[1]);
                    选中内容.名称 = LV.SelectedItems[0].ToString().Split(',')[1].Split('=')[1].Replace("\"", "").Replace(" ", "").Length == 0 ? " " : LV.SelectedItems[0].ToString().Split(',')[1].Split('=')[1].Replace("\"", "").Replace(" ", "");
                    选中内容.官方名称 = LV.SelectedItems[0].ToString().Split(',')[2].Split('=')[1].Replace("\"", "").Replace(" ", "");
                    选中内容.平台 = LV.SelectedItems[0].ToString().Split(',')[3].Split('=')[1].Replace("\"", "").Replace(" ", "");
                    选中内容.UID = LV.SelectedItems[0].ToString().Split(',')[4].Split('=')[1].Replace("\"", "").Replace(" ", "");
                    选中内容展示.Content = "选中内容:" + 选中内容.名称 + "/" + 选中内容.官方名称;
                }
               
            }
            catch (Exception)
            {
            }
        }

    


        private void 搜索按钮_Click(object sender, RoutedEventArgs e)
        {
            List<MMPU.加载网络房间方法.列表加载缓存> 搜索缓存 = new List<MMPU.加载网络房间方法.列表加载缓存>();
            string 搜索内容 = 搜索栏.Text;
            if (!string.IsNullOrEmpty(搜索内容))
            {
                int B = 1;
                foreach (var item in MMPU.加载网络房间方法.列表缓存)
                {
                    if (item.名称.Contains(搜索内容) || item.官方名称.Contains(搜索内容) || item.UID.Contains(搜索内容))
                    {
                        搜索缓存.Add(new MMPU.加载网络房间方法.列表加载缓存
                        {
                            编号 = B,
                            名称 = item.名称,
                            官方名称 = item.官方名称,
                            平台 = item.平台,
                            UID = item.UID,
                            类型 = item.类型
                        });
                        B++;
                    }
                }
            }
            else
            {
                MessageBox.Show("请输入搜索内容");
                return;
            }
            new Task((() => 
            {
                if (搜索缓存.Count < 1)
                {
                    MessageBox.Show("云端数据中没有满足条件的数据");
                    return;
                }
                this.Dispatcher.Invoke(new Action(delegate
                {
                    NetWorkRoomList.Items.Clear();
                    foreach (var item in 搜索缓存)
                    {
                        NetWorkRoomList.Items.Add(new
                        {
                            编号 = item.编号,
                            名称 = item.名称,
                            官方名称 = item.官方名称,
                            平台 = item.平台,
                            账号UID = item.UID,
                            类型 = item.类型
                        });
                    }
                    选中内容展示.Content = "";
                    更新网络房间列表.IsEnabled = true;
                }));
            })).Start();
        }
    }
}
