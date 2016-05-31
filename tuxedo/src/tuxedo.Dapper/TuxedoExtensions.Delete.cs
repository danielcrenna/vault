using System;
using System.Data;
using System.Linq.Expressions;
using Dapper;

namespace tuxedo.Dapper
{
    partial class TuxedoExtensions
    {
        public static int DeleteAll<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null, params Expression<Func<T, object>>[] sortOn) where T : class
        {
            var query = Tuxedo.DeleteAll<T>();
            return connection.Execute(query.Sql, Prepare(query.Parameters), transaction, commandTimeout);
        }

        public static int Delete<T>(this IDbConnection connection, dynamic where, IDbTransaction transaction = null, int? commandTimeout = null, params Expression<Func<T, object>>[] sortOn) where T : class
        {
            Query query = Tuxedo.Delete<T>(where);
            return connection.Execute(query.Sql, Prepare(query.Parameters), transaction, commandTimeout);
        }
    }
}
