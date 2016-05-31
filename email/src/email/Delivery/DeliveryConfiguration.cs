namespace email.Delivery
{
    public class DeliveryConfiguration : IDeliveryConfiguration
    {
        /// <summary>
        /// The number of threads allowed to deliver email in parallel; the default is 10
        /// </summary>
        public int? MaxDegreeOfParallelism { get; set; }

        /// <summary>
        /// The location to store emails that are in queue when the delivery service is stopped, or based on retry policy
        /// </summary>
        public string BacklogFolder { get; set; }

        /// <summary>
        /// The location to store emails that were not successfully delivered, based on retry policy
        /// </summary>
        public string UndeliverableFolder { get; set; }

        /// <summary>
        /// The maximum rate of message delivery, in seconds
        /// </summary>
        public double? MaxDeliveryRate { get; set; }

        /// <summary>
        /// The retry policy for failed delivery attempts
        /// </summary>
        public DeliveryRetryPolicy RetryPolicy { get; set; }

        public DeliveryConfiguration()
        {
            RetryPolicy = new DeliveryRetryPolicy();
        }
    }
}