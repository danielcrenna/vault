using System;
using System.Drawing;

namespace Qualm.Vision
{
    public class PerceptualHash
    {
        public Image Image { get; set; }

        public Number CompareTo(PerceptualHash hash)
        {
            if(hash == null || Image == null || hash.Image == null || Image.Size != hash.Image.Size)
            {
                throw new ArgumentException("Both hashes must have images whose sizes match");
            }
            var left = ImageUtil.ConvertToNumbers(Image);
            var right = ImageUtil.ConvertToNumbers(hash.Image);
            return Distance.Hanning(left, right);
        }
    }
}
