using Core.LogModule;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing.QrCode.Internal;

namespace Core.Tools
{
    public class DokiDoki
    {
        public static DokiClass GetDoki()
        {
            Process currentProcess = null;
            currentProcess = Process.GetCurrentProcess();
            long totalBytesOfMemoryUsed = currentProcess.WorkingSet64;
            (int Total, int Download) = Core.RuntimeObject.RoomInfo.GetTasksInDownloadCount();
            DokiClass dokiClass = new DokiClass()
            {
                CompiledVersion = Init.CompiledVersion,
                Downloading = Download,
                InitType = Init.InitType,
                Total = Total,
                UsingMemory = totalBytesOfMemoryUsed,
                UsingMemoryStr = Core.Tools.Linq.ConversionSize(totalBytesOfMemoryUsed, Core.Tools.Linq.ConversionSizeType.String),
                Ver = Init.Ver,
                StartMode = Core.Init.Mode
            };
            dokiClass.CompilationMode = Core.Config.Core_RunConfig._DevelopmentVersion ? "Dev" : "Release";

            return dokiClass;

        }
        public class DokiClass
        {
            public int Total { get; set; }
            public int Downloading { get; set; }
            public long UsingMemory { get; set; }
            public string UsingMemoryStr { get; set; }
            public string InitType { get; set; }
            public string Ver { get; set; }
            public string CompiledVersion { get; set; }
            public string CompilationMode { set; get; }
            public Core.Config.Mode StartMode { get; set; } = Config.Mode.Core;
        }
    }
}
