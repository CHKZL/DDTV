using Auxiliary;
using DDTV_New.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Auxiliary.RoomInit;

namespace DDTV_New.window
{
    /// <summary>
    /// Interaction logic for FirstTimeSetupWindow.xaml
    /// </summary>
    public partial class FirstTimeSetupWindow : Window
    {
        public FirstTimeSetupWindow()
        {
            InitializeComponent();

            WindowStyle = WindowStyle.None;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            _grids = new List<Grid>
            {
                欢迎使用层,
                设置数据源层,
                登录bilibili层,
                导入VTBVUP数据层,
                完成层
            };

            切换界面(欢迎使用层);
        }

        private List<Grid> _grids;
        private int _数据源 = 0;
        private bool _已导入 = false;

        public void 切换界面(Grid grid)
        {
            for (int i = 0; i < _grids.Count; i++)
            {
                if (_grids[i] == grid)
                {
                    _grids[i].Visibility = Visibility.Visible;

                    if (i == 0) 上一步按钮.Visibility = Visibility.Hidden;
                    else 上一步按钮.Visibility = Visibility.Visible;
                }
                else
                {
                    _grids[i].Visibility = Visibility.Collapsed;
                }
            }
        }

        private void 上一步按钮_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < _grids.Count; i++)
            {
                if (_grids[i].Visibility == Visibility.Visible)
                {
                    if (i == 0) return;

                    切换界面(_grids[i - 1]);
                    return;
                }
            }
        }

        private void 下一步按钮_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < _grids.Count; i++)
            {
                if (_grids[i].Visibility == Visibility.Visible)
                {
                    if (i == _grids.Count - 1)
                    {
                        完成初始化();
                        return;
                    }

                    切换界面(_grids[i + 1]);
                    return;
                }
            }
        }

        private void 完成初始化()
        {
            //写配置文件
            MMPU.setFiles("IsFirstTimeUsing", "0");
            MMPU.setFiles("DataSource", _数据源.ToString());
            MMPU.数据源 = _数据源;
            MMPU.是否第一次使用DDTV = false;
           
            //关闭此窗口
            this.Close();
        }

        private void 导入VTBVUP数据层_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue) // 显示
            {
                if (string.IsNullOrEmpty(MMPU.Cookie))
                {
                    导入VTBVUP提示文本.Text = "您未登录bilibili，请直接点击\"下一步\"";
                }
                else if (_已导入 == false)
                {
                    上一步按钮.IsEnabled = false;
                    下一步按钮.IsEnabled = false;
                    导入VTBVUP((增加的数量, 已经存在的数量) =>
                    {
                        导入VTBVUP提示文本.Text = $"导入成功！原有:{已经存在的数量}个，新增VTB/VUP数：{增加的数量}";
                        上一步按钮.IsEnabled = true;
                        下一步按钮.IsEnabled = true;
                    });
                }
            }
        }

        private void 导入VTBVUP(Action<int,int> callback)
        {
            NewThreadTask.Run(runOnLocalThread =>
            {
                runOnLocalThread(() => 导入VTBVUP提示文本.Text = "正在加载房间列表数据，该过程耗时较长，请耐心等待……");
                MMPU.加载网络房间方法.更新网络房间缓存();
                MMPU.加载网络房间方法.是否正在缓存 = true;
                while (MMPU.加载网络房间方法.是否正在缓存)
                {
                    Thread.Sleep(500);
                }

                runOnLocalThread(() => 导入VTBVUP提示文本.Text = "正在导入关注列表里符合的VTB/VUP数据，请稍候……");
                int 增加的数量 = 0;
                int 已经存在的数量 = 0;
                RoomInit.RoomConfigFile = MMPU.读取exe默认配置文件("RoomConfiguration", "./RoomListConfig.json");
                RoomInit.InitializeRoomConfigFile();
                RoomInit.InitializeRoomList(0, false, false);
                RoomBox rlc = JsonConvert.DeserializeObject<RoomBox>(ReadConfigFile(RoomConfigFile));
                RoomBox RB = new RoomBox
                {
                    data = new List<RoomCadr>()
                };
                if (rlc.data != null)
                {
                    foreach (var item in rlc.data)
                    {
                        RB.data.Add(item);
                    }
                }
                List<MMPU.加载网络房间方法.选中的网络房间> 符合条件的房间 = new List<MMPU.加载网络房间方法.选中的网络房间>();
                JObject BB = bilibili.根据UID获取关注列表(MMPU.UID);
                foreach (var 账号关注数据 in BB["data"])
                {
                    foreach (var 网络房间数据 in MMPU.加载网络房间方法.列表缓存)
                    {
                        if (账号关注数据["UID"].ToString() == 网络房间数据.UID)
                        {
                            符合条件的房间.Add(new MMPU.加载网络房间方法.选中的网络房间()
                            {
                                UID = 网络房间数据.UID,
                                名称 = 网络房间数据.名称,
                                官方名称 = 网络房间数据.官方名称,
                                平台 = 网络房间数据.平台,
                                房间号 = null,
                                编号 = 0
                            });
                            break;
                        }
                    }
                }
                foreach (var 符合条件的 in 符合条件的房间)
                {
                    if (!string.IsNullOrEmpty(符合条件的.UID))
                    {
                        string 房间号 = bilibili.通过UID获取房间号(符合条件的.UID);

                        符合条件的.房间号 = 房间号;
                        bool 是否已经存在 = false;
                        foreach (var item in bilibili.RoomList)
                        {
                            if (item.房间号 == 房间号)
                            {
                                是否已经存在 = true;
                                break;
                            }
                        }
                        if (!是否已经存在 && !string.IsNullOrEmpty(房间号.Trim('0')))
                        {
                            增加的数量++;
                            RB.data.Add(new RoomCadr { Name = 符合条件的.名称, RoomNumber = 符合条件的.房间号, Types = 符合条件的.平台, RemindStatus = false, status = false, VideoStatus = false, OfficialName = 符合条件的.官方名称, LiveStatus = false });
                        }
                        else
                        {
                            已经存在的数量++;
                        }
                    }
                    Thread.Sleep(100);
                }
                string JOO = JsonConvert.SerializeObject(RB);
                MMPU.储存文本(JOO, RoomConfigFile);
                InitializeRoomList(0, false, false);

                _已导入 = true;
                runOnLocalThread(() =>
                {
                    callback(增加的数量, 已经存在的数量);
                });
            }, this);
        }

        private void 设置数据源层_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue) // 显示
            {
                
            }
            else // 隐藏
            {
                if (vtbsmoe单选按钮.IsChecked == null
                    || vtbsmoe单选按钮.IsChecked == true)
                {
                    _数据源 = 0;
                }
                else
                {
                    _数据源 = 1;
                }
            }
        }

        private void 登录bilibili按钮_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(MMPU.读ini配置文件("User", "Cookie", MMPU.BiliUserFile)))
            {
                登录bilibili提示文本1.Text = "您似乎已经登录";
                登录bilibili提示文本2.Text = "正在初始化BiliUser配置文件，请稍候……";
                MMPU.BiliUser配置文件初始化(0);
                登录bilibili提示文本2.Text = "初始化BiliUser配置文件成功！请点击\"下一步\"";
                登录bilibili按钮.IsEnabled = false;
                return;
            }

            BiliLoginWindowQR BLW = new BiliLoginWindowQR();
            BLW.ShowDialog();
            if (!string.IsNullOrEmpty(MMPU.Cookie))
            {
                登录bilibili提示文本1.Text = "登录成功！";
                登录bilibili提示文本2.Text = "请点击\"下一步\"";
                登录bilibili按钮.IsEnabled = false;
            }
            else
            {
                登录bilibili提示文本1.Text = "登录失败，请重试";
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception) { }
        }
        private void 跳过设置按钮_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("跳过设置默认使用vtbs数据源，如需设置和导入关注列表，请在主界面“设置界面”进行设置");
            _数据源 = 0;
            完成初始化();
        }
    }
}
