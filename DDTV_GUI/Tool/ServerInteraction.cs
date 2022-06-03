using DDTV_Core.Tool.ServerMessageClass;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DDTV_GUI.Tool
{
    public class ServerInteraction
    {
        public static void Start()
        {
            CheckUpdates.Update();
            Notice.Start();
            //Dokidoki.Start();
            
        }
        public class CheckUpdates
        {
            public static event EventHandler<EventArgs> NewUpdate;
           
            /// <summary>
            /// 间隔时间(ms)
            /// </summary>
            public static int ReminderInterval = 60 * 60 * 1 * 1000;
            private static bool IsChecked = false;
            private static bool Is = false;
            public static void Update()
            {
                if (!Is)
                {
                    Task.Run(() =>
                    {
                        Is = true;
                        while (true)
                        {
                            try
                            {
                                if (IsChecked)
                                {
                                    Thread.Sleep(ReminderInterval);
                                    if (NewUpdate != null)
                                    {
                                        NewUpdate.Invoke(null, EventArgs.Empty);
                                    }
                                    Console.WriteLine("检测到新版本，请退出本程序后运行目录中的[DDTV_Update]进行更新\n");
                                }
                                else
                                {
                                    if (DDTV_Core.Tool.DDTV_Update.ComparisonVersion("GUI", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()))
                                    {
                                        IsChecked = true;
                                        if (NewUpdate != null)
                                        {
                                            NewUpdate.Invoke(null, EventArgs.Empty);
                                        }
                                    }
                                    
                                }
                            }
                            catch (Exception)
                            {
                            }
                            Thread.Sleep(60 * 10 * 1000);
                        }
                    });
                }
            }
        }
        public class Notice
        {
            public static event EventHandler<EventArgs> NewNotice;
            private static bool Is = false;
            public static void Start()
            {
                if (!Is)
                {
                    Is = true;
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                string N = DDTV_Core.Tool.Notice.GetNotice("GUI");
                                MessageBase.pack<MessageClass.NoticeText> pack = JsonConvert.DeserializeObject<MessageBase.pack<MessageClass.NoticeText>>(N);
                                if (NewNotice != null)
                                {
                                    NewNotice.Invoke(pack.data.Text, EventArgs.Empty);
                                }
                                
                            }
                            catch (Exception)
                            {

                            }
                            Thread.Sleep(3600 * 1000);
                        }
                    });
                }
            }
        }
    }
}
