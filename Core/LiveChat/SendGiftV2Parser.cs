using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace Core.LiveChat
{
    /// <summary>
    /// SEND_GIFT_V2(protobuf)礼物消息解析器
    /// 2026-07起B站灰度上线SEND_GIFT_V2，核心字段塞在 data.pb(base64+protobuf) 中。
    /// 此处将其解码并映射为旧版 SEND_GIFT 的 data 结构，保证下游(SendGiftEventArgs等)无需改动。
    /// proto结构为逆向推断，未确认字段不输出，消费端按存在性保护处理。
    /// </summary>
    public static class SendGiftV2Parser
    {
        /// <summary>
        /// 将 SEND_GIFT_V2 消息中的 pb 字段解码为旧版 SEND_GIFT 的 data 对象
        /// </summary>
        /// <param name="obj">完整的 SEND_GIFT_V2 消息 JsonObject</param>
        /// <returns>旧版 SEND_GIFT 格式的 data JsonObject；解析失败返回 null</returns>
        public static JsonObject DecodeToLegacyGiftData(JsonObject obj)
        {
            // 兼容 data.pb 与 data.data.pb 两种位置
            string pbBase64 = obj?["data"]?["pb"]?.GetValue<string>()
                              ?? obj?["data"]?["data"]?["pb"]?.GetValue<string>();
            if (string.IsNullOrEmpty(pbBase64))
            {
                return null;
            }

            byte[] pbBytes = Convert.FromBase64String(pbBase64);
            var root = PbMessage.Parse(pbBytes);

            // SendGiftV2: 1 uid / 2 uname / 3 face / 8 medal / 9 blind / 10 gift / 15 sender
            long uid = (long)root.GetVarint(1);
            string uname = root.GetString(2);
            string face = root.GetString(3);

            var gift = root.GetMessage(10);
            var medal = root.GetMessage(8);
            var blind = root.GetMessage(9);
            var receiver = gift?.GetMessage(29);

            var data = new JsonObject
            {
                ["uid"] = uid,
                ["uname"] = uname ?? string.Empty,
                ["face"] = face ?? string.Empty,
                // GiftData: 1 gift_id / 2 gift_name / 3 num / 4 gift_type / 5 price / 6 total_coin / 8 coin_type / 9 tid / 10 timestamp / 12 rnd / 18 action
                ["giftId"] = (int)(gift?.GetVarint(1) ?? 0),
                ["giftName"] = gift?.GetString(2) ?? string.Empty,
                ["num"] = (int)(gift?.GetVarint(3) ?? 0),
                ["giftType"] = (int)(gift?.GetVarint(4) ?? 0),
                ["price"] = (float)(gift?.GetVarint(5) ?? 0),
                ["total_coin"] = (int)(gift?.GetVarint(6) ?? 0),
                ["coin_type"] = gift?.GetString(8) ?? string.Empty,
                ["tid"] = gift?.GetString(9) ?? string.Empty,
                ["timestamp"] = (long)(gift?.GetVarint(10) ?? 0),
                ["rnd"] = gift?.GetString(12) ?? string.Empty,
                ["action"] = gift?.GetString(18) ?? string.Empty,
                // MedalInfo: 11 guard_level(推断) —— 旧版 guard_level 为送礼用户大航海等级
                ["guard_level"] = (int)(medal?.GetVarint(11) ?? 0),
            };

            // 粉丝勋章信息(颜色为十进制整数，与 sender_uinfo 中的十六进制串表示不同，勿混用)
            if (medal != null)
            {
                data["medal_info"] = new JsonObject
                {
                    ["anchor_uid"] = (long)medal.GetVarint(1),
                    ["medal_level"] = (int)medal.GetVarint(5),
                    ["medal_name"] = medal.GetString(6) ?? string.Empty,
                    ["color_start"] = (long)medal.GetVarint(7),
                    ["color"] = (long)medal.GetVarint(8),
                    ["color_border"] = (long)medal.GetVarint(9),
                    ["color_end"] = (long)medal.GetVarint(10),
                    ["guard_level"] = (int)medal.GetVarint(11),
                };
            }

            // 盲盒信息: gift_name 为实际爆出的礼物，total_coin 为盲盒购买价(实付)
            if (blind != null)
            {
                data["blind_gift"] = new JsonObject
                {
                    ["gift_action"] = (int)blind.GetVarint(1),
                    ["original_gift_id"] = (int)blind.GetVarint(2),
                    ["original_gift_name"] = blind.GetString(3) ?? string.Empty,
                    ["action"] = blind.GetString(5) ?? string.Empty,
                    ["blind_price"] = (int)blind.GetVarint(6),
                };
            }

            // 收礼主播
            if (receiver != null)
            {
                data["receiver"] = new JsonObject
                {
                    ["uname"] = receiver.GetString(1) ?? string.Empty,
                    ["uid"] = (long)receiver.GetVarint(2),
                };
            }

            return data;
        }

        #region protobuf wire format 解析
        /// <summary>
        /// 极简 protobuf wire format 解析器，仅支持 varint/fixed32/fixed64/length-delimited
        /// </summary>
        private class PbMessage
        {
            private readonly List<(int Field, int WireType, ulong Varint, byte[] Bytes)> _fields = new();

            public static PbMessage Parse(byte[] data)
            {
                var msg = new PbMessage();
                int pos = 0;
                while (pos < data.Length)
                {
                    ulong key = ReadVarint(data, ref pos);
                    int field = (int)(key >> 3);
                    int wireType = (int)(key & 0x7);
                    switch (wireType)
                    {
                        case 0: // varint
                            msg._fields.Add((field, wireType, ReadVarint(data, ref pos), null));
                            break;
                        case 1: // fixed64
                            msg._fields.Add((field, wireType, BitConverter.ToUInt64(data, pos), null));
                            pos += 8;
                            break;
                        case 2: // length-delimited
                            int len = (int)ReadVarint(data, ref pos);
                            byte[] bytes = new byte[len];
                            Array.Copy(data, pos, bytes, 0, len);
                            pos += len;
                            msg._fields.Add((field, wireType, 0, bytes));
                            break;
                        case 5: // fixed32
                            msg._fields.Add((field, wireType, BitConverter.ToUInt32(data, pos), null));
                            pos += 4;
                            break;
                        default: // 3/4(组，已废弃)等未知类型直接终止，避免死循环
                            return msg;
                    }
                }
                return msg;
            }

            public ulong GetVarint(int field)
            {
                foreach (var f in _fields)
                {
                    if (f.Field == field && f.WireType == 0)
                    {
                        return f.Varint;
                    }
                }
                return 0;
            }

            public string GetString(int field)
            {
                foreach (var f in _fields)
                {
                    if (f.Field == field && f.WireType == 2 && f.Bytes != null)
                    {
                        return Encoding.UTF8.GetString(f.Bytes);
                    }
                }
                return null;
            }

            public PbMessage GetMessage(int field)
            {
                foreach (var f in _fields)
                {
                    if (f.Field == field && f.WireType == 2 && f.Bytes != null)
                    {
                        try
                        {
                            return Parse(f.Bytes);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                }
                return null;
            }

            private static ulong ReadVarint(byte[] data, ref int pos)
            {
                ulong result = 0;
                int shift = 0;
                while (pos < data.Length && shift < 64)
                {
                    byte b = data[pos++];
                    result |= (ulong)(b & 0x7F) << shift;
                    if ((b & 0x80) == 0)
                    {
                        return result;
                    }
                    shift += 7;
                }
                return result;
            }
        }
        #endregion
    }
}
