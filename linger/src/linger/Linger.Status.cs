namespace linger
{
    public partial class Linger
    {
        public static string Dump()
        {
            var jobs = Backend.GetAll();
            return JsonSerializer.SerializeJobs(jobs);
        }
    }
}
