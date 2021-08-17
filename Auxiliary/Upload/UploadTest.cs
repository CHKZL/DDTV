using System;
using System.IO;
using System.Threading;
using static Auxiliary.Downloader;

namespace Auxiliary.Upload
{
    /// <summary>
    /// 测试上传
    /// 通过指定的方式实例化该对象
    /// </summary>
    public class UploadTest
    {
        public DownIofoData downIofo;
        public UploadTask uploadTask;

        #region 生成数据
        /// <summary>
        /// 测试上传，自动生成相应数据
        /// 指定上传源路径，上传顺序从config中获取
        /// </summary>
        /// <param name="srcFile">上传源文件路径</param>
        public UploadTest(string srcFile)
        {
            Thread.Sleep(3000);
            Uploader.enableUpload = true;
            MMPU.Debug模式 = true;
            MMPU.Debug打印到终端 = true;
            
            //Uploader.InitUpload();

            downIofo = new DownIofoData();
            downIofo.主播名称 = "UplaodTest_SteamerName";
            downIofo.标题 = "UplaodTest_StreamTitle";
            downIofo.房间_频道号 = "UplaodTest_RoomChannel";
            downIofo.开始时间 = 0;

            downIofo.文件保存路径 = srcFile;

            uploadTask = new UploadTask(downIofo);
            uploadTask.UploadVideo();
        }
        /// <summary>
        /// 测试上传，自动生成相应数据
        /// 生成上传源文件，上传顺序从config中获取
        /// </summary>
        public UploadTest() : this(@"./UplaodTest_FilePath/UplaodTest_FileName.flv")
        {
            CreateFixedSizeFile(@"UplaodTest_FilePath/UplaodTest_FileName.flv",1024*1024*1024);

            uploadTask = new UploadTask(downIofo);
            uploadTask.UploadVideo();
        }
        /// <summary>
        /// 测试上传，自动生成相应数据
        /// 指定上传源文件，指定上传目标
        /// </summary>
        /// <param name="srcFile">上传源文件路径</param>
        /// <param name="type">上传目标</param>
        public UploadTest(string srcFile, string type)
        {
            Thread.Sleep(3000);
            MMPU.Debug模式 = true;
            MMPU.Debug打印到终端 = true;
            Uploader.enableUpload = true;
            Thread.Sleep(5000);
            downIofo = new DownIofoData();
            downIofo.主播名称 = "UplaodTest_SteamerName";
            downIofo.标题 = "UplaodTest_StreamTitle";
            downIofo.房间_频道号 = "UplaodTest_RoomChannel";
            downIofo.开始时间 = 0;

            downIofo.文件保存路径 = srcFile;

            Uploader.UploadOrder.Clear();
            Uploader.UploadOrder.Add(1, type);

            uploadTask = new UploadTask(downIofo);
            uploadTask.UploadVideo();
        }

        /// <summary> 
        /// 创建固定大小的临时文件 
        /// </summary>  
        /// <param name="fileName"> 文件名 </param>  
        /// <param name="fileSize"> 文件大小 </param>  
        public void CreateFixedSizeFile(string fileName, long fileSize)
        {
            if (fileSize < 0) throw new ArgumentException("fileSize");
            //创建目录 
            string dir = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            //创建文件 
            FileStream fs = null;
            try
            {
                fs = new FileStream(fileName, FileMode.Create);
                fs.SetLength(fileSize); //设置文件大小 
            }
            catch
            {
                if (fs != null)
                {
                    fs.Close();
                    File.Delete(fileName); //注意，若由fs.SetLength方法产生了异常，同样会执行删除命令，请慎用overwrite:true参数，或者修改删除文件代码。 
                }
                throw;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }
        #endregion
    }
}
