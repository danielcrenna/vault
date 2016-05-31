using System.Collections.Generic;
using TableDescriptor;
using tuxedo.Extensions;

namespace tuxedo
{
    partial class Tuxedo
    {
        public static Query Insert<T>(T entity)
        {
            var descriptor = GetDescriptor<T>();
            var columnsToInsert = descriptor.Insertable;
            return Insert(entity, descriptor, columnsToInsert);
        }

        private static Query Insert<T>(T entity, Descriptor descriptor, IList<PropertyToColumn> columnsToInsert)
        {
            var sql = InsertSql(descriptor, columnsToInsert);
            var parameters = ParametersFromInstance(entity, descriptor.Insertable);
            return new Query(sql, parameters);
        }

        private static string InsertSql(Descriptor descriptor, IList<PropertyToColumn> columnsToInsert)
        {
            var sql = string.Concat(
                "INSERT INTO ", TableName(descriptor),
                " (", ColumnList(columnsToInsert), ") VALUES (",
                ColumnParameters(columnsToInsert).Concat(),
                ")");
            return sql;
        }
    }
}