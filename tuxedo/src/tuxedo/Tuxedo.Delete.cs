using TableDescriptor;

namespace tuxedo
{
    partial class Tuxedo
    {
        public static Query DeleteAll<T>()
        {
            return new Query(DeleteFrom(GetDescriptor<T>()));
        }

        public static Query Delete<T>(dynamic @where)
        {
            var descriptor = GetDescriptor<T>();
            var deleteFrom = DeleteFrom(descriptor);
            Query whereClause = WhereClauseByExample(descriptor, @where);
            var sql = string.Concat(deleteFrom, " WHERE ", whereClause);
            return new Query(sql, whereClause.Parameters);
        }

        private static string DeleteFrom(Descriptor descriptor)
        {
            return string.Concat("DELETE FROM ", TableName(descriptor));
        }
    }
}
