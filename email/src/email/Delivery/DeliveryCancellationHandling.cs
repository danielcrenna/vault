namespace email.Delivery
{
    /// <summary>
    /// Describes the strategy of the <see cref="DeliveryService"/> when it is asked to stop.
    /// </summary>
    public enum DeliveryCancellationHandling
    {
        /// <summary>
        /// The service immediately redirects all emails in the queue to the backlog; emails that are
        /// queued after a stop call are always sent to the backlog.
        /// </summary>
        SendToBacklog,
        /// <summary>
        /// The service finishes sending the emails that were in queue prior to the stop call; emails that
        /// are queued after a stop call are always sent to the backlog.
        /// </summary>
        EmptyQueue,
    }
}