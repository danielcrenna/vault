using System.Collections.Generic;
using System.Linq;
using TableDescriptor;
using tuxedo.Extensions;

namespace tuxedo
{
    partial class Tuxedo
    {
        public const string SetSuffix = "_set";
        
        public static Query Update<T>(T entity)
        {
            var descriptor = GetDescriptor<T>();
            var hash = DynamicToHash(entity);
            var setClause = BuildSafeSetClause(descriptor, hash);

            string sql;
            var parameters = UpdateSetClause(setClause, descriptor, out sql);

            Dictionary<string, object> keys;
            if (descriptor.Identity != null)
            {
                keys = new Dictionary<string, object>
                {
                    {descriptor.Identity.ColumnName, descriptor.Identity.Property.Get(entity)}
                };
            }
            else
            {
                keys = descriptor.Keys.ToDictionary(id => id.ColumnName, id => id.Property.Get(entity));
            }
            var whereClause = WhereClauseByExample(descriptor, keys);
            
            sql = string.Concat(sql, " WHERE ", whereClause.Sql);
            return new Query(sql, parameters.AddRange(whereClause.Parameters));
        }

        private static Dictionary<string, object> BuildSafeSetClause(Descriptor descriptor, IDictionary<string, object> hash)
        {
            var setClause = new Dictionary<string, object>();
            foreach (var insertable in descriptor.Insertable)
            {
                object value;
                if (hash.TryGetValue(insertable.ColumnName, out value))
                {
                    setClause.Add(insertable.ColumnName, value);
                }
            }
            return setClause;
        }

        public static Query Update<T>(dynamic set, dynamic @where = null)
        {
            var descriptor = GetDescriptor<T>();
            var setClause = (IDictionary<string, object>)BuildSafeSetClause(descriptor, DynamicToHash(set));

            string sql;
            var parameters = UpdateSetClause(setClause, descriptor, out sql);

            if (@where == null)
            {
                return new Query(sql, parameters);
            }

            Query whereClause = WhereClauseByExample(descriptor, @where);
            sql = string.Concat(sql, " WHERE ", whereClause.Sql);
            return new Query(sql, parameters.AddRange(whereClause.Parameters));
        }

        private static IDictionary<string, object> UpdateSetClause(IDictionary<string, object> setClause, Descriptor descriptor, out string sql)
        {
            var parameters = ParametersFromHash(setClause, suffix: SetSuffix);
            var setColumns = ColumnsFromHash(descriptor, setClause);
            sql = string.Concat("UPDATE ", TableName(descriptor), " SET ", ColumnParameterClauses(setColumns, SetSuffix).Concat());
            return parameters;
        }
    }
}
