using System;
using System.Collections.Generic;
using System.Text;

namespace DDTV_Core.Tool.Flv.FLVEx
{
    class BitReader
    {
        private byte[] data;
        private int position = 0;
        private int bitPos = 0;

        public BitReader(byte[] data)
        {
            this.data = data;
        }

        public int Position
        {
            get { return position; }
            set
            {
                bitPos = 8;
                position = value;
            }
        }

        public void SkipBits(int nbits)
        {
            position += (nbits + bitPos) / 8;
            bitPos = (bitPos + nbits) % 8;
        }

        public byte GetBit()
        {
            byte result = (byte)((data[position] >> (7 - bitPos)) & 0x1);
            if (bitPos == 7)
            {
                bitPos = 0;
                position++;
            }
            else
                bitPos++;

            return result;
        }

        public int GetBits(int nbits)
        {
            int result = 0;
            for (int i = 0; i < nbits; i++)
                result = (result << 1) + GetBit();

            return result;
        }
    }
}
