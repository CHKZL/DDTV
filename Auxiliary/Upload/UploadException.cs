using System;

namespace Auxiliary.Upload
{
    /// <summary>
    /// 获取上传文件信息失败
    /// </summary>
    public class CreateUploadTaskFailure : ApplicationException
    {
        public CreateUploadTaskFailure(string message) : base(message) { }
        public CreateUploadTaskFailure(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// 上传失败
    /// </summary>
    public class UploadFailure : ApplicationException
    {
        public UploadFailure(string message) : base(message) { }
        public UploadFailure(string message, Exception inner) : base(message, inner) { }
    }
}
