using Core.RuntimeObject;
using Masuit.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tools
{
    public class KeyCharacterReplacement
    {
        /// <summary>
        /// 检查字符串是否符合文件路径标准,并返回安全字符串
        /// </summary>
        /// <param name="Text"></param>
        /// <returns>返回清除不符合要求的字符后的字符串</returns>
        public static string CheckFilenames(string Text)
        {
            Text = Text.Replace(" ", string.Empty).Replace("/", string.Empty).Replace("\\", string.Empty).Replace("\"", string.Empty).Replace(":", string.Empty).Replace("*", string.Empty).Replace("?", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty).Replace("|", string.Empty).Replace("#", string.Empty).Replace("&", string.Empty).Replace("=", string.Empty).Replace("%", string.Empty).Replace("\0", string.Empty);
            StringBuilder rBuilder = new StringBuilder(Text);
            foreach (char rInvalidChar in Path.GetInvalidPathChars())
                rBuilder = rBuilder.Replace(rInvalidChar.ToString(), string.Empty);
            Text = rBuilder.ToString();
            return Text;
        }

        /// <summary>
        /// 替换关键字(用于替换预设的关键字如{roomid},{name}之类的)
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string ReplaceKeyword(long uid, string Text)
        {
            _Room.GetCardList().TryGetValue(uid, out RoomCardClass roomInfo);


            if (roomInfo.DownInfo.DownloadFileList.VideoFile.Count > 0)
            {
                string FileList = string.Empty;
                foreach (var item in roomInfo.DownInfo.DownloadFileList.VideoFile)
                {
                    FileList += item + ";";
                }
                Text = Text.Replace("{VideoFiles}", FileList);
            }

            if (roomInfo.DownInfo.DownloadFileList.DanmuFile.Count > 0)
            {
                string FileList = string.Empty;
                foreach (var item in roomInfo.DownInfo.DownloadFileList.DanmuFile)
                {
                    FileList += item + ";";
                }
                Text = Text.Replace("{DanMuFile}", FileList);
            }

            if (roomInfo.DownInfo.DownloadFileList.SCFile.Count > 0)
            {
                string FileList = string.Empty;
                foreach (var item in roomInfo.DownInfo.DownloadFileList.SCFile)
                {
                    FileList += item + ";";
                }
                Text = Text.Replace("{SCFile}", FileList);
            }

            if (roomInfo.DownInfo.DownloadFileList.GuardFile.Count > 0)
            {
                string FileList = string.Empty;
                foreach (var item in roomInfo.DownInfo.DownloadFileList.GuardFile)
                {
                    FileList += item + ";";
                }
                Text = Text.Replace("{GuardFile}", FileList);
            }

            if (roomInfo.DownInfo.DownloadFileList.GiftFile.Count > 0)
            {
                string FileList = string.Empty;
                foreach (var item in roomInfo.DownInfo.DownloadFileList.GiftFile)
                {
                    FileList += item + ";";
                }
                Text = Text.Replace("{GiftFile}", FileList);
            }
            Text = Text
                .Replace("{ROOMID}", RoomInfo.GetRoomId(uid).ToString())
                .Replace("{YYYY}", DateTime.Now.ToString("yyyy"))
                .Replace("{YY}", DateTime.Now.ToString("yy"))
                .Replace("{MM}", DateTime.Now.ToString("MM"))
                .Replace("{DD}", DateTime.Now.ToString("dd"))
                .Replace("{HH}", DateTime.Now.ToString("HH"))
                .Replace("{mm}", DateTime.Now.ToString("mm"))
                .Replace("{SS}", DateTime.Now.ToString("ss"))
                .Replace("{FFFF}", DateTime.Now.ToString("fff"))
                .Replace("{yyyy}", DateTime.Now.ToString("yyyy"))
                .Replace("{yy}", DateTime.Now.ToString("yy"))
                .Replace("{MM}", DateTime.Now.ToString("MM"))
                .Replace("{dd}", DateTime.Now.ToString("dd"))
                .Replace("{HH}", DateTime.Now.ToString("HH"))
                .Replace("{mm}", DateTime.Now.ToString("mm"))
                .Replace("{ss}", DateTime.Now.ToString("ss"))
                .Replace("{fff}", DateTime.Now.ToString("fff"))
                .Replace("{NAME}", CheckFilenames(RoomInfo.GetNickname(uid)))
                .Replace("{DATE}", DateTime.Now.ToString("yyyy_MM_dd"))
                .Replace("{TIME}", DateTime.Now.ToString("HH_mm_ss"))
                .Replace("{TITLE}", CheckFilenames(RoomInfo.GetTitle(uid)))
                .Replace("{R}", GetRandomStr(string.Empty, 5))
                .Replace("\\", "/");

            return Text;
        }

        /// <summary>
        /// 随机字符串
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetRandomStr(string chars, int length)
        {
            Random random = new Random();
            if (string.IsNullOrEmpty(chars))
            {
                chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghizklmnopqrstuvwxyz0123456789";
            }
            //const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
