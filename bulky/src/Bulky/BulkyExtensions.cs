using System.Collections.Generic;
using System.Data;
using TableDescriptor;

namespace bulky
{
    public static class BulkyExtensions
    {
        public static void BulkCopy<T>(this IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            Bulky.BulkCopier.Copy(SimpleDescriptor.Create<T>(), connection, entities, transaction, commandTimeout);
        }

        public static void BulkCopy<T>(this IDbConnection connection, Descriptor descriptor, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            Bulky.BulkCopier.Copy(SimpleDescriptor.Create<T>(), connection, entities, transaction, commandTimeout);
        }
    }
}