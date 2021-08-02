module.exports = {
  base: '/',
  locales: {
    '/': {
      lang: 'zh-CN',
      title: 'DDTV 你的地表最强B站录播机',
      keywords:'DDTV bilibili 录播机 B站录播机 bilibili自动录制',
      description: 'DDTV-你的地表最强B站录播机。一个可进行B站直播开播提醒.自动录制.在线播放、直播状态查看、跨平台部署的绿色工具。'
    }
  },
  themeConfig: {
    algolia: {},
    logo: '/DDTV.png',
    repo: 'CHKZL/DDTV2',
    docsDir: 'docs',
    docsBranch: 'dev',
    editLinks: true,
    editLinkText: '在 GitHub 上编辑此页',
    lastUpdated: '上次更新',
    smoothScroll: true,
    nav: [
      { text: '主页', link: '/' },
      { text: '安装', link: '/install/' },
      { text: '配置说明', link: '/config/' },
      { text: '进阶功能说明', link: '/AdvancedFeatures/' },
      { text: 'API', link: '/API/' },
      { text: '常见问题', link: '/QFA/' },
      { text: '更新日志', link: '/CHANGELOG/' },
      { text: '关于项目', link: '/about/' }
    ],
    sidebar: {
      '/install/': [
        {
          title: '安装',
          collapsable: false,
          sidebar: 'auto',
          children: [
            '',
            'DDTV',
            'DDTVLiveRecForWindows',
            'DDTVLiveRecForLinux',
            'DDTVLiveRecForMacOS',
            'Docker'
          ]
        }
      ],
      '/config/': [
        {
          title: '配置说明',
          collapsable: false,
          sidebar: 'auto',
          children: [
            '',
            'exe.config',
            'RoomListConfig.json',
          ]
        }
      ],
      '/AdvancedFeatures/': [
        {
          title: '进阶功能说明',
          collapsable: false,
          sidebar: 'auto',
          children: [
            '',
            '时间轴修复',
            '文件合并',
            '自动转码',
            '弹幕录制',
            'WEB服务器',
            '自动上传',
            'Debug模式',
          ]
        }
      ],
      '/API/': [
        {
          title: 'API',
          collapsable: false,
          sidebar: 'auto',
          children: [
            '',
            'WebHook'
          ]
        }
      ],
      '/CHANGELOG/': [
        {
          title: '更新日志',
          collapsable: false,
          sidebar: 'auto',
          children: [
            ''
          ]
        }
      ], '/about/': [
        {
          title: '关于项目',
          collapsable: false,
          sidebar: 'auto',
          children: [
            '',
            '免责声明',
            '隐私声明',
            '捐赠'
          ]
        }
      ]
    }
  },
}