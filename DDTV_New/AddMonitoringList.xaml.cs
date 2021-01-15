using Auxiliary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using System.Threading;
using static Auxiliary.RoomInit;
using DDTV_New.Utility;

namespace DDTV_New
{
    /// <summary>
    /// AddMonitoringList.xaml 的交互逻辑
    /// </summary>
    public partial class AddMonitoringList : Window
    {
        public static string GUID = "";
        public static bool 直播状态 = false;
        /// <summary>
        /// 确认框
        /// </summary>
        /// <param name="Title">标题名称</param>
        /// <param name="CN_Name">中文名称</param>
        /// <param name="LA_Name">官方名称</param>
        /// <param name="Platform">平台</param>
        /// <param name="号码">房间号</param>
        /// <param name="LiveStatus"></param>
        public AddMonitoringList(string Title,string CN_Name,string LA_Name,string Platform,string 号码,bool LiveStatus)
        {
            直播状态 = LiveStatus;
            GUID = 号码;
            InitializeComponent();
            平台.Items.Clear();
            this.Title = Title;
            中文名称.Text = CN_Name;
            官方名称.Text = LA_Name;
            唯一码.Text = 号码;
            平台.Items.Add("bilibili");
            平台.Items.Add("youtube");
            平台.SelectedItem = Platform;
            if (this.Title == "修改单推属性")
            {
                平台.IsEnabled = false;
                唯一码.IsEnabled = false;
            }
            //new Task((() =>  {
            //    try
            //    {
            //        JObject jo = (JObject)JsonConvert.DeserializeObject(MMPU.TcpSend(20002, "", true));
            //        if(jo["name"].Count()>0)
            //        {
            //            this.Dispatcher.Invoke(new Action(delegate
            //            {
            //                平台.Items.Clear();
            //            }));
            //        }
            //        for (int i = 0; i < jo["name"].Count(); i++)
            //        {
            //            this.Dispatcher.Invoke(new Action(delegate
            //            {
            //                平台.Items.Add(jo["name"][i].ToString());
            //            }));
                        
            //        }
            //    }
            //    catch (Exception)
            //    {
            //        //MessageBox.Show("请求服务器平台列表失败");
            //        //  return;
            //    }
            //})).Start();
          

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            //MessageBoxResult dr = System.Windows.MessageBox.Show("确认取消增加？", "取消", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            //if (dr == MessageBoxResult.OK)
            //{
               
            //}
        }

        private void BT1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(平台.SelectedItem.ToString()))
                {
                    MessageBox.Show("未选择平台");
                    return;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("未选择平台");
                return;
            }
            if (string.IsNullOrEmpty(中文名称.Text) || string.IsNullOrEmpty(平台.SelectedItem.ToString()) || string.IsNullOrEmpty(唯一码.Text))
            {
                MessageBox.Show("不能留空");
                return;
            }
            if (this.Title == "添加新单推"|| this.Title == "从网络添加房间")
            {
                if (平台.SelectedItem.ToString() == "bilibili")
                {
                    foreach (var item in RoomInit.bilibili房间主表)
                    {
                        if (item.唯一码 == 唯一码.Text)
                        {
                            MessageBox.Show("已存在相同的房间号!\r\n" + item.名称 + " " + item.平台 + " " + item.唯一码);
                            return;
                        }
                    }
                }
                else if(平台.SelectedItem.ToString()=="youtube")
                {
                    foreach (var item in RoomInit.youtube房间主表)
                    {
                        if (item.唯一码 == 唯一码.Text)
                        {
                            MessageBox.Show("已存在相同的房间号!\r\n" + item.名称 + " " + item.平台 + " " + item.唯一码);
                            return;
                        }
                    }
                }
                新增V信息 NEWV = new 新增V信息() { CN_Name = 中文名称.Text, LA_Name = 官方名称.Text, Platform = 平台.SelectedItem.ToString(), GUID = 唯一码.Text };
                NewThreadTask.Run(() =>
                {
                    MMPU.TcpSend(Server.RequestCode.GET_NEW_MEMBER_LIST_CONTENT,
                        JsonConvert.SerializeObject(NEWV), true,50);
                });

                RoomBox rlc = JsonConvert.DeserializeObject<RoomBox>(ReadConfigFile(RoomConfigFile));
                RoomBox RB = new RoomBox();
                RB.data = new List<RoomCadr>();
                if (rlc.data != null)
                {
                    foreach (var item in rlc.data)
                    {
                        RB.data.Add(item);
                        if (RoomInit.根据唯一码获取直播状态(item.RoomNumber))
                        {
                            RB.data[RB.data.Count() - 1].LiveStatus = true;
                        }
                    }
                }
                RB.data.Add(new RoomCadr { Name = 中文名称.Text,RoomNumber = 唯一码.Text, Types = 平台.SelectedItem.ToString(), RemindStatus = false, status = false, VideoStatus = false, OfficialName = 官方名称.Text, LiveStatus = RoomInit.根据唯一码获取直播状态(GUID) });
                string JOO = JsonConvert.SerializeObject(RB);
                MMPU.储存文本(JOO, RoomConfigFile);
                if (平台.SelectedItem.ToString() == "bilibili")
                {
                    InitializeRoomList(int.Parse(唯一码.Text), false, false);
                }
                else
                {
                    InitializeRoomList(0, false, false);
                }

                  

                //更新房间列表(平台.SelectedItem.ToString(), 唯一码.Text,1);
                //MessageBox.Show("添加成功");

            }
            else if(this.Title=="修改单推属性")
            {
                新增V信息 NEWV = new 新增V信息() { CN_Name = 中文名称.Text, LA_Name = 官方名称.Text, Platform = 平台.SelectedItem.ToString(), GUID = 唯一码.Text };
                NewThreadTask.Run(() =>
                {
                    MMPU.TcpSend(Server.RequestCode.GET_NEW_MEMBER_LIST_CONTENT,
                        JsonConvert.SerializeObject(NEWV), true,50);
                });

                RoomBox rlc = JsonConvert.DeserializeObject<RoomBox>(ReadConfigFile(RoomConfigFile));
                RoomBox RB = new RoomBox();
                RB.data = new List<RoomCadr>();
                if (rlc.data != null)
                {
                    foreach (var item in rlc.data)
                    {
                        if (item.RoomNumber == GUID)
                        {
                            RB.data.Add(item);
                            RB.data[RB.data.Count - 1].Name = 中文名称.Text;
                            RB.data[RB.data.Count - 1].OfficialName = 官方名称.Text;
                            RB.data[RB.data.Count - 1].Types = 平台.SelectedItem.ToString();
                        }
                        else
                        {
                            RB.data.Add(item);
                            if (RoomInit.根据唯一码获取直播状态(item.RoomNumber))
                            {
                                RB.data[RB.data.Count() - 1].LiveStatus = true;
                            }
                        }
                    }
                }
               
                string JOO = JsonConvert.SerializeObject(RB);
                MMPU.储存文本(JOO, RoomConfigFile);
                InitializeRoomList(0,false, false);
                //var rlc2 = JsonConvert.DeserializeObject<RoomBox>(ReadConfigFile(RoomConfigFile));
                //RoomBox RB = new RoomBox();
                //RB.data = new List<RoomCadr>();
                //int rlclen = 房间主表.Count()-1;
                //int 覆盖的编号 = 0;
                //for (int i =0;i< rlclen; i++)
                //{
                //    if(房间主表[i].唯一码==GUID)
                //    {
                //        覆盖的编号 = i;
                //        //房间主表.Remove(房间主表[i]);
                //        //i--;
                //        RB.data.Add(new RoomCadr { Name = 中文名称.Text, RoomNumber = 唯一码.Text, Types = 平台.SelectedItem.ToString(), RemindStatus = false, status = false, VideoStatus = false, OfficialName = 官方名称.Text,LiveStatus= RoomInit.根据唯一码获取直播状态(GUID) });
                //    }
                //    else
                //    {
                //        RB.data.Add(new RoomCadr(){ LiveStatus= 房间主表[i] .直播状态,Name= 房间主表[i] .名称,OfficialName= 房间主表[i] .原名,RoomNumber= 房间主表[i] .唯一码,VideoStatus= 房间主表[i] .是否录制,Types= 房间主表[i] .平台, RemindStatus= 房间主表[i] .是否提醒,status=false });
                //        if (RoomInit.根据唯一码获取直播状态(房间主表[i].唯一码))
                //        {
                //            RB.data[RB.data.Count() - 1].LiveStatus = true;
                //        }
                //    }
                //}
                //房间主表.Clear();
                //foreach (var item in RB.data)
                //{
                //    房间主表.Add(new RL { 名称=item.Name,原名=item.OfficialName,唯一码=item.RoomNumber,平台=item.Types,是否录制=item.VideoStatus,是否提醒=item.RemindStatus,直播状态=item.LiveStatus});
                //}

                //新增V信息 NEWV = new 新增V信息() { CN_Name = 中文名称.Text, LA_Name = 官方名称.Text, Platform = 平台.SelectedItem.ToString(), GUID = 唯一码.Text };

                //new Task(() => { MMPU.TcpSend(20001, JsonConvert.SerializeObject(NEWV), true); }).Start();
                //string JOO = JsonConvert.SerializeObject(RB);
                //MMPU.储存文本(JOO, RoomConfigFile);
                //InitializeRoomList();
                ////MessageBox.Show("修改成功");

            }
            this.Close();
        }
        public class 新增V信息
        {
            public string CN_Name { set; get; }//中文名
            public string LA_Name { set; get; }//外文名
            public string Platform { set; get; }//平台
            public string GUID { set; get; }//唯一码（房间号,频道号）
        }
    }
}
