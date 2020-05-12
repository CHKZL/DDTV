using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DDTV_New.Utility
{
    public static class NewThreadTask
    {
        public static void Run(Action<Action<Action>> newThreadTask, Window window)
        {
            new Task(() =>
            {
                _doRun(newThreadTask, window);
            }).Start();
        }

        private static void _doRun(Action<Action<Action>> newThreadTask, Window window)
        {
            try
            {
                if (newThreadTask != null) newThreadTask.Invoke((localThreadTask) =>
                {
                    window.Dispatcher.Invoke(localThreadTask);
                });
            }
            catch (Exception) { }
        }

        public static void Run(Action newThreadTask)
        {
            new Task(() =>
            {
                _doRun(newThreadTask);
            }).Start();
        }
        private static void _doRun(Action newThreadTask)
        {
            try
            {
                if (newThreadTask != null) newThreadTask.Invoke();
            }
            catch (Exception) { }
        }

        public static void Loop(Action<Action<Action>> newThreadTask, Window window, int period)
        {
            new Task(() =>
            {
                while (true)
                {
                    _doRun(newThreadTask, window);
                    Thread.Sleep(period);
                }
            }).Start();
        }

        public static void Loop(Action newThreadTask, int period)
        {
            new Task(() =>
            {
                while (true)
                {
                    _doRun(newThreadTask);
                    Thread.Sleep(period);
                }
            }).Start();
        }
    }
}
