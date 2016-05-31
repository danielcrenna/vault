namespace email.Delivery
{
    /// <summary>
    /// A decision about how to handle a failed email
    /// </summary>
    public enum DeliveryRetryDecision
    {
        /// <summary>
        /// The delivery service must immediately retry the failed email (higher priority than queued messages)
        /// </summary>
        RetryImmediately,
        /// <summary>
        /// The failed email is sent to the back of the current processing queue (higher priority than backlogged messages)
        /// </summary>
        SendToBackOfQueue,
        /// <summary>
        /// The failed email is sent to the backlog (higher priority than undeliverable messages)
        /// </summary>
        SendToBacklog,
        /// <summary>
        /// The failed email is sent to the undeliverable folder; it won't be delivered again
        /// </summary>
        SendToUndeliverableFolder,
        /// <summary>
        /// The failed email is deleted
        /// </summary>
        Destroy
    }
}