module.exports = {
  base: '/',
  locales: {
    '/': {
      lang: 'zh-CN',
      title: 'DDTV',
      keywords: 'DDTV bilibili bili live 播放器 录播机 B站录播机 bilibili自动录制',
      description: 'DDTV-你的地表最强B站播放器。一个可进行B站直播多窗口观看、开播提醒、自动录制、直播状态查看、跨平台部署的绿色工具。'
    }
  },
  head: [
    ['meta', { name: 'keywords', content: 'DDTV,bilibili,bili,live,播放器,录播机,B站录播机,bilibili自动录制' }]
  ],
  themeConfig: {
    algolia: {},
    logo: '/DDTV.png',
    repo: 'CHKZL/DDTV',
    docsDir: 'Doc/docs',
    docsBranch: 'master',
    editLinks: true,
    editLinkText: '在 GitHub 上编辑此页',
    lastUpdated: '上次更新',
    smoothScroll: true,
    nav: [
      { text: '主页', link: '/' },
      { text: '安装', link: '/install/' },
      { text: '配置说明', link: '/config/' },
      { text: '进阶功能', link: '/AdvancedFeatures/' },
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
            'DDTV_Desktop(Windows)',
            'DDTV_Server(Linux)',
            'DDTV_Server(Windows)',
            'DDTV_Client(Windows)', 
            'Docker',
            'WEBUI'
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
            'DDTV_Config',
            'RoomListConfig.json',
            'shell.md'
          ]
        }
      ], '/about/': [
        {
          title: '关于项目',
          collapsable: false,
          sidebar: 'auto',
          children: [
            '',
            '免责声明'
          ]
        }
      ],
      '/AdvancedFeatures/': [
        {
          title: '进阶功能',
          collapsable: false,
          sidebar: 'auto',
          children: [
            '',
            '弹幕录制',
            '时间轴修复',
            '自动转码',
            '自动切割',
            '文件合并',
            '邮件通知',
            'WebHook',
            '房间Shell命令',
            '观看时长心跳',
            'Debug模式',
            'API',
            'WebSocket服务器',
            'WEB服务器'
          ]
        }
      ],
      '/API/': [
        {
          title: 'API 文档',
          collapsable: false,
          sidebar: 'auto',
          children: [
            '',
            'API',
            'WEB'
          ]
        }
      ]
    }
  },
}
