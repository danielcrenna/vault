namespace linger
{
    public static class RepeatExtensions
    {
        public static For For(this RepeatInfo info, int n)
        {
            return new For(info, n);
        }
    }
}