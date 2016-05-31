using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using TableDescriptor;
using tuxedo.Dialects;
using tuxedo.Extensions;

namespace tuxedo
{
    public partial class Tuxedo
    {
        public static Dialect Dialect { get; set; }
        public static Func<Type, Descriptor> DescriptorFunction = type => SimpleDescriptor.Create(type);

        internal static Descriptor GetDescriptor<T>()
        {
            return DescriptorFunction(typeof(T));
        }

        static Tuxedo()
        {
            Dialect = new SqlServerDialect();
        }

        public static Query Identity()
        {
            var identity = Dialect.Identity;
            return new Query(identity);
        }
        
        private static IEnumerable<PropertyToColumn> ColumnsFromHash(Descriptor descriptor, IDictionary<string, object> hash)
        {
            return descriptor.All.Where(c => hash.ContainsKey(c.ColumnName));
        }

        private static IDictionary<string, object> ParametersFromHash(IEnumerable<KeyValuePair<string, object>> hash, IDictionary<string, object> parent = null, string suffix = "")
        {
            var parameters = parent ?? new Dictionary<string, object>();
            foreach (var entry in hash)
            {
                parameters.Add(string.Concat("@", entry.Key, suffix), entry.Value);
            }
            return parameters;
        }

        private static IDictionary<string, object> ParametersFromInstance(dynamic entity, IEnumerable<PropertyToColumn> columns)
        {
            var hash = (IDictionary<string, object>)DynamicToHash(entity);
            var keys = columns.Select(c => c.ColumnName).Intersect(hash.Keys).ToList();
            var result = keys.ToDictionary(key => string.Concat("@", key), key => hash[key]);
            return result;
        }
        
        private static IEnumerable<string> ColumnParameterClauses(IEnumerable<PropertyToColumn> columns, string suffix = "")
        {
            return columns.Select(column => string.Concat(column.ColumnName.Qualify(Dialect), " = @", column.ColumnName, suffix));
        }

        private static IEnumerable<string> ColumnParameters(IEnumerable<PropertyToColumn> columns, string suffix = "")
        {
            return columns.Select(column => string.Concat("@", column.ColumnName, suffix));
        }

        public static string TableName(Descriptor descriptor)
        {
            return new[] {descriptor.Schema, descriptor.Table}.ConcatQualified(Dialect, new string(Dialect.Separator, 1));
        }

        public static string Column(PropertyToColumn column)
        {
            return column.ColumnName.Qualify(Dialect);
        }

        public static string ColumnList(IEnumerable<PropertyToColumn> columns)
        {
            return columns.Select(p => p.ColumnName).ConcatQualified(Dialect);
        }

        private static IDictionary<string, object> DynamicToHash(dynamic example)
        {
            if (example is IDynamicMetaObjectProvider)
            {
                var hash = example as ExpandoObject;
                if (hash != null)
                {
                    return (IDictionary<string, object>)example;
                }
            }
            return StaticToHash(example);
        }

        private static readonly IDictionary<Type, List<string>> TypeMembers = new Dictionary<Type, List<string>>();
        private static IDictionary<string, object> StaticToHash(object example)
        {
            var accessor = TypeAccessor.Create(example.GetType());
            var type = example.GetType();
            if (example.Implements<IDictionary<string, object>>()) return (IDictionary<string, object>)example;
            List<string> members;
            if (!TypeMembers.TryGetValue(type, out members))
            {
                members = accessor.PropertyInfos.Select(p => p.Name).ToList();
                members.AddRange(accessor.FieldInfos.Select(f => f.Name));
                TypeMembers.Add(type, members);
            }
            return members.ToDictionary(member => member, member => accessor[example, member]);
        }

        private static Query WhereClauseByExample(Descriptor descriptor, dynamic @where)
        {
            var hash = (IDictionary<string, object>)DynamicToHash(@where);
            var exampleColumns = ColumnsFromHash(descriptor, hash);
            var parameters = ParametersFromHash(hash);
            var whereClause = ColumnParameterClauses(exampleColumns).Concat(" AND ");
            return new Query(whereClause, parameters);
        }
    }
}