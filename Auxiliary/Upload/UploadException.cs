using System;

namespace Auxiliary.Upload
{
    /// <summary>
    /// Task创建失败
    /// </summary>
    public class TaskException : ApplicationException
    {
        public TaskException(string message) : base(message) { }
        public TaskException(string message, Exception inner) : base(message, inner) { }
    }
    /// <summary>
    /// File创建失败
    /// </summary>
    public class FileException : ApplicationException
    {
        public FileException(string message) : base(message) { }
        public FileException(string message, Exception inner) : base(message, inner) { }
    }
    /// <summary>
    /// Project创建失败
    /// </summary>
    public class ProjectException : ApplicationException
    {
        public ProjectException(string message) : base(message) { }
        public ProjectException(string message, Exception inner) : base(message, inner) { }
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
