using System.Drawing;

namespace Qualm.Vision
{
    /// <summary>
    /// Calculates a perceptual hash using a basic averaging algorithm by Dr. Neal Krawetz.
    /// <see href="http://www.hackerfactor.com/blog/index.php?/archives/432-Looks-Like-It.html" />
    /// </summary>
    public class BasicAverage : IPerceptualHashAlgorithm
    {
#if DEBUG
        public Image Crushed { get; set; }
        public Image Gray { get; set; }
#endif
        public PerceptualHash Generate(Image image)
        {
            var crushed = ImageUtil.Resize(image, 8, 8);
            var gray = ImageUtil.Grayscale(crushed);
#if DEBUG
            Crushed = crushed;
            Gray = gray;
#endif
            var averageColor = ImageUtil.AverageColor(gray);
            var mean = (uint)averageColor.ToArgb();
            var hash = new Bitmap(gray.Width, gray.Height);
            using (var bitmap = new Bitmap(gray))
            {
                for (var x = 0; x < gray.Width; x++)
                {
                    for (var y = 0; y < gray.Height; y++)
                    {
                        var p = (uint)bitmap.GetPixel(x, y).ToArgb();
                        var c = p >= mean ? Color.White : Color.Black;
                        hash.SetPixel(x, y, c);
                    }
                }
            }
            var result = new PerceptualHash { Image = hash };
            return result;
        }
    }
}