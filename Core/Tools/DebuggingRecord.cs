using Core.LogModule;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tools
{

    public class DebuggingRecord
    {
        private static string publicKey = "MIIBCgKCAQEAvVfJfvg0QbH0dJtocE+Cm2RWyT1/hyTm2lebHljVghEzfI0BjiXspLyZBZojqRyeLqK+7A1KRnRJjSbE+pRdv7jkiEY6LjmjPCQBgFsJ0TgW2N4lxm0j4uTV1YxsnYNNslYW88Mv0kwTMfEtoas19wEeKFsjLJPS8qwpjnIX5buI8zDqyk4by+7xMdBFT12c9Jrp4AAizvrRIUVMbB95/YBZ16UjvMdCXhgDZEmrL6Dva8JbKaVY4NZ5S4u9NgrbtRBsIJAhSYLae/nIcoRsi/B755Jw0UxT1AePgaRwjo0uBYCJUlAoB9aykwfMML2sueYjk/dZxUPD27suhs1ZQQIDAQAB";
        internal static string GetPublicKey { get { return publicKey; } }

        internal static void EncryptFile(string inputFilePath, string outputFilePath)
        {
            byte[] fileBytes = File.ReadAllBytes(inputFilePath);
            byte[] encryptedFileBytes;
            byte[] encryptedKey;
            byte[] iv;

            using (var aes = Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();
                iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(fileBytes, 0, fileBytes.Length);
                        cs.FlushFinalBlock();
                        encryptedFileBytes = ms.ToArray();
                    }
                }

                using (var rsa = RSA.Create())
                {
                    rsa.ImportRSAPublicKey(Convert.FromBase64String(GetPublicKey), out _);
                    encryptedKey = rsa.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA256);
                }
            }

            using (var ms = new MemoryStream())
            {
                ms.Write(iv, 0, iv.Length);
                ms.Write(encryptedKey, 0, encryptedKey.Length);
                ms.Write(encryptedFileBytes, 0, encryptedFileBytes.Length);
                File.WriteAllBytes(outputFilePath, ms.ToArray());
            }
        }

        /// <summary>
        /// 生成Debug快照文件
        /// </summary>
        public static string GenerateReportSnapshot()
        {
            string GIDPath = string.Empty;
            string ZipFilePath = string.Empty;
            try
            {
                string GID = $"Debug_{DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss-fff")}";
                GIDPath = $"{Config.Core_RunConfig._TemporaryFileDirectory}{GID}/";
                if(!Directory.Exists(GIDPath))
                {
                    Directory.CreateDirectory(GIDPath);
                }
                FileOperations.CopyAllFiles(Config.Core_RunConfig._ConfigDirectory, GIDPath);
                Thread.Sleep(100);
                if (File.Exists(LogModule.LogDB.ErrorFilePath))
                {
                    FileInfo fileInfo = new(LogModule.LogDB.ErrorFilePath);
                    File.Copy(LogModule.LogDB.ErrorFilePath, $"{GIDPath}{fileInfo.Name}",true);
                }
                if (File.Exists(LogModule.LogDB.dbPath))
                {
                    FileInfo fileInfo = new(LogModule.LogDB.dbPath);
                    File.Copy(LogModule.LogDB.dbPath, $"{GIDPath}{fileInfo.Name}",true);
                }


                ZipFilePath = Path.Combine($"{Config.Core_RunConfig._DebugFileDirectory}", $"{GID}.zip");
                ZipFile.CreateFromDirectory(GIDPath, ZipFilePath);
                FileInfo ZipFileInfo = new(ZipFilePath);
                string EncryofFilaName = $"{Config.Core_RunConfig._DebugFileDirectory}{ZipFileInfo.Name}_Encryof.{ZipFileInfo.Extension}";
                EncryptFile(ZipFilePath, EncryofFilaName);
                Log.Info(nameof(GenerateReportSnapshot),$"生成Debug快照成功，生成文件{EncryofFilaName}");
                return EncryofFilaName;
            }
            catch (Exception e)
            {
                return "";
            }
            finally
            {
                Tools.FileOperations.DeleteEmptyDirectories(Core.Config.Core_RunConfig._TemporaryFileDirectory);
                if (Directory.Exists(GIDPath))
                {
                    Tools.FileOperations.DeletePathFile(GIDPath);
                    //Directory.Delete(GIDPath, true);
                }
                if (File.Exists(ZipFilePath))
                {
                    Tools.FileOperations.Delete(ZipFilePath);
                }
            }
        }

    }
}
