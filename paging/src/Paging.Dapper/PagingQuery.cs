using System.Collections.Generic;

namespace Paging
{
    public class PagingQuery
    {
        public string CountQuery { get; set; }
        public string PageQuery { get; set; }
        public IDictionary<string, object> Parameters { get; private set; }

        public PagingQuery()
        {
            Parameters = new Dictionary<string, object>();
        }
    }
}