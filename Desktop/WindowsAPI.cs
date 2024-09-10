using System.Runtime.InteropServices;

namespace Desktop
{
    public class WindowsAPI
    {
        // 导入SetThreadExecutionState函数
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(uint esFlags);

        // 定义执行状态标志
        const uint ES_CONTINUOUS = 0x80000000;
        const uint ES_SYSTEM_REQUIRED = 0x00000001;
        const uint ES_DISPLAY_REQUIRED = 0x00000002;
        /// <summary>
        /// 恢复系统休眠
        /// </summary>
        public static void OpenWindowsHibernation()
        {
            try
            {
                // 恢复系统休眠
                SetThreadExecutionState(ES_CONTINUOUS);
            }
            catch (Exception)
            {

            }
        }
        /// <summary>
        /// 阻止系统休眠
        /// </summary>
        public static void CloseWindowsHibernation()
        {
            try
            {
                // 阻止系统休眠
                SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
            }
            catch (Exception)
            {

            }
        }
    }
}
