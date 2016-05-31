using System;
using Stripe;

namespace cohort.Services
{
    public class StripeService
    {
        public bool SaveCustomerByToken(string email, string stripeToken)
        {
            try
            {
                var customer = new StripeCustomerCreateOptions();
                customer.Email = email;
                //customer.Description = "Johnny Tenderloin (pork@email.com)";
                customer.TokenId = stripeToken;
                //customer.PlanId = *planId*;                          // only if you have a plan
                //customer.Coupon = *couponId*;                        // only if you have a coupon
                //customer.TrialEnd = DateTime.UtcNow.AddMonths(1);    // when the customers trial ends (overrides the plan if applicable)
                //customer.Quantity = 1;                               // optional, defaults to 1
                var customerService = new StripeCustomerService(Cohort.Site.Stripe.SecretKey);
                var stripeCustomer = customerService.Create(customer);
                // Create linkage between signup and customer for later charging

                return true;
            }
            catch (Exception)
            {
                // log 

                return false;
            }
        }
    }
}
