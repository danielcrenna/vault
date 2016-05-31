using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using TableDescriptor;

namespace bulky
{
    /// <summary>
    /// Performs high speed bulk inserts against a SQL Server database.
    /// Optimizations:
    /// - TABLOCK is set
    /// - The batch size is set to the number of rows inserted
    /// - The recovery model is set to bulk-logged
    /// - Page verify is temporarily turned off if it is on
    /// Assumptions (for best performance):
    /// - The target table is not being replicated
    /// - The target table does not have any triggers
    /// - The target table has either 0 rows or no indexes
    /// <seealso href="http://msdn.microsoft.com/en-us/library/aa173854(v=sql.80).aspx" />
    /// <seealso href="http://msdn.microsoft.com/en-us/library/aa178096(v=sql.80).aspx" />
    /// </summary>
    public class SqlServerBulkCopy : IBulkCopy
    {
        public virtual void Copy<T>(Descriptor descriptor, IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            descriptor = descriptor ?? SimpleDescriptor.Create<T>();
            var reader = new EnumerableDataReader<T>(entities);
            var timeout = commandTimeout.HasValue ? commandTimeout.Value : 0;
            var mapping = GenerateBulkCopyMapping(descriptor, reader, connection, timeout);
            
            var settings = BeforeBulkInsert(connection, transaction, timeout);
            try
            {
                using (var bcp = new SqlBulkCopy((SqlConnection)connection, SqlBulkCopyOptions.TableLock, (SqlTransaction)transaction))
                {
                    foreach (var column in mapping.DatabaseTableColumns)
                    {
                        bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column, column));
                    }
                    bcp.BatchSize = reader.Count;
                    bcp.DestinationTableName = !string.IsNullOrWhiteSpace(descriptor.Schema) ? string.Format("[{0}].[{1}]", descriptor.Schema, mapping.DataReaderTable.TableName) : string.Format("[dbo].[{0}]", mapping.DataReaderTable.TableName);
                    bcp.BulkCopyTimeout = timeout;
                    bcp.WriteToServer(reader);
                }
            }
            finally
            {
                AfterBulkInsert(connection, transaction, timeout, settings);    
            }
        }

        public virtual SqlServerSettings BeforeBulkInsert(IDbConnection connection, IDbTransaction transaction, int timeout)
        {
            var database = connection.Database;
            var settings = new SqlServerSettings
            {
                PageVerify = Util.AdHoc(connection, transaction, timeout, string.Format("SELECT page_verify_option_desc FROM sys.databases WHERE [NAME] = '{0}'", database)).ToString(),
                RecoveryModel = Util.AdHoc(connection, transaction, timeout, string.Format("SELECT recovery_model_desc FROM sys.databases WHERE [NAME] = '{0}'", database)).ToString()
            };

            Util.AdHoc(connection, transaction, timeout, "USE master;");
            Util.AdHoc(connection, transaction, timeout, string.Format("ALTER DATABASE [{0}] SET PAGE_VERIFY NONE;", database));
            Util.AdHoc(connection, transaction, timeout, string.Format("ALTER DATABASE [{0}] SET RECOVERY BULK_LOGGED", database));
            Util.AdHoc(connection, transaction, timeout, string.Format("USE [{0}]", database));

            return settings;
        }

        public virtual void AfterBulkInsert(IDbConnection connection, IDbTransaction transaction, int timeout, SqlServerSettings settings)
        {
            var database = connection.Database;
            Util.AdHoc(connection, transaction, timeout, "USE master");
            Util.AdHoc(connection, transaction, timeout, string.Format("ALTER DATABASE [{0}] SET PAGE_VERIFY {1};", database, settings.PageVerify));
            Util.AdHoc(connection, transaction, timeout, string.Format("ALTER DATABASE [{0}] SET RECOVERY {1}", database, settings.RecoveryModel));
            Util.AdHoc(connection, transaction, timeout, string.Format("USE [{0}]", database));
        }
        
        public virtual BulkCopyMapping GenerateBulkCopyMapping(Descriptor descriptor, IDataReader reader, IDbConnection connection, int commandTimeout)
        {
            var schemaTable = reader.GetSchemaTable();
            var schemaTableColumns = (from DataColumn column in schemaTable.Columns select column.ColumnName).ToList();
            var databaseTableColumns = GetDatabaseTableColumns(connection, schemaTable.TableName, commandTimeout).ToList();
            var excludedColumns = descriptor.Computed.Select(c => c.ColumnName).ToList();
            databaseTableColumns = databaseTableColumns.Except(excludedColumns).ToList();
            Debug.Assert(databaseTableColumns.Count() == schemaTableColumns.Count());
            return new BulkCopyMapping
            {
                DataReaderTable = schemaTable,
                DatabaseTableColumns = databaseTableColumns,
                SchemaTableColumns = schemaTableColumns
            };
        }

        public virtual IEnumerable<string> GetDatabaseTableColumns(IDbConnection connection, string tableName, int commandTimeout)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "sp_Columns";
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = commandTimeout;

                //command.Parameters.Add("@table_name", SqlDbType.NVarChar, 384).Value = tableName;
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@table_name";
                parameter.DbType = DbType.String;
                parameter.Size = 384;
                parameter.Value = tableName;
                command.Parameters.Add(parameter);
                
                if (connection.State == ConnectionState.Closed) connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader != null && reader.Read())
                    {
                        yield return (string) reader["column_name"];
                    }
                }
            }
        }
    }
}