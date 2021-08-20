using System;
using System.Collections.Generic;
using System.Text;

namespace Auxiliary.Upload.Service
{
    public interface ServiceInterface
    {
        void doUpload(Info.TaskInfo task);
    }
}
