using System;
using System.Security.Cryptography;
using System.Text;

namespace Core.Tools
{
    /// <summary>
    /// B站直播观看心跳(x25Kn协议)的s签名计算工具：按secret_rule指定的算法顺序做链式HMAC，
    /// 每一步以上一步的小写hex输出作为下一步输入，secret_key始终作为HMAC的key。
    /// secret_rule索引与算法对应关系：0=HMAC-MD5 1=HMAC-SHA1 2=HMAC-SHA256 3=HMAC-SHA224 4=HMAC-SHA512 5=HMAC-SHA384
    /// </summary>
    public static class HmacChain
    {
        /// <summary>
        /// 计算链式HMAC签名，返回最终的小写hex字符串
        /// </summary>
        /// <param name="message">待签名内容（spyderData的JSON字符串）</param>
        /// <param name="key">HMAC密钥（secret_key）</param>
        /// <param name="rule">算法索引数组（secret_rule）</param>
        public static string Compute(string message, string key, int[] rule)
        {
            if (rule == null || rule.Length == 0)
            {
                throw new ArgumentException("secret_rule不能为空", nameof(rule));
            }
            byte[] keyBytes = Encoding.UTF8.GetBytes(key ?? string.Empty);
            byte[] data = Encoding.UTF8.GetBytes(message ?? string.Empty);
            string hex = string.Empty;
            foreach (int r in rule)
            {
                byte[] digest = r switch
                {
                    0 => HmacBytes(new HMACMD5(keyBytes), data),
                    1 => HmacBytes(new HMACSHA1(keyBytes), data),
                    2 => HmacBytes(new HMACSHA256(keyBytes), data),
                    3 => HmacSha224(keyBytes, data),
                    4 => HmacBytes(new HMACSHA512(keyBytes), data),
                    5 => HmacBytes(new HMACSHA384(keyBytes), data),
                    _ => throw new ArgumentOutOfRangeException(nameof(rule), $"未知的secret_rule索引:{r}")
                };
                hex = ToLowerHex(digest);
                //链式递推：上一步的hex字符串作为下一步的输入
                data = Encoding.UTF8.GetBytes(hex);
            }
            return hex;
        }

        private static byte[] HmacBytes(HMAC hmac, byte[] data)
        {
            using (hmac)
            {
                return hmac.ComputeHash(data);
            }
        }

        /// <summary>
        /// HMAC-SHA224。.NET BCL没有内置HMACSHA224，按RFC2104用托管SHA-224手动实现（块大小64字节）
        /// </summary>
        public static byte[] HmacSha224(byte[] key, byte[] message)
        {
            const int blockSize = 64;
            if (key.Length > blockSize)
            {
                key = Sha224.ComputeHash(key);
            }
            byte[] k = new byte[blockSize];
            Array.Copy(key, k, key.Length);

            byte[] inner = new byte[blockSize + message.Length];
            byte[] outerPad = new byte[blockSize];
            for (int i = 0; i < blockSize; i++)
            {
                inner[i] = (byte)(k[i] ^ 0x36);
                outerPad[i] = (byte)(k[i] ^ 0x5c);
            }
            Array.Copy(message, 0, inner, blockSize, message.Length);
            byte[] innerHash = Sha224.ComputeHash(inner);

            byte[] outer = new byte[blockSize + innerHash.Length];
            Array.Copy(outerPad, outer, blockSize);
            Array.Copy(innerHash, 0, outer, blockSize, innerHash.Length);
            return Sha224.ComputeHash(outer);
        }

        private static string ToLowerHex(byte[] bytes)
        {
            char[] chars = new char[bytes.Length * 2];
            const string hexDigits = "0123456789abcdef";
            for (int i = 0; i < bytes.Length; i++)
            {
                chars[i * 2] = hexDigits[bytes[i] >> 4];
                chars[i * 2 + 1] = hexDigits[bytes[i] & 0xF];
            }
            return new string(chars);
        }
    }

    /// <summary>
    /// 托管SHA-224实现。.NET未提供SHA-224，SHA-224与SHA-256共用压缩函数，仅初始向量和输出长度(28字节)不同
    /// </summary>
    public static class Sha224
    {
        private static readonly uint[] K = new uint[]
        {
            0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
            0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
            0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
            0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
            0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
            0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
            0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
            0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
        };

        public static byte[] ComputeHash(byte[] data)
        {
            //SHA-224初始向量
            uint h0 = 0xc1059ed8, h1 = 0x367cd507, h2 = 0x3070dd17, h3 = 0xf70e5939,
                 h4 = 0xffc00b31, h5 = 0x68581511, h6 = 0x64f98fa7, h7 = 0xbefa4fa4;

            //消息填充：原消息 + 0x80 + 0填充至56(mod 64) + 8字节大端bit长度
            ulong bitLen = (ulong)data.LongLength * 8;
            int padLen = (56 - (data.Length + 1) % 64 + 64) % 64;
            byte[] padded = new byte[data.Length + 1 + padLen + 8];
            Array.Copy(data, padded, data.Length);
            padded[data.Length] = 0x80;
            for (int i = 0; i < 8; i++)
            {
                padded[padded.Length - 1 - i] = (byte)(bitLen >> (8 * i));
            }

            uint[] w = new uint[64];
            for (int offset = 0; offset < padded.Length; offset += 64)
            {
                for (int t = 0; t < 16; t++)
                {
                    w[t] = (uint)(padded[offset + t * 4] << 24 | padded[offset + t * 4 + 1] << 16 | padded[offset + t * 4 + 2] << 8 | padded[offset + t * 4 + 3]);
                }
                for (int t = 16; t < 64; t++)
                {
                    uint s0 = RotR(w[t - 15], 7) ^ RotR(w[t - 15], 18) ^ (w[t - 15] >> 3);
                    uint s1 = RotR(w[t - 2], 17) ^ RotR(w[t - 2], 19) ^ (w[t - 2] >> 10);
                    w[t] = w[t - 16] + s0 + w[t - 7] + s1;
                }

                uint a = h0, b = h1, c = h2, d = h3, e = h4, f = h5, g = h6, h = h7;
                for (int t = 0; t < 64; t++)
                {
                    uint S1 = RotR(e, 6) ^ RotR(e, 11) ^ RotR(e, 25);
                    uint ch = (e & f) ^ (~e & g);
                    uint temp1 = h + S1 + ch + K[t] + w[t];
                    uint S0 = RotR(a, 2) ^ RotR(a, 13) ^ RotR(a, 22);
                    uint maj = (a & b) ^ (a & c) ^ (b & c);
                    uint temp2 = S0 + maj;
                    h = g; g = f; f = e; e = d + temp1;
                    d = c; c = b; b = a; a = temp1 + temp2;
                }
                h0 += a; h1 += b; h2 += c; h3 += d; h4 += e; h5 += f; h6 += g; h7 += h;
            }

            //SHA-224只输出前7个字(28字节)
            byte[] result = new byte[28];
            uint[] hs = { h0, h1, h2, h3, h4, h5, h6 };
            for (int i = 0; i < 7; i++)
            {
                result[i * 4] = (byte)(hs[i] >> 24);
                result[i * 4 + 1] = (byte)(hs[i] >> 16);
                result[i * 4 + 2] = (byte)(hs[i] >> 8);
                result[i * 4 + 3] = (byte)hs[i];
            }
            return result;
        }

        private static uint RotR(uint x, int n) => (x >> n) | (x << (32 - n));
    }
}
