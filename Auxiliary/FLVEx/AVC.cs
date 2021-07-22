using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Auxiliary.FLVEx
{
    static class AVC
    {
        public static bool Read(DataStream stream, ref int width, ref int height)
        {
            AVCPacketType avcType = (AVCPacketType)stream.ReadByte();

            // make sure, that this is header packet
            if (avcType != AVCPacketType.SequenceHeader)
                return false;

            // read composition time
            stream.Read24Int();

            AVCDecoderConfigurationRecord configuration = new AVCDecoderConfigurationRecord(stream);

            // do we have SPS?
            if ((configuration.NumOfSequenceParameterSets & 0x1F) == 0)
                return false;

            // read SPS size
            ushort spsSize = stream.ReadUShort();

            // read SPS itself
            byte[] spsBuffer = stream.ReadBytes(spsSize);

            ParseSPS(spsBuffer, out width, out height);

            return true;
        }

        private static int ExpGolumbUE(BitReader reader)
        {
            bool bit = reader.GetBit() != 0;
            byte significantBits = 0;

            while (!bit)
            {
                significantBits++;
                bit = reader.GetBit() != 0;
            }

            return (1 << significantBits) + reader.GetBits(significantBits) - 1;
        }

        private static int ExpGolumbSE(BitReader reader)
        {
            int result = ExpGolumbUE(reader);
            if ((result & 0x1) == 0)
                return -(result >> 1);
            else
                return (result + 1) >> 1;
        }

        private static void ParseScalingList(int size, BitReader reader)
        {
            int last_scale = 8;
            int next_scale = 8;
            uint delta_scale;
            for (int i = 0; i < size; i++)
            {
                if (next_scale != 0)
                {
                    delta_scale = (uint)ExpGolumbSE(reader);
                    next_scale = (int)((last_scale + delta_scale + 256) % 256);
                }
                if (next_scale != 0)
                    last_scale = next_scale;
            }
        }

        private static void ParseSPS(byte[] buffer, out int width, out int height)
        {
            BitReader reader = new BitReader(buffer);

            // skip first byte, since we already know we're parsing a SPS
            reader.SkipBits(8);

            // get profile
            int profile = reader.GetBits(8);

            // skip 4 bits + 4 zeroed bits + 8 bits = 16 bits = 2 bytes 
            reader.SkipBits(16);

            // read sps id, first exp-golomb encoded value
            ExpGolumbUE(reader);

            if (profile == 100 || profile == 110 || profile == 122 || profile == 144)
            {
                // chroma format idx 
                if (ExpGolumbUE(reader) == 3)
                {
                    reader.SkipBits(1);
                }
                // bit depth luma minus8 
                ExpGolumbUE(reader);
                // bit depth chroma minus8
                ExpGolumbUE(reader);
                // Qpprime Y Zero Transform Bypass flag
                reader.SkipBits(1);
                // Seq Scaling Matrix Present Flag
                if (reader.GetBit() != 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        // Seq Scaling List Present Flag
                        if (reader.GetBit() != 0)
                            ParseScalingList(i < 6 ? 16 : 64, reader);
                    }
                }
            }
            // log2_max_frame_num_minus4
            ExpGolumbUE(reader);
            // pic_order_cnt_type */
            int picOrderCntType = ExpGolumbUE(reader);
            if (picOrderCntType == 0)
            {
                // log2_max_pic_order_cnt_lsb_minus4
                ExpGolumbUE(reader);
            }
            else if (picOrderCntType == 1)
            {
                // delta_pic_order_always_zero_flag
                reader.SkipBits(1);
                // offset_for_non_ref_pic
                ExpGolumbSE(reader);
                // offset_for_top_to_bottom_field
                ExpGolumbSE(reader);
                int size = ExpGolumbUE(reader);
                for (int i = 0; i < size; i++)
                {
                    // offset_for_ref_frame
                    ExpGolumbSE(reader);
                }
            }
            // num_ref_frames
            ExpGolumbUE(reader);
            // gaps_in_frame_num_value_allowed_flag
            reader.SkipBits(1);
            // pic_width_in_mbs 
            int width_in_mbs = ExpGolumbUE(reader) + 1;
            // pic_height_in_map_units */
            int height_in_map_units = ExpGolumbUE(reader) + 1;
            // frame_mbs_only_flag */
            bool frame_mbs_only_flag = reader.GetBit() != 0;
            if (!frame_mbs_only_flag)
            {
                // mb_adaptive_frame_field
                reader.SkipBits(1);
            }
            // direct_8x8_inference_flag
            reader.SkipBits(1);
            // frame_cropping */
            int left = 0, right = 0, top = 0, bottom = 0;
            if (reader.GetBit() != 0)
            {
                left = ExpGolumbUE(reader) * 2;
                right = ExpGolumbUE(reader) * 2;
                top = ExpGolumbUE(reader) * 2;
                bottom = ExpGolumbUE(reader) * 2;
                if (!frame_mbs_only_flag)
                {
                    top *= 2;
                    bottom *= 2;
                }
            }
            // width
            width = width_in_mbs * 16 - (left + right);
            // height
            height = height_in_map_units * 16 - (top + bottom);
            if (!frame_mbs_only_flag)
                height *= 2;
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        private struct AVCDecoderConfigurationRecord
        {
            public byte ConfigurationVersion;
            public byte AVCProfileIndication;
            public byte Profile_compatibility;
            public byte AVCLevelIndication;
            public byte LengthSizeMinusOne;
            public byte NumOfSequenceParameterSets;

            public AVCDecoderConfigurationRecord(DataStream stream)
            {
                ConfigurationVersion = stream.ReadByte();
                AVCProfileIndication = stream.ReadByte();
                Profile_compatibility = stream.ReadByte();
                AVCLevelIndication = stream.ReadByte();
                LengthSizeMinusOne = stream.ReadByte();
                NumOfSequenceParameterSets = stream.ReadByte();
            }
        }
    }
}
