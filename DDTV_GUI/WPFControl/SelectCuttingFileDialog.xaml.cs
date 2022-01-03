using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_GUI.DDTV_Window;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDTV_GUI.WPFControl
{
    /// <summary>
    /// SelectCuttingFileDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SelectCuttingFileDialog
    {
        EventHandler<EventArgs> eventHandler;
        int index = -1;
        string FileName = "";
        RoomInfoClass.RoomInfo roomInfo=null;
        public SelectCuttingFileDialog(FileInfo[] FileList, EventHandler<EventArgs> _, RoomInfoClass.RoomInfo _roomInfo)
        {
            InitializeComponent();
            eventHandler = _;
            roomInfo = _roomInfo;
            int i = 0;
            foreach (FileInfo File in FileList)
            {
                FileListComboBox.Items.Add(new ComboBoxItem()
                {
                    Content = File.Name,
                    Tag = i
                });
                i++;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(index<0)
            {
                MessageBox.Show("请选择文件！");
                return;
            }
            ClipWindow clipWindow = new(FileName, roomInfo);
            clipWindow.Show();
            eventHandler.Invoke(this, EventArgs.Empty);
        }

        private void FileListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ComboBoxItem _ = e.AddedItems[0] as ComboBoxItem;
                index = int.Parse(_.Tag.ToString());
                FileName = _.Content.ToString();
            }
        }

        private void Border_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {

        }
    }
}
