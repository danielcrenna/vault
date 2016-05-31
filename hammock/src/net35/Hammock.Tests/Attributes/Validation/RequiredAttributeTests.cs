using Hammock.Attributes.Specialized;
using Hammock.Attributes.Validation;
using Hammock.Validation;
using Hammock.Web;
using NUnit.Framework;

namespace Hammock.Tests.Attributes.Validation
{
    [TestFixture]
    public class RequiredAttributeTests
    {
        public class RequiredInfo : IWebQueryInfo
        {
            [Required]
            [Header("Result")]
            public string ICantBeNull { get; set; }
        }

        [Test]
        [ExpectedException(typeof(ValidationException))]
        public void Can_use_required_validation_to_block_null_value()
        {
            var info = new RequiredInfo { ICantBeNull = null };

            var client = new RestClient
            {
                Authority = "http://nowhere.com",
                Info = info
            };

            var request = new RestRequest
            {
                Path = "fast"
            };

            client.Request(request);
        }
    }

}
