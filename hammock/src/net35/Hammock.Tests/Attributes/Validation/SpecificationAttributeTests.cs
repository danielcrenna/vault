using System;
using Hammock.Attributes.Specialized;
using Hammock.Attributes.Validation;
using Hammock.Specifications;
using Hammock.Validation;
using Hammock.Web;
using NUnit.Framework;

namespace Hammock.Tests.Attributes.Validation
{
    [TestFixture]
    public class SpecificationAttributeTests
    {
        public class DateTimeFormatInfo : IWebQueryInfo
        {
            [Specification(typeof(ValidEmailSpecification))]
            [Parameter("Contact")]
            public string Email { get; set; }
        }

        [Test]
        [ExpectedException(typeof(ValidationException))]
        public void Can_use_specification_validation_to_block_request()
        {
            var info = new DateTimeFormatInfo { Email = "nowhere" };

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
