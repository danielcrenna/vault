using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace Paging
{
    /// <summary>
    /// Derived from: https://github.com/toptensoftware/PetaPoco
    /// </summary>
    internal static class SqlBuilder
    {
        // Hashtable has better read-without-locking semantics than a dictionary (/ht @MarcGravell)
        private static readonly Hashtable QueryCache = new Hashtable();
        private static PagingQuery GetOrCreateQueryFor(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException("sql");
            var query = (PagingQuery)QueryCache[sql];
            if (query != null) return query;
            lock (QueryCache)
            {
                query = (PagingQuery)QueryCache[sql];
                if (query != null)
                {
                    return query;
                }
                query = CreateQueryFor(sql);
                QueryCache[sql] = query;
                return query;
            }
        }
        
        public static PagingQuery Page(string sql, int itemIndex, int pageSize)
        {
            var query = GetOrCreateQueryFor(sql);
            switch (PagingExtensions.Dialect)
            {
                case SqlDialect.SqlServer:
                    query.Parameters.Add("@RowStart", itemIndex);
                    query.Parameters.Add("@RowEnd", itemIndex + pageSize);
                    break;
                case SqlDialect.MySql:
                case SqlDialect.Sqlite:
                default:
                    query.Parameters.Add("@PageSize", pageSize);
                    query.Parameters.Add("@ItemIndex", itemIndex);
                    break;
            }
            return query;
        }

        private static PagingQuery CreateQueryFor(string sql)
        {
            // Reduce unnecessary work here...
            var query = new PagingQuery();
            string @select, @orderBy, sqlCount;
            if (!SplitSqlForPaging(sql, out sqlCount, out @select, out @orderBy))
            {
                throw new Exception("Unable to parse SQL statement for paged query");
            }
            query.CountQuery = sqlCount;

            switch (PagingExtensions.Dialect)
            {
                case SqlDialect.SqlServer:
                    RowOverPageQuery(@select, @orderBy, query);
                    break;
                case SqlDialect.MySql:
                case SqlDialect.Sqlite:
                default:
                    query.PageQuery = string.Concat(sql, " LIMIT @PageSize OFFSET @ItemIndex");
                    break;
            }
            return query;
        }

        private static void RowOverPageQuery(string @select, string @orderBy, PagingQuery query)
        {
            const string inner = "page_inner";
            @select = SqlRegex.OrderBy.Replace(@select, "").Trim();
            if (SqlRegex.Distinct.IsMatch(@select))
            {
                @select = string.Concat(inner, ".* FROM (SELECT ", @select, ") ", inner);
            }
            @orderBy = @orderBy ?? "ORDER BY (SELECT NULL)";
            query.PageQuery = string.Concat(
                "SELECT * FROM (SELECT ROW_NUMBER() OVER (", @orderBy,
                ") [RowNumber], ", @select,
                ") [PageData] WHERE [RowNumber] > @RowStart AND [RowNumber] <= @RowEnd"
                );
        }
        
        private static bool SplitSqlForPaging(string sql, out string sqlCount, out string sqlSelectRemoved, out string sqlOrderBy)
        {
            sqlSelectRemoved = null;
            sqlCount = null;
            sqlOrderBy = null;

            // Extract the columns from "SELECT <whatever> FROM"
            var m = SqlRegex.SelectColumns.Match(sql);
            if (!m.Success)
            {
                return false;
            }

            // Save column list and replace with COUNT(*)
            var g = m.Groups[1];
            sqlSelectRemoved = sql.Substring(g.Index);

            var @select = sql.Substring(0, g.Index);
            var @from = sql.Substring(g.Index + g.Length);
            sqlCount = SqlRegex.Distinct.IsMatch(sqlSelectRemoved)
                           ? GetDistinctCount(@select, g, @from)
                           : string.Concat(@select, "COUNT(*) ", @from);

            // Look for an "ORDER BY <whatever>" clause
            m = SqlRegex.OrderBy.Match(sqlCount);
            if (m.Success)
            {
                g = m.Groups[0];
                sqlOrderBy = g.ToString();
                sqlCount = sqlCount.Substring(0, g.Index) + sqlCount.Substring(g.Index + g.Length);
            }
            return true;
        }

        private static string GetDistinctCount(string @select, Group g, string @from)
        {
            var columns = g.ToString().Trim();
            if(columns == "DISTINCT")
            {
                throw new ArgumentException("Malformed SQL; DISTINCT queries must specify at least one column");
            }
            var distinct = string.Concat(@select, "COUNT(", columns,  ") ", @from);
            return distinct;
        }
    }
}