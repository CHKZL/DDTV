using Desktop.Models;
using Masuit.Tools;
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
    public partial class AddRoom_roomid : FluentWindow
    {
        bool IsAutoRec = false;
        bool IsDanmu = false;
        bool IsRemind = false;
        public AddRoom_roomid()
        {
            InitializeComponent();

        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void AutoRecChechBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)AutoRecChechBox.IsChecked)
            {
                IsAutoRec = true;
            }
            else
            {
                IsAutoRec = false;
                RecDanmuChechBox.IsChecked = false;
            }
        }

        private void RemindChechBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)RemindChechBox.IsChecked)
            {
                IsDanmu = true;
            }
            else
            {
                IsDanmu = false;
            }
        }

        private void RecDanmuChechBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)RecDanmuChechBox.IsChecked)
            {
                IsDanmu = true;
            }
            else
            {
                IsDanmu = false;
            }
        }

        private void AddRoomSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(RoomId_TextBox.Text))
            {
                System.Windows.MessageBox.Show("请输入房间号");
                return;
            }
            if (!long.TryParse(RoomId_TextBox.Text, out long room_id) || room_id < 1)
            {
                System.Windows.MessageBox.Show("请输入正确的房间号");
                return;
            }
            else
            {
                Dictionary<string, object> dic = new Dictionary<string, object>
                {
                    {"room_id", RoomId_TextBox.Text },
                    {"auto_rec",IsAutoRec },
                    {"remind",IsDanmu },
                    {"rec_danmu",IsRemind },
                };
                int State = NetWork.Post.PostBody<int>("http://127.0.0.1:11419/api/set_rooms/add_room", dic);
                switch (State)
                {
                    case 1:
                        Save_Message.Content = $"房间({RoomId_TextBox.Text})添加成功";
                        //添加成功
                        break;
                    case 2:
                        Save_Message.Content = $"房间({RoomId_TextBox.Text})已在列表中，添加失败";
                        //房间已存在
                        break;
                    case 3:
                        Save_Message.Content = $"房间({RoomId_TextBox.Text})不存在，添加失败";
                        //房间不存在
                        break;
                    case 4:
                        Save_Message.Content = $"添加房间({RoomId_TextBox.Text})由于参数错误失败";
                        //参数有误
                        break;
                }
            }
        }
    }
}
