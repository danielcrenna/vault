using System.Collections.Generic;

namespace tuxedo
{
    public class Query
    {
        public string Sql { get; set; }
        public IDictionary<string, object> Parameters { get; set; }

        public Query(string sql) : this(sql, new Dictionary<string, object>())
        {
            
        }

        public Query(string sql, IDictionary<string, object> parameters)
        {
            Sql = sql;
            Parameters = parameters;
        }
        
        public override string ToString()
        {
            return Sql;
        }
    }
}