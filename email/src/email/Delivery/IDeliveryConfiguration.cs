namespace email.Delivery
{
    public interface IDeliveryConfiguration
    {
        /// <summary>
        /// The number of threads allowed to deliver email in parallel; the default is 10
        /// </summary>
        int? MaxDegreeOfParallelism { get; }

        /// <summary>
        /// The location to store emails that are in queue when the delivery service is stopped, or based on retry policy
        /// </summary>
        string BacklogFolder { get; }

        /// <summary>
        /// The location to store emails that were not successfully delivered, based on retry policy
        /// </summary>
        string UndeliverableFolder { get; }

        /// <summary>
        /// The maximum rate of message delivery, in seconds
        /// </summary>
        double? MaxDeliveryRate { get; set; }

        /// <summary>
        /// The retry policy for failed delivery attempts
        /// </summary>
        DeliveryRetryPolicy RetryPolicy { get; set; }
    }
}