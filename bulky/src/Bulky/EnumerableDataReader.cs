using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TableDescriptor;

namespace bulky
{
    /// <summary>
    /// A data reader that wraps an <see cref="IEnumerable{T}" /> sequence.
    /// <remarks>
    ///     - Since bulk copying sometimes requires the total record count to optimize batch sizes, it's up to the enumerable to avoid re-enumerating when Count() is requested.
    /// </remarks>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumerableDataReader<T> : IDataReader
    {
        private IEnumerable<T> _enumerable;
        private object[] _record;
        private readonly PropertyToColumn[] _properties;
        private readonly string _tableName;
        private readonly IEnumerator<T> _enumerator;

        public int Count
        {
            get { return _enumerable.Count(); }
        }

        public EnumerableDataReader(IEnumerable<T> entities) : this()
        {
            _enumerable = new ConcurrentQueue<T>(entities);
            _enumerator = _enumerable.GetEnumerator();
        }
        
        public EnumerableDataReader() : this(SimpleDescriptor.Create<T>())
        {
            
        }

        public EnumerableDataReader(Descriptor descriptor)
        {
            _tableName = descriptor.Table;
            _properties = descriptor.Insertable.ToArray();
            FieldCount = _properties.Length;
        }
        
        public DataTable GetSchemaTable()
        {
            var table = new DataTable();
            foreach (var property in _properties)
            {
                var columnName = property.ColumnName;
                var columnType = property.Property.Type;
                var fieldType = columnType;
                var isNullable = IsNullable(columnType);
                if (isNullable)
                {
                    fieldType = Nullable.GetUnderlyingType(columnType);
                }
                var column = table.Columns.Add(columnName, fieldType);
                column.AllowDBNull = isNullable;
                column.ReadOnly = true;
            }
            table.TableName = _tableName;
            return table;
        }

        public static bool IsNullable(Type type)
        {
            return type.IsValueType &&
                   type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public bool Read()
        {
            if (!_enumerator.MoveNext())
            {
                return false;
            }
            var next = _enumerator.Current;
            var properties = _properties;
            _record = new object[properties.Length];
            for (var i = 0; i < _record.Length; i++)
            {
                var propertyMap = properties[i];
                _record[i] = propertyMap.Property.Get(next);
            }
            return true;
        }

        private TValue GetValue<TValue>(int i)
        {
            var value = GetValue(i);
            if (value == DBNull.Value && !IsNullable(typeof(TValue)))
            {
                throw new InvalidOperationException("Value is not nullable");
            }
            return (TValue)value;
        }

        public int GetOrdinal(string name)
        {
            var map = _properties.FirstOrDefault(p => p.ColumnName == name);
            if (map == null)
            {
                throw new InvalidOperationException(String.Format("No property found with name '{0}'", name));
            }
            for (var i = 0; i < _properties.Length; i++)
            {
                if (_properties[i] == map)
                {
                    return i;
                }
            }
            return -1;
        }

        public int GetValues(object[] values)
        {
            var columns = 0;
            for (var i = 0; i < values.Length && i < FieldCount; i++)
            {
                columns += 1;
                values[i] = GetValue(i);
            }
            return columns;
        }

        public int FieldCount { get; private set; }

        public void Close()
        {
            _enumerable = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                Close();
            }
        }
        
        #region Not Supported
        private static void NotUsedByBulkCopy()
        {
            throw new NotSupportedException("This method does not apply to in-memory collections, which is all this class supports");
        }

        public int RecordsAffected
        {
            get
            { 
                NotUsedByBulkCopy();
                return 0;
            }
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            NotUsedByBulkCopy();
            return 0;
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            NotUsedByBulkCopy();
            return 0;
        }
        public IDataReader GetData(int i)
        {
            NotUsedByBulkCopy();
            return null;
        }
        #endregion

        #region Passthrough

        public object this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        public object this[int i]
        {
            get { return GetValue(i); }
        }

        public bool NextResult()
        {
            return false;
        }

        public int Depth
        {
            get { return 1; }
        }

        public bool IsClosed
        {
            get { return _enumerable == null; }
        }

        public object GetValue(int i)
        {
            return _record[i];
        }
        
        public string GetName(int i)
        {
            return _properties[i].ColumnName;
        }

        public string GetDataTypeName(int i)
        {
            return GetFieldType(i).Name;
        }

        public Type GetFieldType(int i)
        {
            return _properties[i].Property.Type;
        }

        public bool GetBoolean(int i)
        {
            return GetValue<bool>(i);
        }

        public byte GetByte(int i)
        {
            return GetValue<byte>(i);
        }

        public char GetChar(int i)
        {
            return GetValue<char>(i);
        }

        public Guid GetGuid(int i)
        {
            return GetValue<Guid>(i);
        }

        public short GetInt16(int i)
        {
            return GetValue<short>(i);
        }

        public int GetInt32(int i)
        {
            return GetValue<int>(i);
        }

        public long GetInt64(int i)
        {
            return GetValue<long>(i);
        }

        public float GetFloat(int i)
        {
            return GetValue<float>(i);
        }

        public double GetDouble(int i)
        {
            return GetValue<double>(i);
        }

        public string GetString(int i)
        {
            return GetValue<string>(i);
        }

        public decimal GetDecimal(int i)
        {
            return GetValue<decimal>(i);
        }

        public DateTime GetDateTime(int i)
        {
            return GetValue<DateTime>(i);
        }

        public bool IsDBNull(int i)
        {
            return (GetValue(i) == DBNull.Value);
        }
        #endregion
    }
}
