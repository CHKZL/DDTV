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
}
