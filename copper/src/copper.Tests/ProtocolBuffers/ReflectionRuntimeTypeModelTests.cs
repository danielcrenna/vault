using NUnit.Framework;
using copper.ProtocolBuffers;

namespace copper.Tests.ProtocolBuffers
{
    [TestFixture]
    public class ReflectionRuntimeTypeModelTests
    {
        [Test]
        public void Model_reflects_arbitrary_type()
        {
            var model = ReflectionRuntimeTypeModel.Create();
            ReflectionRuntimeTypeModel.AddType<StringEvent>(model);
            Assert.IsTrue(model.CanSerialize(typeof(StringEvent)));
        }
    }
}
