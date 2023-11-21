using System;
using System.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using ZXing;
using ZXing.QrCode;

namespace Core.Tools
{
    public class QRConsole
    {
        /// <summary>
        /// 生成二维码内容输出到控制台
        /// </summary>
        /// <param name="text"></param>
        public static void Output(string text)
        {
            var writer = new ZXing.Windows.Compatibility.BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = 30,
                    Width = 30,
                    Margin = 1
                }
            };
            var bitmap = writer.Write(text);

            // Convert the bitmap to a boolean array
            bool[,] pixels = new bool[bitmap.Width, bitmap.Height];
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    pixels[x, y] = bitmap.GetPixel(x, y).R == 0;
                }
            }

            // Print the QR code to the console
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Console.Write(pixels[x, y] ? "██" : "  ");
                }
                Console.WriteLine();
            }
        }

    }
}
