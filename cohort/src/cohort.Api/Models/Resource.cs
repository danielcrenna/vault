using ServiceStack.Text;
using TableDescriptor;

namespace cohort.Api.Models
{
    public abstract class Resource : IResource
    {
        public long Id { get; set; }
        [Transient] public string Type { get; set; }

        protected Resource()
        {
            Type = GetType().Name.ToLowercaseUnderscore();
        }
    }
}