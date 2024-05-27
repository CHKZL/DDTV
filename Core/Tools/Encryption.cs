using Masuit.Tools;
using SixLabors.ImageSharp.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tools
{
    public static class Encryption
    {

        #region 文件相关操作

        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="fileName">文件绝对路径</param>
        /// <returns>MD5值</returns>
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }

        /// <summary>
        /// 输入文本输出加密文件
        /// </summary>
        /// <param name="inputText">输入的文本</param>
        /// <param name="outputFile">输出的文件路径</param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="IsCreate">是否覆盖</param>
        public static void EncryptFile(string inputText, string outputFile)
        {
            string ACC = AesStr(inputText, Config.Core_RunConfig._Key, Config.Core_RunConfig._IV);
            File.WriteAllText(outputFile, ACC);
        }

        /// <summary>
        /// 输入加密文件输出解密文字
        /// </summary>
        /// <param name="inputFile">输入的文件路径</param>
        /// <param name="outputText">输出的文本</param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static void DecryptFile(string inputFile, out string outputText)
        {
            using (FileStream fsInput = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                string source = fsInput.ReadAllText(Encoding.UTF8);
                outputText = UnAesStr(source, Config.Core_RunConfig._Key, Config.Core_RunConfig._IV);
            }
        }


        #endregion
        #region MD5加解
        /// <summary>
        /// MD5加密
        /// </summary>
        public static string Md532(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return "";
            }
            Encoding encoding = Encoding.UTF8;
            MD5 md5 = MD5.Create();
            return HashAlgorithmBase(md5, source, encoding);
        }
        /// <summary>
        /// 加盐MD5加密
        /// </summary>
        public static string Md532Salt(this string source, string salt)
        {
            return string.IsNullOrEmpty(salt) ? source.Md532() : (source + "[" + salt + "]").Md532();
        }
        #endregion
        #region 16MD5
        /// <summary>
        /// 获取16位md5加密
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Get16MD5One(string source)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
                //转换成字符串，并取9到25位
                string sBuilder = BitConverter.ToString(data, 4, 8);
                //BitConverter转换出来的字符串会在每个字符中间产生一个分隔符，需要去除掉
                sBuilder = sBuilder.Replace("-", "");
                return sBuilder.ToString().ToUpper();
            }
        }
        #endregion
        #region AES 加密解密
        /// <summary>  
        /// AES加密  
        /// </summary>  
        /// <param name="source">待加密字段</param>  
        /// <param name="keyVal">密钥值</param>  
        /// <param name="ivVal">加密辅助向量</param>
        /// <returns></returns>  
        public static string AesStr(this string source, string keyVal = "rzqIzYmDQFqQmWfr", string ivVal = "itkIBBs5JdCLKqpP")
        {
            //return Base64(source);
            Encoding encoding = Encoding.UTF8;
            byte[] btKey = keyVal.FormatByte(encoding);
            byte[] btIv = ivVal.FormatByte(encoding);
            byte[] byteArray = encoding.GetBytes(source);
            string encrypt;
            Aes aes = Aes.Create();
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
        public static string UnAesStr(this string source, string keyVal = "rzqIzYmDQFqQmWfr", string ivVal = "itkIBBs5JdCLKqpP")
        {
            int len = source.Length % 4;
            if (len != 0)
            {
                string B1 = string.Empty;
                for (int i = 0; i < (4 - len); i++)
                {
                    B1 += "=";
                }
                source += B1;
            }
            //return UnBase64(source);
            Encoding encoding = Encoding.UTF8;
            byte[] btKey = keyVal.FormatByte(encoding);
            byte[] btIv = ivVal.FormatByte(encoding);
            byte[] byteArray = Convert.FromBase64String(source);
            string decrypt;
            Aes aes = Aes.Create();
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
            byte[] btArray = Encoding.UTF8.GetBytes(source);
            string BB = Convert.ToBase64String(btArray, 0, btArray.Length);
            return BB;
        }

        /// <summary>
        /// BASE64 解密
        /// </summary>
        /// <param name="source">待解密字段</param>
        /// <returns></returns>
        public static string UnBase64(this string source)
        {
            try
            {
                byte[] btArray = Convert.FromBase64String(source);
                return Encoding.UTF8.GetString(btArray);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion
        #region SHA1
        /// <summary>
        /// 对字符串进行SHA1加密
        /// </summary>
        /// <param name="strIN">需要加密的字符串</param>
        /// <returns>密文</returns>
        public static string SHA1_Encrypt(string Source_String)
        {
            byte[] StrRes = Encoding.UTF8.GetBytes(Source_String);
            HashAlgorithm iSHA = new SHA1CryptoServiceProvider();
            StrRes = iSHA.ComputeHash(StrRes);
            StringBuilder EnText = new StringBuilder();
            foreach (byte iByte in StrRes)
            {
                EnText.AppendFormat("{0:x2}", iByte);
            }
            return EnText.ToString().ToLower();
        }
        #endregion
        #region 内部方法

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
