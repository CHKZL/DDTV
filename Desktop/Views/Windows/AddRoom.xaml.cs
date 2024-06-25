using Core;
using Desktop.Models;
using Masuit.Tools;
using Microsoft.AspNetCore.Http.HttpResults;
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
    public partial class AddRoom : FluentWindow
    {
        bool _IsAutoRec = false;
        bool _IsDanmu = false;
        bool _IsRemind = false;
        Mode _Mode;

        public enum Mode
        {
            RoomNumberMode,
            UidNumberMode
        }

        public AddRoom(Mode mode)
        {
            InitializeComponent();
            _Mode = mode;
            switch (_Mode)
            {
                case Mode.RoomNumberMode:
                    RoomId_TextBox.PlaceholderText = "请输入房间号";
                    break;
                case Mode.UidNumberMode:
                     RoomId_TextBox.PlaceholderText = "请输入UID,多个UID请使用逗号分割";
                    break;
            }
        }
        private void AutoRecChechBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)AutoRecChechBox.IsChecked)
            {
                _IsAutoRec = true;
            }
            else
            {
                _IsAutoRec = false;
                RecDanmuChechBox.IsChecked = false;
            }
        }

        private void RemindChechBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)RemindChechBox.IsChecked)
            {
                _IsRemind= true;
            }
            else
            {
                _IsRemind = false;
            }
        }

        private void RecDanmuChechBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)RecDanmuChechBox.IsChecked)
            {
                _IsDanmu  = true;
            }
            else
            {
                _IsDanmu = false;
            }
        }

        private void AddRoomSave_Click(object sender, RoutedEventArgs e)
        {
            switch (_Mode)
            {
                case Mode.RoomNumberMode:
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
                            Task.Run(() =>
                            {
                                Dictionary<string, string> dic = new Dictionary<string, string>();
                                Dispatcher.Invoke(() =>
                                {
                                    dic = new Dictionary<string, string>
                                    {
                                        {"room_id", RoomId_TextBox.Text },
                                        {"auto_rec",_IsAutoRec.ToString() },
                                        {"remind",_IsRemind.ToString() },
                                        {"rec_danmu",_IsDanmu.ToString() },
                                    };
                                });
                                int State = NetWork.Post.PostBody<int>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/set_rooms/add_room", dic).Result;
                                Dispatcher.Invoke(() =>
                                {
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
                                        default:
                                            Core.LogModule.Log.Warn(nameof(AddRoomSave_Click), "调用Core的API[add_room]增加房间失败，返回的对象为Null，详情请查看Core日志", null, true);
                                            return;
                                    }
                                });
                            });
                        }
                        break;
                    }
                case Mode.UidNumberMode:
                    {
                        string UidStr = RoomId_TextBox.Text.Replace("，",",");
                        List<long> UidList= new List<long>();
                        if (string.IsNullOrEmpty(UidStr))
                        {
                            System.Windows.MessageBox.Show("请输入房间号");
                            return;
                        }
                        foreach (var item in UidStr.Split(','))
                        {
                            if (!string.IsNullOrEmpty(item) && long.TryParse(item, out long uid) && uid > 0)
                            {
                                UidList.Add(uid);
                            }
                        }
                        if (UidList.Count > 0)
                        {
                            Task.Run(() =>
                            {

                                long[] _uidl = new long[UidList.Count];
                                for (int i = 0; i < UidList.Count; i++)
                                {
                                    _uidl[i] = UidList[i];
                                }
                                Dictionary<string, string> dic = new Dictionary<string, string>
                                {
                                    {"uids", string.Join(",",_uidl) },
                                    {"auto_rec",_IsAutoRec.ToString() },
                                    {"remind",_IsRemind.ToString() },
                                    {"rec_danmu",_IsDanmu.ToString() },
                                };
                                List<(long key, int State, string Message)> State = NetWork.Post.PostBody<List<(long key, int State, string Message)>>($"{Config.Core_RunConfig._DesktopIP}:{Config.Core_RunConfig._DesktopPort}/api/set_rooms/batch_add_room", dic).Result;
                                Dispatcher.Invoke(() =>
                                {
                                    if (State == null)
                                    {
                                        Core.LogModule.Log.Warn(nameof(AddRoomSave_Click), "调用Core的API[batch_add_room]批量添加房间失败，返回的对象为Null，详情请查看Core日志", null, true);
                                        Save_Message.Content = $"增加房间失败，如果一直提示该错误，请联系开发者";
                                        return;
                                    }
                                    else
                                    {
                                        int Count = UidList.Count;
                                        int Ok = State.Count(item => item.State == 1);
                                        int Repeat = State.Count(item => item.State == 2);
                                        int NotPresent = State.Count(item => item.State == 3);
                                        Save_Message.Content = $"输入有效UID {Count} 个，{Repeat} 个已存在，{NotPresent} 个房间不存在，成功添加 {Ok} 个";
                                    }
                                });
                            });
                        }
                        break;
                    }
            }

        }
    }
}
