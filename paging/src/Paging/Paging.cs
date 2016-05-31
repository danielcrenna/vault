namespace Paging
{
    public class Paging
    {
        public static int DefaultPageSize { get; set; }

        static Paging()
        {
            DefaultPageSize = 10;
        }
    }
}
