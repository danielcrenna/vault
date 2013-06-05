using System;
using Hammock.Attributes.Specialized;
using Hammock.Attributes.Validation;
using Hammock.Web;
using NUnit.Framework;

namespace Hammock.Tests.Attributes.Validation
{
    [TestFixture]
    public class DateTimeFormatTests
    {
        public class DateTimeFormatInfo : IWebQueryInfo
        {
            [DateTimeFormat("mmm-DD-yyyy")]
            [Header("Anniversary")]
            public DateTime IAmADate { get; set; }
        }

        [Test]
        public void Can_use_date_time_validation_to_transform_header_value()
        {
            var info = new DateTimeFormatInfo {IAmADate = DateTime.Now };

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