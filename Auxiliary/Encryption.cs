using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    public static class Encryption
    {
        #region AES 加密解密

        /// <summary>  
        /// AES加密  
        /// </summary>  
        /// <param name="source">待加密字段</param>  
        /// <param name="keyVal">密钥值</param>  
        /// <param name="ivVal">加密辅助向量</param>
        /// <returns></returns>  
        public static string AesStr(this string source, string keyVal, string ivVal)
        {
            var encoding = Encoding.UTF8;
            byte[] btKey = keyVal.FormatByte(encoding);
            byte[] btIv = ivVal.FormatByte(encoding);
            byte[] byteArray = encoding.GetBytes(source);
            string encrypt;
            Rijndael aes = Rijndael.Create();
            using (MemoryStream mStream = new MemoryStream())
            {
                using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateEncryptor(btKey, btIv), CryptoStreamMode.Write))
                {
                    cStream.Write(byteArray, 0, byteArray.Length);
                    cStream.FlushFinalBlock();
                    encrypt = Convert.ToBase64String(mStream.ToArray());
                }
            }
            aes.Clear();
            return encrypt;
        }

        /// <summary>  
        /// AES解密  
        /// </summary>  
        /// <param name="source">待加密字段</param>  
        /// <param name="keyVal">密钥值</param>  
        /// <param name="ivVal">加密辅助向量</param>  
        /// <returns></returns>  
        public static string UnAesStr(this string source, string keyVal, string ivVal)
        {
            var encoding = Encoding.UTF8;
            byte[] btKey = keyVal.FormatByte(encoding);
            byte[] btIv = ivVal.FormatByte(encoding);
            byte[] byteArray = Convert.FromBase64String(source);
            string decrypt;
            Rijndael aes = Rijndael.Create();
            using (MemoryStream mStream = new MemoryStream())
            {
                using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateDecryptor(btKey, btIv), CryptoStreamMode.Write))
                {
                    cStream.Write(byteArray, 0, byteArray.Length);
                    cStream.FlushFinalBlock();
                    decrypt = encoding.GetString(mStream.ToArray());
                }
            }
            aes.Clear();
            return decrypt;
        }
        #endregion
        #region BASE64 加密解密

        /// <summary>
        /// BASE64 加密
        /// </summary>
        /// <param name="source">待加密字段</param>
        /// <returns></returns>
        public static string Base64(this string source)
        {
            var btArray = Encoding.UTF8.GetBytes(source);
            return Convert.ToBase64String(btArray, 0, btArray.Length);
        }

        /// <summary>
        /// BASE64 解密
        /// </summary>
        /// <param name="source">待解密字段</param>
        /// <returns></returns>
        public static string UnBase64(this string source)
        {
            var btArray = Convert.FromBase64String(source);
            return Encoding.UTF8.GetString(btArray);
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 转成数组
        /// </summary>
        private static byte[] Str2Bytes(this string source)
        {
            source = source.Replace(" ", "");
            byte[] buffer = new byte[source.Length / 2];
            for (int i = 0; i < source.Length; i += 2) buffer[i / 2] = Convert.ToByte(source.Substring(i, 2), 16);
            return buffer;
        }

        /// <summary>
        /// 转换成字符串
        /// </summary>
        private static string Bytes2Str(this IEnumerable<byte> source, string formatStr = "{0:X2}")
        {
            StringBuilder pwd = new StringBuilder();
            foreach (byte btStr in source) { pwd.AppendFormat(formatStr, btStr); }
            return pwd.ToString();
        }

        private static byte[] FormatByte(this string strVal, Encoding encoding)
        {
            return encoding.GetBytes(strVal.Base64().Substring(0, 16).ToUpper());
        }

        /// <summary>
        /// HashAlgorithm 加密统一方法
        /// </summary>
        private static string HashAlgorithmBase(HashAlgorithm hashAlgorithmObj, string source, Encoding encoding)
        {
            byte[] btStr = encoding.GetBytes(source);
            byte[] hashStr = hashAlgorithmObj.ComputeHash(btStr);
            return hashStr.Bytes2Str();
        }

        #endregion
    }
}
