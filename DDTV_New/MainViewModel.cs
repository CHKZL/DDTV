using Auxiliary;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DDTV_New
{
    public class MainViewModel : ReactiveObject
    {
        public MainViewModel()
        {
            _当前状态 = this
                .WhenAnyValue(x => x.直播中数量)
                .Select(num =>
                {
                    if (num == -1) return "单推列表正在更新中.....";
                    if (num == 0) return "监控列表中没有直播中的单推对象";
                    return "在监控列表中有" + num + "个单推对象正在直播";
                })
                .ToProperty(this, x => x.当前状态);

            _数据源服务器延迟文本 = this
              .WhenAnyValue(x => x.数据源服务器延迟)
              .Select(delay =>
              {
                  if (delay == -1.0) return "数据源服务器(vtbs)延迟: 未测试";
                  if (delay == -2.0) return "数据源服务器(vtbs)延迟: 连接超时";
                  return "数据源服务器(vtbs)延迟: " + delay + "ms";
              })
              .ToProperty(this, x => x.数据源服务器延迟文本);

            _国内服务器延迟文本 = this
                .WhenAnyValue(x => x.国内服务器延迟)
                .Select(delay =>
                {
                    if (delay == -1.0) return "国内服务器延迟(阿B): 未测试";
                    if (delay == -2.0) return "国内服务器延迟(阿B): 连接超时";
                    return "国内服务器延迟(阿B): " + delay + "ms";
                })
                .ToProperty(this, x => x.国内服务器延迟文本);

            _国外服务器延迟文本 = this
               .WhenAnyValue(x => x.国外服务器延迟)
               .Select(delay =>
               {
                   if (delay == -1.0) return "国外服务器延迟(404): 未测试";
                   if (delay == -2.0) return "国外服务器延迟(404): 连接超时";
                   return "国外服务器延迟(404): " + delay + "ms";
               })
               .ToProperty(this, x => x.国外服务器延迟文本);

            _数据更新时间文本 = this
                .WhenAnyValue(x => x.数据更新时间)
                .Select(time =>
                {
                    if (time == null) return "数据更新时间: 单推列表正在更新中.....";
                    return "数据更新时间: " + time.ToString("yyyy-MM-dd HH:mm:ss");
                })
                .ToProperty(this, x => x.数据更新时间文本);

            更新默认音量命令 = ReactiveCommand.Create<int, Unit>(新音量 =>
            {
                MMPU.默认音量 = 新音量;
                MMPU.修改默认音量设置(MMPU.默认音量);
                return Unit.Default;
            });

            this.WhenAnyValue(x => x.默认音量)
                .Throttle(TimeSpan.FromMilliseconds(500)) // 每500ms更新一次MMPU数值
                .InvokeCommand(更新默认音量命令);

            默认音量 = MMPU.默认音量;

            切换界面命令 = ReactiveCommand.Create<string, Unit>(要切换的层对应的字符串 =>
            {
                foreach (string 层对应的字符串 in 所有层.Keys)
                {
                    if (层对应的字符串 == 要切换的层对应的字符串)
                    {
                        if (层对应的字符串 != "home")
                            所有层["top层"].Visibility = Visibility.Visible;
                        所有层[层对应的字符串].Visibility = Visibility.Visible;
                    }
                    else 所有层[层对应的字符串].Visibility = Visibility.Collapsed;
                }
                return Unit.Default;
            });

            _选中内容文本 = this
                .WhenAnyValue(x => x.当前选中直播间)
                .Select(当前选中直播间 =>
                {
                    if (当前选中直播间 == null) return "";

                    return 当前选中直播间.平台 + "\n" 
                        + 当前选中直播间.唯一码 + "\n" 
                        + 当前选中直播间.名称;
                })
                .ToProperty(this, x => x.选中内容文本);
        }

        private int _单推数量;
        public int 单推数量
        {
            get => _单推数量;
            set => this.RaiseAndSetIfChanged(ref _单推数量, value);
        }

        private string _推送内容1;
        public string 推送内容1
        {
            get => _推送内容1;
            set => this.RaiseAndSetIfChanged(ref _推送内容1, value);
        }

        private double _数据源服务器延迟;
        public double 数据源服务器延迟
        {
            get => _数据源服务器延迟;
            set => this.RaiseAndSetIfChanged(ref _数据源服务器延迟, value);
        }
        private readonly ObservableAsPropertyHelper<string> _数据源服务器延迟文本;
        public string 数据源服务器延迟文本 => _数据源服务器延迟文本.Value;

        private double _国内服务器延迟;
        public double 国内服务器延迟
        {
            get => _国内服务器延迟;
            set => this.RaiseAndSetIfChanged(ref _国内服务器延迟, value);
        }
        private readonly ObservableAsPropertyHelper<string> _国内服务器延迟文本;
        public string 国内服务器延迟文本 => _国内服务器延迟文本.Value;

        private double _国外服务器延迟;
        public double 国外服务器延迟
        {
            get => _国外服务器延迟;
            set => this.RaiseAndSetIfChanged(ref _国外服务器延迟, value);
        }
        private readonly ObservableAsPropertyHelper<string> _国外服务器延迟文本;
        public string 国外服务器延迟文本 => _国外服务器延迟文本.Value;


        private int _直播中数量 = -1;
        public int 直播中数量
        {
            get => _直播中数量;
            set => this.RaiseAndSetIfChanged(ref _直播中数量, value);
        }
        private int _未直播数量 = -1;
        public int 未直播数量
        {
            get => _未直播数量;
            set => this.RaiseAndSetIfChanged(ref _未直播数量, value);
        }

        private readonly ObservableAsPropertyHelper<string> _当前状态;
        public string 当前状态 => _当前状态.Value;

        private string _动态推送1;
        public string 动态推送1
        {
            get => _动态推送1;
            set => this.RaiseAndSetIfChanged(ref _动态推送1, value);
        }

        private DateTime _数据更新时间;
        public DateTime 数据更新时间
        {
            get => _数据更新时间;
            set => this.RaiseAndSetIfChanged(ref _数据更新时间, value);
        }
        private readonly ObservableAsPropertyHelper<string> _数据更新时间文本;
        public string 数据更新时间文本 => _数据更新时间文本.Value;

        private int _默认音量;
        public int 默认音量
        {
            get => _默认音量;
            set => this.RaiseAndSetIfChanged(ref _默认音量, value);
        }
        public ReactiveCommand<int, Unit> 更新默认音量命令 { get; }

        public ReactiveCommand<string, Unit> 切换界面命令 { get; }

        public Dictionary<string, Grid> 所有层 { get; set; }

        private 直播间 _当前选中直播间;
        public 直播间 当前选中直播间
        {
            get => _当前选中直播间;
            set => this.RaiseAndSetIfChanged(ref _当前选中直播间, value);
        }
        private readonly ObservableAsPropertyHelper<string> _选中内容文本;
        public string 选中内容文本 => _选中内容文本.Value;
    }

    public class 直播间
    {
        public string 编号 { get; set; }
        public string 名称 { get; set; }
        public string 原名 { get; set; }
        public bool 是否直播中 { get; set; }
        public string 平台 { get; set; }
        public bool 是否提醒 { get; set; }
        public bool 是否录制 { get; set; }
        public string 唯一码 { get; set; }

        public 直播间(string 内容文本)
        {
            编号 = MMPU.获取livelist平台和唯一码.编号(内容文本);
            名称 = MMPU.获取livelist平台和唯一码.名称(内容文本);
            原名 = MMPU.获取livelist平台和唯一码.原名(内容文本);
            是否直播中 = MMPU.获取livelist平台和唯一码.状态(内容文本) == "●直播中" ? true : false;
            平台 = MMPU.获取livelist平台和唯一码.平台(内容文本);
            是否提醒 = MMPU.获取livelist平台和唯一码.是否提醒(内容文本) == "√" ? true : false;
            是否录制 = MMPU.获取livelist平台和唯一码.是否录制(内容文本) == "√" ? true : false;
            唯一码 = MMPU.获取livelist平台和唯一码.唯一码(内容文本);
        }
        public override string ToString()
        {
            return $"{{ 编号 = {编号}, 名称 = {名称}, 原名 = {原名}, 是否直播中 = {(是否直播中 ? "●直播中" : "○未直播")}, 平台 = {平台}, 是否提醒 = {(是否提醒 ? "√" : "")}, 是否录制 = {(是否录制 ? "√" : "")}, 唯一码 = {唯一码} }}";
        }
    }
}
