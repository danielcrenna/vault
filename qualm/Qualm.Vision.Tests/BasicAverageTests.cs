using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace Qualm.Vision.Tests
{
    [TestFixture]
    public class BasicAverageTests
    {
        [Test]
        public void Can_create_basic_average_perceptual_hash()
        {
            var image = Image.FromFile("Images/ah.jpg");
            var algo = new BasicAverage();
            var hash = algo.Generate(image);
            Assert.IsNotNull(hash);
            Assert.IsNotNull(hash.Image);
            Assert.AreEqual(new Size(8, 8), hash.Image.Size);
            
            algo.Crushed.Save("Images/ah-crushed.jpg", ImageFormat.Jpeg);
            algo.Gray.Save("Images/ah-gray.jpg", ImageFormat.Jpeg);
            hash.Image.Save("Images/ah-hash.jpg", ImageFormat.Jpeg);
        }

        [Test]
        public void There_is_no_distance_between_identical_hashes()
        {
            var image = Image.FromFile("Images/ah.jpg");
            var algo = new BasicAverage();
            var hash1 = algo.Generate(image);
            Assert.AreEqual(0, (int)hash1.CompareTo(hash1));
        }

        [Test]
        public void There_is_a_distance_between_different_hashes()
        {
            var algo = new BasicAverage();
            var image1 = Image.FromFile("Images/ah.jpg");
            var hash1 = algo.Generate(image1);
            var image2 = Image.FromFile("Images/ah-doctored.jpg");
            var hash2 = algo.Generate(image2);

            Assert.AreEqual(6, (int)hash1.CompareTo(hash2));
        }
    }
}
