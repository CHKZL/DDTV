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
using DDTV_New.Utility;

namespace DDTV_New
{
    
    /// <summary>
    /// 下载工具.xaml 的交互逻辑
    /// </summary>
    public partial class 下载工具 : Window
    {
        public static int 选中的行 = -1;
        public 下载工具()
        {
            InitializeComponent();

            NewThreadTask.Loop(runOnLocalThread =>
            {
                //更新下载队列
                int i = 1;
                runOnLocalThread(() => DownList.Items.Clear());
                try
                {
                    foreach (var item in Auxiliary.MMPU.DownList)
                    {
                        runOnLocalThread(() =>
                        {
                            DownList.Items.Add(new 
                            { 
                                编号 = i, 
                                唯一码 = item.DownIofo.房间_频道号, 
                                名称 = item.DownIofo.标题, 
                                状态 = item.DownIofo.结束时间 > 0 ? "下载结束" : "下载中", 
                                备注 = item.DownIofo.备注, 
                                平台 = item.DownIofo.平台, 
                                已下载 = item.DownIofo.已下载大小str, 
                                开始时间 = Auxiliary.MMPU.Unix转换为DateTime(
                                        item.DownIofo.开始时间.ToString()
                                    ).ToString("yyyy-MM-dd HH:mm:ss"), 
                                结束时间 = item.DownIofo.结束时间 > 0 
                                    ? Auxiliary.MMPU.Unix转换为DateTime(
                                          item.DownIofo.结束时间.ToString()
                                      ).ToString("yyyy-MM-dd HH:mm:ss") 
                                    : "", 
                                保存地址 = item.DownIofo.文件保存路径 
                            });
                        });
                        i++;
                    }
                }
                catch (Exception) { }
            }, this, 1 * 1000);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void 取消下载按钮点击事件(object sender, RoutedEventArgs e)
        {
            if (选中的行 >= 0)
            {
                MessageBoxResult dr = MessageBox.Show("确认取消第" + (选中的行 + 1) + "行的下载任务？\n详细内容:\n" + Auxiliary.MMPU.DownList[选中的行].DownIofo.平台 + "\n" + Auxiliary.MMPU.DownList[选中的行].DownIofo.房间_频道号 + "\n" + Auxiliary.MMPU.DownList[选中的行].DownIofo.标题, "确定", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (dr == MessageBoxResult.OK)
                {
                    Auxiliary.MMPU.DownList[选中的行].DownIofo.下载状态 = false;
                    Auxiliary.MMPU.DownList[选中的行].DownIofo.结束时间 = Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                    Auxiliary.MMPU.DownList[选中的行].DownIofo.备注 = "用户取消下载";
                    Auxiliary.MMPU.DownList[选中的行].DownIofo.WC.CancelAsync();
                }
            }
        }

        private void DownList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int num = this.DownList.SelectedIndex; //选中的listview的行
            if(num>=0)
            {
                选择行.Content = "当前选择"+ (num+1) + "行:" + Auxiliary.MMPU.DownList[num].DownIofo.平台 + "  " + Auxiliary.MMPU.DownList[num].DownIofo.房间_频道号 + "  " + Auxiliary.MMPU.DownList[num].DownIofo.标题;
                选中的行 = num;
            }

        }

        private void 清除列表按钮点击事件(object sender, RoutedEventArgs e)
        {
            for(int i =0;i< Auxiliary.MMPU.DownList.Count;i++)
            {
                if (!Auxiliary.MMPU.DownList[i].DownIofo.下载状态|| Auxiliary.MMPU.DownList[i].DownIofo.结束时间>0)
                {
                    Auxiliary.MMPU.DownList.Remove(Auxiliary.MMPU.DownList[i]);
                    i--;
                }
            }
           
            //for (int i = 0; i < DownList.Items.Count; i++)
            //{
            //    if (Auxiliary.MMPU.寻找下载列表键值(DownList.Items[i].ToString(), "状态") == "下载结束")
            //    {
            //        DownList.Items.Remove(DownList.Items[i]);
            //    }  
            //}
        }
    }
}
