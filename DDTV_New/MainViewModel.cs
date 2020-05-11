using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_New
{
    public class MainViewModel : ReactiveObject
    {
        public MainViewModel()
        {
            _tabText = this
                .WhenAnyValue(x => x.NowStreamingNum)
                .Select(num =>
                {
                    if (num == -1) return "单推列表正在更新中.....";
                    if (num == 0) return "监控列表中没有直播中的单推对象";
                    return "在监控列表中有" + num + "个单推对象正在直播";
                })
                .ToProperty(this, x => x.TabText);

            _serverDelayBilibiliText = this
                .WhenAnyValue(x => x.ServerDelayBilibili)
                .Select(delay =>
                {
                    if (delay == -1.0) return "国内服务器延迟(阿B): 未测试";
                    if (delay == -2.0) return "国内服务器延迟(阿B): 连接超时";
                    return "国内服务器延迟(阿B): " + delay + "ms";
                })
                .ToProperty(this, x => x.ServerDelayBilibiliText);

            _serverDelayYoutubeText = this
               .WhenAnyValue(x => x.ServerDelayYoutube)
               .Select(delay =>
               {
                   if (delay == -1.0) return "国外服务器延迟(404): 未测试";
                   if (delay == -2.0) return "国外服务器延迟(404): 连接超时";
                   return "国外服务器延迟(404): " + delay + "ms";
               })
               .ToProperty(this, x => x.ServerDelayYoutubeText);

            _latestDataUpdateTimeText = this
                .WhenAnyValue(x => x.LatestDataUpdateTime)
                .Select(time =>
                {
                    if (time == null) return "数据更新时间: 单推列表正在更新中.....";
                    return "数据更新时间: " + time.ToString("yyyy-MM-dd HH:mm:ss");
                })
                .ToProperty(this, x => x.LatestDataUpdateTimeText);
        }

        private int _tanoshiNum;
        public int TanoshiNum 
        { 
            get => _tanoshiNum;
            set => this.RaiseAndSetIfChanged(ref _tanoshiNum, value); 
        }

        private string _announcement;
        public string Announcement 
        { 
            get => _announcement;
            set => this.RaiseAndSetIfChanged(ref _announcement, value);
        }

        private double _serverDelayBilibili;
        public double ServerDelayBilibili
        {
            get => _serverDelayBilibili;
            set => this.RaiseAndSetIfChanged(ref _serverDelayBilibili, value);
        }
        private double _serverDelayYoutube;
        public double ServerDelayYoutube
        {
            get => _serverDelayYoutube;
            set => this.RaiseAndSetIfChanged(ref _serverDelayYoutube, value);
        }
        private readonly ObservableAsPropertyHelper<string> _serverDelayBilibiliText;
        public string ServerDelayBilibiliText => _serverDelayBilibiliText.Value;
        private readonly ObservableAsPropertyHelper<string> _serverDelayYoutubeText;
        public string ServerDelayYoutubeText => _serverDelayYoutubeText.Value;

        private int _nowStreamingNum = -1;
        public int NowStreamingNum
        {
            get => _nowStreamingNum;
            set => this.RaiseAndSetIfChanged(ref _nowStreamingNum, value);
        }
        private int _notStreamingNum = -1;
        public int NotStreamingNum
        {
            get => _notStreamingNum;
            set => this.RaiseAndSetIfChanged(ref _notStreamingNum, value);
        }

        private readonly ObservableAsPropertyHelper<string> _tabText;
        public string TabText => _tabText.Value;

        private string _pushNotification;
        public string PushNotification
        {
            get => _pushNotification;
            set => this.RaiseAndSetIfChanged(ref _pushNotification, value);
        }

        private DateTime _latestDataUpdateTime;
        public DateTime LatestDataUpdateTime
        {
            get => _latestDataUpdateTime;
            set => this.RaiseAndSetIfChanged(ref _latestDataUpdateTime, value);
        }
        private readonly ObservableAsPropertyHelper<string> _latestDataUpdateTimeText;
        public string LatestDataUpdateTimeText => _latestDataUpdateTimeText.Value;
    }
}
