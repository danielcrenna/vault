using System.Web.Http;
using cohort.API.Filters;

namespace cohort.Api.Controllers.Users
{
    [AuthorizeUser]
    public class FooController : ApiController
    {
        public string Get()
        {
            return "Foo!";
        }
    }
}