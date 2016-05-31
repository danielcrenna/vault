using System.Collections.Generic;
using System.Linq;
using Dapper;
using cohort.Models;
using tophat;

namespace cohort.Sqlite
{
    public class ConfigRepository : IConfigRepository
    {
        public ConfigSetting GetByKey(string key)
        {
            var db = UnitOfWork.Current;
            return db.Query<ConfigSetting>("SELECT * FROM Config WHERE Key = @Key", new { Key = key }).SingleOrDefault();
        }

        public IEnumerable<ConfigSetting> GetAll()
        {
            var db = UnitOfWork.Current;
            return db.Query<ConfigSetting>("SELECT * FROM Config");
        }
    }
}