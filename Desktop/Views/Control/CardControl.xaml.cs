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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace Desktop.Views.Control
{
    /// <summary>
    /// CardControl.xaml 的交互逻辑
    /// </summary>
    public partial class CardControl : UserControl
    {
        public Models.DataCard DataCard { get; set; }
        public CardControl()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (System.Windows.Controls.MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var grid = (Grid)contextMenu.PlacementTarget;
            Models.DataCard dataContext = (Models.DataCard)grid.DataContext;
        }

        private void Border_DoubleClickToOpenPlaybackWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var border = (Border)sender;
                var grid = (Grid)border.Parent;
                Models.DataCard dataContext = (Models.DataCard)grid.DataContext;
                Windows.PlayWindow playWindow = new Windows.PlayWindow(dataContext.Room_Id);
                playWindow.Show();
            }
        }
    }
}
