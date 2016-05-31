using cohort.API.Filters;
using cohort.Api.Models;
using cohort.Api.Models.Internal;

namespace cohort.Api.Controllers.Internal
{
    [AuthorizeAdmin]
    public class DunningController : ResourceController<Dunning>
    {
        public DunningController(IResourceRepository<Dunning> repository) : base(repository)
        {

        }
    }
}