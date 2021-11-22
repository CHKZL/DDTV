using DDTV_Core.Tool.Flv.FLVEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_Core.Tool.Flv
{
    internal class Fix
    {
        /// <summary>
        /// 修复FLV
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<int> FixFlvMetaData(string FileName)
        {
            CmdModel model = new CmdModel();
            CommandLineParser<CmdModel> parser = new CommandLineParser<CmdModel>(model);
            parser.CaseSensitive = false;
            parser.AssignmentSyntax = true;
            parser.WriteUsageOnError = true;
            if (!parser.Parse(new string[] { FileName }))
                return 1;

            if (model.FromSeconds.HasValue && model.ToSeconds.HasValue && model.FromSeconds.Value >= model.ToSeconds.Value)
            {
                Console.WriteLine("Start of output window (from) should be larger than end (to).");
                return 1;
            }
            Stream inputStream;
            if (model.OutputFile == null)
            {
                Console.WriteLine("Loading whole file to memory.");
                inputStream = new MemoryStream();
                using (FileStream fs = new FileStream(model.InputFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    fs.CopyTo(inputStream);
                inputStream.Position = 0;
            }
            else
                inputStream = new FileStream(model.InputFile, FileMode.Open, FileAccess.Read, FileShare.Read);

            DateTime fileDate = File.GetLastWriteTime(model.InputFile);

            FLVFile file = new FLVFile(inputStream);

            file.PrintReport();

            //if (model.FromSeconds.HasValue)
            //    file.CutFromStart(TimeSpan.FromSeconds(model.FromSeconds.Value));
            //if (model.ToSeconds.HasValue)
            //    file.CutToEnd(TimeSpan.FromSeconds(model.ToSeconds.Value));
            //if (model.FilterPackets)
            //    file.FilterPackets();
            ////if (model.FixTimestamps)
            //    file.FixTimeStamps();
            ////if (model.FixMetadata)
            //    file.FixMetadata();
            //if (model.RemoveMetadata)
            //    file.RemoveMetadata();



            file.FilterPackets();

            file.FixTimeStamps();

            file.FixMetadata();


            string outputFile = model.OutputFile ?? model.InputFile;
            Console.WriteLine("Writing: {0}", outputFile);
            file.Write(outputFile);

            inputStream.Dispose();

            if (model.PreserveDate)
                File.SetLastWriteTime(outputFile, fileDate);
            return 0;
        }
    }
}
