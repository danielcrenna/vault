using System.Collections.Generic;
using System.Data;
using TableDescriptor;

namespace bulky
{
    public interface IBulkCopy
    {
        void Copy<T>(Descriptor descriptor, IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null);
    }
}
