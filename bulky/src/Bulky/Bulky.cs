namespace bulky
{
    public static class Bulky
    {
        public static IBulkCopy BulkCopier { get; set; }
        static Bulky()
        {
            BulkCopier = new SqlServerBulkCopy();
        }
    }
}