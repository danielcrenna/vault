using System;
using System.Data;
using System.Linq.Expressions;
using Dapper;

namespace tuxedo.Dapper
{
    partial class TuxedoExtensions
    {
        public static int Update<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null, params Expression<Func<T, object>>[] sortOn) where T : class
        {
            var query = Tuxedo.Update(entity);
            var result = connection.Execute(query.Sql, Prepare(query.Parameters), transaction, commandTimeout);
            return result;
        }

        public static int Update<T>(this IDbConnection connection, dynamic set, dynamic where = null, IDbTransaction transaction = null, int? commandTimeout = null, params Expression<Func<T, object>>[] sortOn) where T : class
        {
            Query query = Tuxedo.Update<T>(set, where);
            var result = connection.Execute(query.Sql, Prepare(query.Parameters), transaction, commandTimeout);
            return result;
        }
    }
}
