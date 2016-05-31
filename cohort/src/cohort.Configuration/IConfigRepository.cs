using System.Collections.Generic;

namespace cohort.Models
{
    public interface IConfigRepository
    {
        ConfigSetting GetByKey(string key);
        IEnumerable<ConfigSetting> GetAll();
    }
}