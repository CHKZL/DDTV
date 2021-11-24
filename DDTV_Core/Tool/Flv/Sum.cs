using System;
using System.IO;


namespace DDTV_Core.Tool.Flv
{
    public class Sum
    {
        public static void FlvFileSum(SystemAssembly.BilibiliModule.Rooms.RoomInfoClass.RoomInfo roomInfo,string OkFilePath)
        {
            if (roomInfo.DownloadingList.Count>1)
            {
                FileStream OldFileStream = new FileStream(roomInfo.DownloadingList[0].File, FileMode.Open);
                for (int i = 1 ; i < roomInfo.DownloadingList.Count ; i++)
                {
                    using FileStream NewFileStream = new FileStream(roomInfo.DownloadingList[i].File, FileMode.Open);
                    string TmpFile = "";
                    using (FileStream fsMerge = new FileStream(PathOperation.CreateAll("./tmp/")+$"{roomInfo.room_id}_{new Random().Next(10000, 99999)}.flv", FileMode.Create))
                        if (GetFLVFileInfo(OldFileStream) != null && GetFLVFileInfo(NewFileStream) != null)
                        {
                            if (IsSuitableToMerge(GetFLVFileInfo(OldFileStream), GetFLVFileInfo(NewFileStream)) == false)
                            {
                                SystemAssembly.Log.Log.AddLog(nameof(Flv), SystemAssembly.Log.LogClass.LogType.Warn, $"来自{roomInfo.room_id}房间的录制任务在直播过程中主播切换了码率或分辨率，合并会造成文件错误，放弃本次合并任务");
                                return;
                            }
                            int time = Merge(OldFileStream, fsMerge, true, 0);
                            time = Merge(NewFileStream, fsMerge, false, time);

                            OldFileStream.Close();
                            OldFileStream.Dispose();
                            TmpFile=fsMerge.Name;
                        }
                    OldFileStream = new FileStream(TmpFile, FileMode.Open);
                }
                File.Copy(OldFileStream.Name, OkFilePath);
                OldFileStream.Close();
                OldFileStream.Dispose();
                foreach (var item in roomInfo.DownloadingList)
                {
                    File.Delete(item.File);
                }
            }
        }

        const int FLV_HEADER_SIZE = 9;
        const int FLV_TAG_HEADER_SIZE = 11;
        const int MAX_DATA_SIZE = 16777220;
        class FLVContext
        {
            public byte soundFormat;
            public byte soundRate;
            public byte soundSize;
            public byte soundType;
            public byte videoCodecID;
        }

        static bool IsSuitableToMerge(FLVContext flvCtx1, FLVContext flvCtx2)
        {
            return (flvCtx1.soundFormat == flvCtx2.soundFormat) &&
              (flvCtx1.soundRate == flvCtx2.soundRate) &&
              (flvCtx1.soundSize == flvCtx2.soundSize) &&
              (flvCtx1.soundType == flvCtx2.soundType) &&
              (flvCtx1.videoCodecID == flvCtx2.videoCodecID);
        }

        static bool IsFLVFile(FileStream fs)
        {
            byte[] buf = new byte[FLV_HEADER_SIZE];
            fs.Position = 0;
            if (FLV_HEADER_SIZE != fs.Read(buf, 0, buf.Length))
                return false;

            if (buf[0] != 'F' || buf[1] != 'L' || buf[2] != 'V' || buf[3] != 0x01)
                return false;
            else
                return true;
        }
        static FLVContext GetFLVFileInfo(FileStream fs)
        {
            bool hasAudioParams, hasVideoParams;
            int skipSize;
            int dataSize;
            byte tagType;
            byte[] tmp = new byte[FLV_TAG_HEADER_SIZE + 1];
            if (fs == null) return null;

            FLVContext flvCtx = new FLVContext();
            fs.Position = 0;
            skipSize = 9;
            fs.Position += skipSize;
            hasVideoParams = hasAudioParams = false;
            skipSize = 4;
            while (!hasVideoParams || !hasAudioParams)
            {
                fs.Position += skipSize;

                if (FLV_TAG_HEADER_SIZE + 1 != fs.Read(tmp, 0, tmp.Length))
                    return null;

                tagType = (byte)(tmp[0] & 0x1f);
                switch (tagType)
                {
                    case 8:
                        flvCtx.soundFormat = (byte)((tmp[FLV_TAG_HEADER_SIZE] & 0xf0) >> 4);
                        flvCtx.soundRate = (byte)((tmp[FLV_TAG_HEADER_SIZE] & 0x0c) >> 2);
                        flvCtx.soundSize = (byte)((tmp[FLV_TAG_HEADER_SIZE] & 0x02) >> 1);
                        flvCtx.soundType = (byte)((tmp[FLV_TAG_HEADER_SIZE] & 0x01) >> 0);
                        hasAudioParams = true;
                        break;
                    case 9:
                        flvCtx.videoCodecID = (byte)((tmp[FLV_TAG_HEADER_SIZE] & 0x0f));
                        hasVideoParams = true;
                        break;
                    default:
                        break;
                }

                dataSize = FromInt24StringBe(tmp[1], tmp[2], tmp[3]);
                skipSize = dataSize - 1 + 4;
            }

            return flvCtx;
        }

        static int FromInt24StringBe(byte b0, byte b1, byte b2)
        {
            return (int)((b0 << 16) | (b1 << 8) | (b2));
        }

        static int GetTimestamp(byte b0, byte b1, byte b2, byte b3)
        {
            return ((b3 << 24) | (b0 << 16) | (b1 << 8) | (b2));
        }

        static void SetTimestamp(byte[] data, int idx, int newTimestamp)
        {
            data[idx + 3] = (byte)(newTimestamp >> 24);
            data[idx + 0] = (byte)(newTimestamp >> 16);
            data[idx + 1] = (byte)(newTimestamp >> 8);
            data[idx + 2] = (byte)(newTimestamp);
        }

        static int Merge(FileStream fsInput, FileStream fsMerge, bool isFirstFile, int lastTimestamp = 0)
        {
            int readLen;
            int curTimestamp = 0;
            int newTimestamp = 0;
            int dataSize;
            byte[] tmp = new byte[20];
            byte[] buf = new byte[MAX_DATA_SIZE];

            fsInput.Position = 0;
            if (isFirstFile)
            {
                if (FLV_HEADER_SIZE + 4 == (fsInput.Read(tmp, 0, FLV_HEADER_SIZE + 4)))
                {
                    fsMerge.Position = 0;
                    fsMerge.Write(tmp, 0, FLV_HEADER_SIZE + 4);
                }
            }
            else
            {
                fsInput.Position = FLV_HEADER_SIZE + 4;
            }

            while (fsInput.Read(tmp, 0, FLV_TAG_HEADER_SIZE) > 0)
            {
                dataSize = FromInt24StringBe(tmp[1], tmp[2], tmp[3]);
                curTimestamp = GetTimestamp(tmp[4], tmp[5], tmp[6], tmp[7]);
                newTimestamp = curTimestamp + lastTimestamp;
                SetTimestamp(tmp, 4, newTimestamp);
                fsMerge.Write(tmp, 0, FLV_TAG_HEADER_SIZE);

                readLen = dataSize + 4;
                if (fsInput.Read(buf, 0, readLen) > 0)
                {
                    fsMerge.Write(buf, 0, readLen);
                }
                else
                {
                    goto failed;
                }
            }

            return newTimestamp;

failed:
            throw new Exception("Merge Failed");
        }

    }
}
