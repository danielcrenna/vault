namespace linger
{
    public static class EveryExtensions
    {
        public static Every Every(this ScheduledJob job)
        {
            return new Every(job);
        }

        public static Every Every(this ScheduledJob job, int n)
        {
            return new Every(job, n);
        }
    }
}