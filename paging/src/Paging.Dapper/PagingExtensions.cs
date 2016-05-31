using System.Collections.Generic;
using System.Data;
using Dapper;

namespace Paging
{
    public static class PagingExtensions
    {
        public static SqlDialect Dialect { get; set; }

        public static IPagedEnumerable<T> Query<T>(this IDbConnection connection, string sql, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?), int? page = default(int?), int? count = default(int?))
        {
            var pageSize = count ?? Paging.DefaultPageSize;
            var pageIndex = ((page ?? 1) - 1) * pageSize;

            var query = SqlBuilder.Page(sql, pageIndex, pageSize);
            var prepared = Prepare(param);
            foreach(var parameter in query.Parameters)
            {
                prepared.Add(parameter.Key, parameter.Value);
            }

            var total = connection.Execute(query.CountQuery);
            var results = SqlMapper.Query<T>(connection, query.PageQuery, prepared, transaction, buffered, commandTimeout,commandType);
            
            return new PagedEnumerable<T>(results, pageIndex, pageSize, total);
        }

        private static DynamicParameters Prepare(this IEnumerable<KeyValuePair<string, object>> parameters)
        {
            var result = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                result.Add(parameter.Key, parameter.Value);
            }
            return result;
        }
    }
}
