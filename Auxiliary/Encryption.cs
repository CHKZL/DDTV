using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Auxiliary
{
    public static class Encryption
    {
        /// <summary>
        /// MD5加密
        /// </summary>
        public static string Md532(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }
            var encoding = Encoding.UTF8;
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
        public static string AesStr(this string source, string keyVal, string ivVal)
        {
            //return Base64(source);
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
            int len = source.Length % 4;
            if(len!=0)
            {
                string B1 = string.Empty;
                for (int i = 0; i < (4- len); i++)
                {
                    B1 += "=";
                }
                source += B1;
            }
            //return UnBase64(source);
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
                var btArray = Convert.FromBase64String(source);
                return Encoding.UTF8.GetString(btArray);
            }
            catch (Exception e)
            {
                return null;
            }
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

        #region 机器码(仅用于区分设备使用，不会储存，而且加盐单向加密)
        public class 机器码
        {
            // 取得设备硬盘的卷标号
            private static string 取得设备硬盘的卷标号()
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"c:\"");
                disk.Get();
                return disk.GetPropertyValue("VolumeSerialNumber").ToString();
            }
            //获得CPU的序列号
            private static string 获得CPU的序列号()
            {
                string strCpu = null;
                ManagementClass myCpu = new ManagementClass("win32_Processor");
                ManagementObjectCollection myCpuConnection = myCpu.GetInstances();
                foreach (ManagementObject myObject in myCpuConnection)
                {
                    strCpu = myObject.Properties["Processorid"].Value.ToString();
                    break;
                }
                return strCpu;
            }
            public static string 获取机器码(string name)
            {
                string code = 获得CPU的序列号() +"|"+ 取得设备硬盘的卷标号()+"|"+name;
                code = Md532Salt(code, "DDTV");
                return Get16MD5One(code);
               // return code+"|"+ name;
            }  
        }    
        #endregion

    }
}
