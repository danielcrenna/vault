using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Stripe;
using cohort.Logging;
using tophat;
using tuxedo.Dapper;

namespace cohort.API.Controllers
{
    /// <summary>
    /// Accepts incoming Stripe API web hooks and processes them
    /// </summary>
    public class WebHookController : ApiController
    {
        public async Task<HttpResponseMessage> Post()
        {
            return await Request.Content.ReadAsStringAsync().ContinueWith(task =>
            {
                using (var db = UnitOfWork.Current)
                {
                    TryProcessWebHook(task, db);
                }
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Accepted };
            });
        }

        private static void TryProcessWebHook(Task<string> task, IDbConnection db)
        {
            try
            {
                var json = task.Result;
                var @event = StripeEventUtility.ParseEvent(json);

                // Ignore test events on a production system
                var production = !Cohort.Site.Stripe.TestMode;
                var testEvent = !@event.LiveMode.HasValue || !@event.LiveMode.Value;
                if (production && testEvent)
                {
                    return;
                }

                db.Insert(@event);
                Logger.Info("Processed web hook -> " + json);
            }
            catch (Exception ex)
            {
                Logger.Error("Error processing web hook -> {0}", ex);
            }
        }
    }
}