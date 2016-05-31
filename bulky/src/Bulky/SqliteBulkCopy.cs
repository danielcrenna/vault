using System.Collections.Generic;
using System.Data;
using System.Linq;
using TableDescriptor;
using tuxedo;
using tuxedo.Dialects;

namespace bulky
{
    /// <summary>
    /// Performs high speed bulk inserts against a SQLite database.
    /// </summary>
    public class SqliteBulkCopy : IBulkCopy
    {
        private static readonly SqliteDialect Dialect;

        static SqliteBulkCopy()
        {
            Dialect = new SqliteDialect();
        }

        public void Copy<T>(Descriptor descriptor, IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (entities == null || !entities.Any())
            {
                return;
            }

            var settings = BeforeBulkInsert(connection, transaction, commandTimeout);
            try
            {
                descriptor = descriptor ?? SimpleDescriptor.Create<T>();
                var command = connection.CreateCommand();
                transaction = transaction ?? connection.BeginTransaction();

                SetCommandText<T>(command, entities.First());

                var properties = descriptor.Insertable.ToArray();
                var parameterNames = properties.Select(p => p.ColumnName);
                var parameters = parameterNames.Select(pn =>
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = pn;
                    command.Parameters.Add(parameter);
                    return parameter;
                }).ToArray();

                command.Transaction = transaction;
                command.Prepare();
                
                var reader = new EnumerableDataReader<T>(entities);
                while (reader.Read())
                {
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        parameters[i].Value = reader.GetValue(i);
                    }
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            finally
            {
                AfterBulkInsert(connection, settings);
            }
        }

        private static void SetCommandText<T>(IDbCommand command, T example)
        {
            var dialect = Tuxedo.Dialect;
            Tuxedo.Dialect = Dialect;
            command.CommandText = Tuxedo.Insert(example).Sql;
            Tuxedo.Dialect = dialect;
        }

        private static SqliteSettings BeforeBulkInsert(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var settings = new SqliteSettings
            {
                Synchronous = Util.AdHoc(connection, transaction, commandTimeout.HasValue ? commandTimeout.Value : 0, "PRAGMA synchronous").ToString(),
                JournalMode = Util.AdHoc(connection, transaction, commandTimeout.HasValue ? commandTimeout.Value : 0, "PRAGMA journal_mode").ToString()
            }; 
            var pragma = connection.CreateCommand();
            pragma.CommandText = "PRAGMA synchronous = OFF";
            pragma.ExecuteNonQuery();
            pragma.CommandText = "PRAGMA journal_mode = MEMORY";
            pragma.ExecuteNonQuery();
            return settings;
        }

        private static void AfterBulkInsert(IDbConnection connection, SqliteSettings settings)
        {
            var pragma = connection.CreateCommand();
            pragma.CommandText = "PRAGMA synchronous = " + settings.Synchronous;
            pragma.ExecuteNonQuery();
            pragma.CommandText = "PRAGMA journal_mode = " + settings.JournalMode;
            pragma.ExecuteNonQuery();
        }
    }
}