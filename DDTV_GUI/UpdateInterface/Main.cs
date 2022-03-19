using DDTV_GUI.DDTV_Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DDTV_Core.SystemAssembly.BilibiliModule.Rooms;
using DDTV_Core.SystemAssembly.NetworkRequestModule;

namespace DDTV_GUI.UpdateInterface
{
    internal class Main
    {
        public enum Interface
        {
            HomePage,
            LiveListPage,
            RecListPage,
            OverListPage,
            OtherPage,
            SetUpPage,
            AboutPage
        }
        private static int activationinterface = -1;
        private static int previouspage = 0;
        public static int ActivationInterface
        {
            get
            {
                return activationinterface;
            }
            set
            {
                previouspage = activationinterface;
                activationinterface = value;
            }
        }
        public static int PreviousPage
        {
            get { return previouspage; }
        }
        public class ContextMenuState
        {
            public static bool LiveListPage = false;
            public static bool RecListPage = false;
            public static bool OverListPage = false;
        }
        public static List<BindingData.LiveList> liveList = new();
        public static List<BindingData.RecList> recList = new();
        public static List<BindingData.OverList> overList = new();
        public static void update(MainWindow mainWindow)
        {
            Task.Run(() =>
            {
               
                while (true)
                {
                    try
                    {
                        switch (activationinterface)
                        {
                            case (int)Interface.HomePage:
                                {
                                    HomePage.Update(mainWindow);
                                    Thread.Sleep(500);
                                    break;
                                }
                            case (int)Interface.LiveListPage:
                                {
                                    liveList = LiveListPage.Update(liveList, mainWindow);
                                    Thread.Sleep(1*1000);
                                    break;
                                }
                            case (int)Interface.RecListPage:
                                {
                                    recList = RecListPage.Update(recList, mainWindow);
                                    Thread.Sleep(1 * 1000);
                                    break;
                                }
                            case (int)Interface.OverListPage:
                                {
                                    overList = OverListPage.Update(overList, mainWindow);
                                    Thread.Sleep(1 * 1000);
                                    break;
                                }
                                default:
                                Thread.Sleep(10);
                                break;
                        }

                    }
                    catch (Exception)
                    {
                    }
                }
            });
        }
    }
}
