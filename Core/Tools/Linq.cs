using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tools
{
    internal class Linq
    {
        internal static RuntimeObject.Download.File.EXTM3U SerializedM3U8(string str)
        {
            RuntimeObject.Download.File.EXTM3U eXTM3U = new();
            int Ver = 0;
            int TimeOffSet = 0;
            long MediaSequence = 0;
            double Targetduration = 0;
            string[] list = str.Split('\n');
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].Contains("#EXT-X-VERSION"))
                {
                    int.TryParse(list[i].Split(':')[1], out Ver);
                    eXTM3U.Version = Ver;
                }
                else if (list[i].Contains("#EXT-X-START:TIME-OFFSET"))
                {
                    int.TryParse(list[i].Split('=')[1], out TimeOffSet);
                    eXTM3U.TimeOffSet = TimeOffSet;
                }
                else if (list[i].Contains("#EXT-X-MEDIA-SEQUENCE"))
                {
                    long.TryParse(list[i].Split(':')[1], out MediaSequence);
                    eXTM3U.MediaSequence = MediaSequence;
                }
                else if (list[i].Contains("#EXT-X-TARGETDURATION"))
                {
                    double.TryParse(list[i].Split(':')[1], out Targetduration);
                    eXTM3U.Targetduration = Targetduration;
                }
                else if (list[i].Contains("#EXT-X-MAP:URI"))
                {
                    eXTM3U.Map_URI = list[i].Split('=')[1].Replace("\"","");
                }
                else if (list[i].Contains("#EXTINF"))
                {
                    double.TryParse(list[i].Split(':')[1].Split(',')[0], out double Duration);
                    RuntimeObject.Download.File.EXTM3U.EXTINF eXTINF = new()
                    {
                        Duration = Duration,
                        Aux = list[i - 1].Split(':')[1],
                        FileName = list[i + 1].Split('.')[0],
                        ExtensionName = list[i + 1].Split(".")[1]
                    };
                    eXTM3U.eXTINFs.Add(eXTINF);
                }
                else if (list[i].Contains("#EXT-X-ENDLIST"))
                {
                    eXTM3U.IsEND = true;
                }  
            }
            return eXTM3U;
        }
    }
}
