using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_GUI.Tool
{
    public class Beep
    {
        //发出不同类型的声音的参数如下：  
        //Ok = 0x00000000,  
        //Error = 0x00000010,  
        //Question = 0x00000020,  
        //Warning = 0x00000030,  
        //Information = 0x00000040  
        [DllImport("user32.dll")]
        public static extern int MessageBeep(uint uType);
        public enum Type
        {
            Ok = 0x00000000,
            Error = 0x00000010,
            Question = 0x00000020,
            Warning = 0x00000030,
            Information = 0x00000040
        }

    }
}
