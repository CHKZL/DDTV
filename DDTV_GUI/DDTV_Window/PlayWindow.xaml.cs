using HandyControl.Controls;
using LibVLCSharp.Shared;

using System.Windows;

using System.Windows.Controls.Primitives;

using System.Windows.Input;


namespace DDTV_GUI.DDTV_Window
{
    /// <summary>
    /// PlayWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PlayWindow : System.Windows.Window
    {

        LibVLC vlcVideo = null;
        public PlayWindow(long Uid)
        {
            InitializeComponent();

            //InitVLC();
            //Play(Uid);
        }

        private void InitVLC()
        {

            //vlcVideo = new LibVLC();
            //VLC = new MediaPlayer(vlcVideo);
            //VLCV.Loaded += (sender, e) => VLCV.MediaPlayer = _mediaPlayer;
            //this.vlcVideo = new VlcControl();
            //this.VLC.Content = this.vlcVideo;
            //var libDirectory = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\plugins\\vlc");
            //this.vlcVideo.SourceProvider.CreatePlayer(libDirectory);//创建视频播放器
        }
        private void Play(long Uid)
        {
            string Ulr = DDTV_Core.SystemAssembly.BilibiliModule.API.RoomInfo.playUrl(Uid, DDTV_Core.SystemAssembly.BilibiliModule.Rooms.RoomInfoClass.PlayQuality.HighDefinition);

            string videourl = Ulr;
            //vlcVideo.SourceProvider.MediaPlayer.Play(new Uri(videourl));
            //Clipboard.SetDataObject(videourl);
            //vlcVideo.SourceProvider.MediaPlayer.Play(@"E:\老D\DDTV2\DDTV_New\bin\Debug\tmp\bilibili_桐生ココ-桐生ココ_21752686\【ARK】和露西娅前辈一起攻略下层洞窟！_20200508220611583.flv");
          
        }

        private void GlowWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender; 
            if((bool)toggleButton.IsChecked)
            {
                PlayControl.Visibility = Visibility.Visible;
            }
            else
            {
                PlayControl.Visibility = Visibility.Collapsed;
            }
        }
    }
}
