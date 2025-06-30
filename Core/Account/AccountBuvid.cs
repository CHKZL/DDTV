using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Account
{
    internal class AccountBuvid
    {
        private static readonly Random RandomGenerator = new Random();
        private const string HexDigits = "0123456789ABCDEF";
        private const int TimestampDigits = 5;
        private const string FixedSuffix = "infoc";

        public static string CreateUniqueIdentifier()
        {
            var identifierParts = new List<string>
            {
                GenerateRandomHexSegment(8),
                GenerateRandomHexSegment(4),
                GenerateRandomHexSegment(4),
                GenerateRandomHexSegment(4),
                GenerateRandomHexSegment(12)
            };

            string timeComponent = GetTruncatedTimestamp();

            return string.Join("-", identifierParts) + timeComponent + FixedSuffix;
        }

        private static string GenerateRandomHexSegment(int length)
        {
            var segment = new char[length];
            for (int i = 0; i < length; i++)
            {
                segment[i] = HexDigits[RandomGenerator.Next(HexDigits.Length)];
            }
            return new string(segment);
        }

        private static string GetTruncatedTimestamp()
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return (currentTime % 100000).ToString($"D{TimestampDigits}");
        }
    }
}
