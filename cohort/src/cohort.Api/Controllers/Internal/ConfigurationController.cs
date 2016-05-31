using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using cohort.API.Filters;
using cohort.Models;
using cohort.Sqlite;
using tophat;
using Dapper;

namespace cohort.Api.Controllers.Internal
{
    [AuthorizeSuperUser]
    public class ConfigurationController : ApiController
    {
        private readonly IConfigRepository _repository;

        public ConfigurationController(IConfigRepository repository)
        {
            _repository = repository;
        }

        public ConfigurationController() : this(new ConfigRepository())
        {
            
        }

        public async Task<dynamic> Get()
        {
            
            return await Task.Factory.StartNew(() =>
            {
                IEnumerable<ConfigSetting> settings;
                using (var db = UnitOfWork.Current)
                {
                    settings = db.Query<ConfigSetting>("SELECT * FROM Config");
                }
                dynamic model = new { Settings = settings };
                return model;
            });
        }
    }
}