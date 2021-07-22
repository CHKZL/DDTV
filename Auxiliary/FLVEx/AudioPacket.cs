using System;
using System.Collections.Generic;
using System.Text;

namespace Auxiliary.FLVEx
{
    public class AudioPacket : StreamCopyPacket
    {
        public SoundFormat SoundFormat { get; }
        public SoundRate SoundRate { get; }
        public SoundSize SoundSize { get; }
        public SoundType SoundType { get; }

        public double GetSampleRate()
        {
            switch (SoundRate)
            {
                case SoundRate.kHz55:
                    return 5500;
                case SoundRate.kHz11:
                    return 11000;
                case SoundRate.kHz22:
                    return 22050;
                case SoundRate.kHz44:
                    return 44100;
                default:
                    throw new InvalidOperationException("Invalid Sound Rate");
            }
        }

        public double GetSoundSize()
        {
            switch (SoundSize)
            {
                case SoundSize.Bit8:
                    return 8;
                case SoundSize.Bit16:
                    return 16;
                default:
                    throw new InvalidOperationException("Invalid Sound Size");
            }
        }

        public bool GetStereo()
        {
            return SoundType == SoundType.Stereo;
        }

        public double GetSoundFormat()
        {
            return (double)SoundFormat;
        }

        internal AudioPacket(DataStream stream, uint prevPacketSize, PacketType type) : base(stream, prevPacketSize, type)
        {
            byte b = stream.ReadByte();

            SoundFormat = (SoundFormat)((b >> 4) & 0xF);
            SoundRate = (SoundRate)((b >> 2) & 0x3);
            SoundSize = (SoundSize)((b >> 1) & 0x1);
            SoundType = (SoundType)((b >> 0) & 0x1);

            SkipPayload(stream);
        }

        public override string ToString()
        {
            return $"{base.ToString()}: {SoundFormat} {SoundRate} {SoundSize} {SoundType}";
        }
    }

    public enum SoundFormat
    {
        LinearPCM_Platform,
        ADPCM,
        MP3,
        LinearPCM_LE,
        Nellymoser16kHzMono,
        Nellymoser8kHzMono,
        Nellymoser,
        G711_ALaw,
        G711_MuLaw,
        Reserved,
        AAC,
        Speex,
        Mp38kHz,
        DeviceSpecific
    }

    public enum SoundRate
    {
        /// <summary>
        /// 5.5kHz
        /// </summary>
        kHz55,
        /// <summary>
        /// 11kHz
        /// </summary>
        kHz11,
        /// <summary>
        /// 22kHz
        /// </summary>
        kHz22,
        /// <summary>
        /// 44kHz
        /// </summary>
        kHz44,
    }

    public enum SoundSize
    {
        Bit8,
        Bit16,
    }

    public enum SoundType
    {
        Mono,
        Stereo,
    }
}
