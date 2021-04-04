using System;
using System.Collections.Generic;
using System.Text;

namespace Auxiliary.Upload
{
    public class UploadFailure : ApplicationException
    {
        public UploadFailure(string message) : base(message) { }
        public UploadFailure(string message, Exception inner) : base(message, inner) { }
    }

    public class OneDriveException : ApplicationException
    {
        public OneDriveException(string message) : base(message) { }
        public OneDriveException(string message, Exception inner) : base(message, inner) { }
    }
}
