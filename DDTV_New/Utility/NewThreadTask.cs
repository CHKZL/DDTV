using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DDTV_New.Utility
{
    /// <summary>
    /// 在新线程运行指定代码的包装函数类
    /// </summary>
    public static class NewThreadTask
    {
        /// <summary>
        /// 在新线程运行指定代码。<br/>
        /// 运行的代码需要在原始线程运行其中一部分。<br/>
        /// <example>
        /// 使用方法：
        /// <code>
        /// NewThreadTask.Run(runOnLocalThread => <br/>
        /// {<br/>
        ///     [此处代码在新线程运行]<br/>
        ///     runOnLocalThread(() => { [此处代码在原始线程运行] });<br/>
        /// }, [线程所在的窗口Window对象，一般为this]);<br/>
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="newThreadTask">新线程中运行的函数</param>
        /// <param name="window">原始窗口线程</param>
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

        /// <summary>
        /// 在新线程运行指定代码。<br/>
        /// 运行的代码独立于原始线程。<br/>
        /// <example>
        /// 使用方法：
        /// <code>
        /// NewThreadTask.Run(() => <br/>
        /// {<br/>
        ///     [此处代码在新线程运行]<br/>
        /// });<br/>
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="newThreadTask">新线程中运行的函数</param>
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

        /// <summary>
        /// 在新线程每隔指定时间运行一次代码。<br/>
        /// 运行的代码需要在原始线程运行其中一部分。<br/>
        /// <example>
        /// 使用方法：
        /// <code>
        /// NewThreadTask.Loop(runOnLocalThread => <br/>
        /// {<br/>
        ///     [此处代码在新线程运行]<br/>
        ///     runOnLocalThread(() => { [此处代码在原始线程运行] });<br/>
        /// }, [线程所在的窗口Window对象，一般为this], [每隔此时间运行一次(单位：毫秒)]);<br/>
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="newThreadTask">新线程中运行的函数</param>
        /// <param name="window">原始窗口线程</param>
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

        /// <summary>
        /// 在新线程每隔指定时间运行一次代码。<br/>
        /// 运行的代码独立于原始线程。<br/>
        /// <example>
        /// 使用方法：
        /// <code>
        /// NewThreadTask.Loop(() => <br/>
        /// {<br/>
        ///     [此处代码在新线程运行]<br/>
        /// }, [每隔此时间运行一次(单位：毫秒)]);<br/>
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="newThreadTask">新线程中运行的函数</param>
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
