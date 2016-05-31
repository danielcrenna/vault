namespace cohort.Api.Models
{
    public interface IResource
    {
        long Id { get; set; }
        string Type { get; set; }
    }
}