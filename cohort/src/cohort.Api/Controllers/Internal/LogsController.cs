using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Dapper;
using cohort.Models;
using cohort.API.Filters;
using cohort.Sqlite;
using tophat;

namespace cohort.Api.Controllers.Internal
{
    [AuthorizeSuperUser]
    public class LogsController : ApiController
    {
        private readonly ILogRepository _repository;

        public LogsController(ILogRepository repository)
        {
            _repository = repository;
        }

        public LogsController() : this(new LogRepository())
        {
            
        }

        public async Task<dynamic> Get()
        {
            return await Task.Factory.StartNew(() =>
            {
                IEnumerable<Log> logs;
                using (var db = UnitOfWork.Current)
                {
                    logs = db.Query<Log>("SELECT * FROM Log");
                }
                var model = new { Logs = logs };
                return model;
            });
        }
    }
}