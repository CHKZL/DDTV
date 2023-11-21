using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tools
{
    public class ImageProcess
    {
        /// <summary>
        /// 将SKData转换为Bitmap对象
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Bitmap GetBitmap(SKData data)
        {
            // 使用SKData对象创建SKImage对象
            SKImage image = SKImage.FromEncodedData(data);

            // 将SKImage对象转换为SKBitmap对象
            SKBitmap skBitmap = SKBitmap.Decode(data);

            // 将SKBitmap对象转换为SKImage对象
            SKImage skImage = SKImage.FromBitmap(skBitmap);

            // 然后，你可以使用SkiaSharp.Views.Desktop.Extensions.ToBitmap方法将SKImage对象转换为System.Drawing.Bitmap对象
            Bitmap bitmap;
            using (var imageStream = skImage.Encode(SKEncodedImageFormat.Png, 100).AsStream())
            {
                bitmap = new Bitmap(imageStream);
            }
            return bitmap;
        }
    }
}
