using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using cohort.Sqlite;
using Dapper;
using cohort.API.Filters;
using cohort.Models;
using tophat;

namespace cohort.Api.Controllers.Internal
{
    [AuthorizeAdmin]
    public class BrokenLinkController : ApiController 
    {
        private readonly IBrokenLinkRepository _repository;

        public BrokenLinkController(IBrokenLinkRepository repository)
        {
            _repository = repository;
        }

        public BrokenLinkController() : this(new BrokenLinkRepository())
        {
            
        }

        public async Task<dynamic> Get()
        {
            return await Task.Factory.StartNew(() =>
            {
                IEnumerable<BrokenLink> links;
                using (var db = UnitOfWork.Current)
                {
                    links = db.Query<BrokenLink>("SELECT * FROM BrokenLink");
                }
                var model = new {Links = links};
                return model;
            });
        }
    }
}
