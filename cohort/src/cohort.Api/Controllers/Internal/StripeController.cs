using System.Threading.Tasks;
using System.Web.Http;
using Stripe;
using cohort.API.Filters;

namespace cohort.Api.Controllers.Internal
{
    [AuthorizeAdmin]
    public class StripeController : ApiController
    {
        public async Task<dynamic> GetPlans()
        {
            return await Task.Factory.StartNew(() =>
            {
                var forPlans = new StripePlanService(Cohort.Site.Stripe.SecretKey);
                var plans = forPlans.List();
                return new { Plans = plans };
            });
        }
    }

    public class PlanRow
    {
        public string Name { get; set; }
        public int SubscriberCount { get; set; }
    }

}