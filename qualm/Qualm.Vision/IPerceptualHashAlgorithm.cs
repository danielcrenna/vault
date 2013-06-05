using System.Drawing;

namespace Qualm.Vision
{
    public interface IPerceptualHashAlgorithm
    {
        PerceptualHash Generate(Image image);
    }
}