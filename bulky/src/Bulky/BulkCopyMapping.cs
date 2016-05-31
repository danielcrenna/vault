using System.Collections.Generic;
using System.Data;

namespace bulky
{
    public class BulkCopyMapping
    {
        public DataTable DataReaderTable { get; set; }
        public IEnumerable<string> SchemaTableColumns { get; set; }
        public IEnumerable<string> DatabaseTableColumns { get; set; }
    }
}