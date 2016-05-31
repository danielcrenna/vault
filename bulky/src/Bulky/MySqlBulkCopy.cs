using System.Collections.Generic;
using System.Data;
using System.Linq;
using TableDescriptor;
using tuxedo;
using tuxedo.Dialects;

namespace bulky
{
    /// <summary>
    /// Performs high speed bulk inserts against a MySQL database.
    /// <remarks>
    ///     - Note that prepared statements bypass the query cache in MySQL; Flint caches statements by type and batch size
    ///     - This currently doesn't intelligently set the batch size based on packets; the value is arbitrary
    /// </remarks>
    /// <seealso cref="http://stackoverflow.com/questions/2821725/is-that-possible-to-do-bulk-copy-in-mysql"/>
    /// <seealso cref="http://stackoverflow.com/questions/1774142/optimizing-mysql-inserts-to-handle-a-data-stream"/>
    /// <seealso cref="http://stackoverflow.com/questions/2371935/batch-insert-with-mysql-connector-for-net"/> 
    /// </summary>
    public class MySqlBulkCopy : IBulkCopy
    {
        public const int BatchSize = 100;

        private static readonly MySqlDialect Dialect;
        
        static MySqlBulkCopy()
        {
            Dialect = new MySqlDialect();
        }

        public void Copy<T>(Descriptor descriptor, IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            descriptor = descriptor ?? SimpleDescriptor.Create<T>();
            transaction = transaction ?? connection.BeginTransaction();
            var reader = new EnumerableDataReader<T>(entities);
            var total = reader.Count;
            var batchSize = total < BatchSize ? total : BatchSize;
            var pending = new List<object[]>(batchSize);
            var columnCount = descriptor.Insertable.Count();
            var read = 0;
            while (reader.Read())
            {
                read = ReadOne(columnCount, reader, pending, read);
                if (pending.Count >= batchSize)
                {
                    PrepareAndInsert(connection, transaction, descriptor, batchSize, pending);
                }
            }
            if (pending.Count > 0)
            {
                PrepareAndInsert(connection, transaction, descriptor, pending.Count, pending);
            }
            transaction.Commit();
        }

        private static int ReadOne(int columnCount, IDataRecord reader, ICollection<object[]> pending, int read)
        {
            var count = columnCount;
            var parameterValues = new object[count];
            for (var i = 0; i < count; i++)
            {
                parameterValues[i] = reader.GetValue(i);
            }
            pending.Add(parameterValues);
            read++;
            return read;
        }

        private void PrepareAndInsert(IDbConnection connection, IDbTransaction transaction, Descriptor map, int batchSize, ICollection<object[]> pending)
        {
            var command = PrepareCommand(connection, transaction, map, batchSize);
            var index = 0;
            foreach (var item in pending.SelectMany(@group => @group))
            {
                ((IDbDataParameter)command.Parameters[index]).Value = item;
                index++;
            }
            command.Connection = connection;
            command.ExecuteNonQuery();
            pending.Clear();
        }

        private static readonly IDictionary<Descriptor, IDictionary<int, IDbCommand>> InsertCommands = new Dictionary<Descriptor, IDictionary<int, IDbCommand>>();
        private IDbCommand PrepareCommand(IDbConnection connection, IDbTransaction transaction, Descriptor classMap, int batchSize)
        {
            IDictionary<int, IDbCommand> store;
            if (!InsertCommands.TryGetValue(classMap, out store))
            {
                store = new Dictionary<int, IDbCommand>();
                InsertCommands.Add(classMap, store);
            }
            IDbCommand command;
            if (!store.TryGetValue(batchSize, out command))
            {
                command = connection.CreateCommand();
                command.CommandText = MultiValueInsert(classMap, batchSize);
                var properties = classMap.Insertable.ToArray();
                var parameterNames = properties.Select(p => p.ColumnName).ToList();
                var parameters = new List<IDbDataParameter>();
                for (var i = 0; i < batchSize; i++)
                {
                    foreach (var pn in parameterNames)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = string.Concat(pn, "_", i);
                        command.Parameters.Add(parameter);
                        parameters.Add(parameter);
                    }
                }
                command.Transaction = transaction;
                command.Prepare();
                store.Add(batchSize, command);
            }
            return command;
        }

        private static readonly IDictionary<Descriptor, IDictionary<int, string>> MultiValueInserts = new Dictionary<Descriptor, IDictionary<int, string>>();
        public string MultiValueInsert(Descriptor descriptor, int blockSize)
        {
            IDictionary<int, string> store;
            if (!MultiValueInserts.TryGetValue(descriptor, out store))
            {
                store = new Dictionary<int, string>();
                MultiValueInserts.Add(descriptor, store);
            }
            string sql;
            if (!store.TryGetValue(blockSize, out sql))
            {
                var columns = descriptor.Insertable.ToArray();
                var parameters = new List<string>();
                var columnNames = new List<string>();

                var dialect = Tuxedo.Dialect;
                Tuxedo.Dialect = Dialect;
                
                foreach (var column in columns)
                {
                    columnNames.Add(column.ColumnName);
                    for (var i = 0; i < blockSize; i++)
                    {
                        parameters.Add(string.Concat("@", column.ColumnName, "_", i));
                    }
                }
                sql = string.Concat("INSERT INTO ", Tuxedo.TableName(descriptor), " (", columnNames.ConcatQualified(Dialect), ") VALUES ");
                var sqlValues = new List<string>();
                var index = 0;
                for (var i = 0; i < blockSize; i++)
                {
                    var slice = new List<string>();
                    for (var j = 0; j < columns.Length; j++)
                    {
                        slice.Add(parameters[index]);
                    }
                    sqlValues.Add(string.Concat("(", slice.Concat(), ")"));
                    index++;
                }
                sql = string.Concat(sql, sqlValues.Concat());
                store.Add(blockSize, sql);

                Tuxedo.Dialect = dialect;
            }
            return sql;
        }
    }
}