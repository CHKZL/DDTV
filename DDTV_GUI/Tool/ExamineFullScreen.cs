using DDTV_Core.SystemAssembly.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_GUI.Tool
{
    public class ExamineFullScreen
    {
        [StructLayout(LayoutKind.Sequential)] 
        public struct RECT 
        {
            public int left; 
            public int top;
            public int right;
            public int bottom; 
        }
        [DllImport("user32.dll", SetLastError = true)] 
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId); [DllImport("user32.dll")] 
        private static extern bool GetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect);
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow(); 
        public static bool IsForegroundFullScreen() 
        {
            return IsForegroundFullScreen(null); 
        }
        /// <summary>
        /// 检测当前是否有全屏程序
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        public static bool IsForegroundFullScreen(System.Windows.Forms.Screen screen)
        {
            if (screen == null)
            {
                screen = System.Windows.Forms.Screen.PrimaryScreen;
            }
            RECT rect = new RECT(); IntPtr hWnd = (IntPtr)GetForegroundWindow(); GetWindowRect(new HandleRef(null, hWnd), ref rect);
            if (screen.Bounds.Width == (rect.right - rect.left) && screen.Bounds.Height == (rect.bottom - rect.top))
            {
                //Log.AddLog("ExamineFullScreen", LogClass.LogType.Debug,"√有全屏任务运行");
                return true;
            }
            else
            {
                //Log.AddLog("ExamineFullScreen", LogClass.LogType.Debug, "X无全屏任务运行");
                return false;
            }
        }
    }
}
