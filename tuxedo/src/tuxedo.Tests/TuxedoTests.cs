using NUnit.Framework;

namespace tuxedo.Tests
{
    public class TuxedoTests
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            Tuxedo.Dialect = new PlainDialect();
        }
    }
}