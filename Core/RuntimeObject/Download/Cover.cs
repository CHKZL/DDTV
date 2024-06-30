using Core.LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.RuntimeObject.Download
{
    public class Cover
    {
        public static void SaveCover(RoomCardClass card)
        {
            Task.Run(() =>
            {
                try
                {
                    if (card != null && !string.IsNullOrEmpty(card.cover_from_user.Value))
                    {
                        string File = $"{Config.Core_RunConfig._RecFileDirectory}{Core.Tools.KeyCharacterReplacement.ReplaceKeyword($"{Config.Core_RunConfig._DefaultLiverFolderName}/{Core.Config.Core_RunConfig._DefaultDataFolderName}{(string.IsNullOrEmpty(Core.Config.Core_RunConfig._DefaultDataFolderName) ? "" : "/")}{Config.Core_RunConfig._DefaultFileName}", DateTime.Now, card.UID)}_cover.jpg";
                        Basics.CreateDirectoryIfNotExists(File.Substring(0, File.LastIndexOf('/')));
                        Network.Download.File.DownloadFile(card.cover_from_user.Value, File, true);
                        Log.Info(nameof(SaveCover), $"保存{card.Name}({card.RoomId})封面完成");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(SaveCover), $"保存{card.Name}({card.RoomId})封面出现意外错误！", ex, false);
                }
            });
        }
    }
}
