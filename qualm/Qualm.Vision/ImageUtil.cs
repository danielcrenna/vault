using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Qualm.Vision
{
    public class ImageUtil
    {
        public static Number[] ConvertToNumbers(Image image)
        {
            var result = new List<Number>();
            using (var bitmap = new Bitmap(image))
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    for (var y = 0; y < bitmap.Height; y++)
                    {
                        var p = (uint)bitmap.GetPixel(x, y).ToArgb();
                        result.Add(p);
                    }
                }
            }
            return result.ToArray();
        }

        public static Bitmap Resize(Image image, int width, int height)
        {
            var result = new Bitmap(width, height);
            using (var g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(image, new Rectangle(0, 0, width, height));
            }
            return result;
        }

        // Source from: http://www.switchonthecode.com/tutorials/csharp-tutorial-convert-a-color-image-to-grayscale
        public static Image Grayscale(Image image)
        {
            var result = new Bitmap(image.Width, image.Height);
            using (var g = Graphics.FromImage(result))
            {
                var colorMatrix = new ColorMatrix(new[]
                {
                    new[] { 0.3f, 0.3f, 0.3f, 0.0f, 0.0f },
                    new[] { 0.59f, 0.59f, 0.59f, 0.0f, 0.0f },
                    new[] { 0.11f, 0.11f, 0.11f, 0.0f, 0.0f },
                    new[] { 0.0f, 0.0f, 0.0f, 1.0f, 0.0f },
                    new[] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f }
                });

                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

                var rect = new Rectangle(0, 0, image.Width, image.Height);
                g.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
            return result;
        }

        public static Color AverageColor(Image image)
        {
            int r = 0, g = 0, b = 0, t = 0;
            using (var bitmap = new Bitmap(image))
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    for (var y = 0; y < bitmap.Height; y++)
                    {
                        var p = bitmap.GetPixel(x, y);
                        r += p.R;
                        g += p.G;
                        b += p.B;
                        t++;
                    }
                }
            }
            return Color.FromArgb(r / t, g / t, b / t);
        }
    }
}