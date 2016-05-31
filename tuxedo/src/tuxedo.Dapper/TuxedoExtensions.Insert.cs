using System.Data;
using System.Linq;
using Dapper;

namespace tuxedo.Dapper
{
    partial class TuxedoExtensions
    {
        public static T Insert<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var descriptor = Tuxedo.GetDescriptor<T>();
            var insert = Tuxedo.Insert(entity);
            var sql = insert.Sql;
            if(descriptor.Identity != null)
            {
                sql = string.Concat(sql, "; ", Tuxedo.Identity());
                var result = connection.Query<int>(sql, Prepare(insert.Parameters), transaction, true, commandTimeout).Single();
                MapBackId(descriptor, entity, result);
            }
            else
            {
                connection.Execute(sql, Prepare(insert.Parameters), transaction, commandTimeout); 
            }
            return entity;
        }
    }
}
