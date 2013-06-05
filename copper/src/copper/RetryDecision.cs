namespace copper
{
    public enum RetryDecision
    {
        RetryImmediately,
        Requeue,
        Backlog,
        Undeliverable,
        Destroy
    }
}