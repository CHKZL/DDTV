using System;

namespace DDTV_Core.SystemAssembly.BilibiliModule.API.LiveChatScript
{
    internal static class BEBitConverter
    {
        internal static int ToInt32(byte[] arr, int offset)
        {
            int value = BitConverter.ToInt32(arr, offset);
            unsafe { Extensions.SwapBytes((byte*)&value, 4); }
            return value;
        }

        internal static ushort ToUInt16(byte[] arr,int offset)
        {
            ushort value = BitConverter.ToUInt16(arr, offset); 
            unsafe { Extensions.SwapBytes((byte*)&value, 2); }
            return value;
        }
    }
}
