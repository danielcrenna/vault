using NUnit.Framework;

namespace protobuf.Poco.Tests
{
    [TestFixture]
    public class ReflectionRuntimeTypeModelTests
    {
        [Test]
        public void Model_reflects_arbitrary_type()
        {
            var model = ReflectionRuntimeTypeModel.Create();
            ReflectionRuntimeTypeModel.AddType<User>(model);
            Assert.IsTrue(model.CanSerialize(typeof(User)));
        }
    }
}
