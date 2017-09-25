using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NaiveCoin.Extensions;
using NaiveCoin.Models;

namespace NaiveCoin.DataAccess
{
    public class SqliteTransactionRepository : SqliteRepository, ITransactionRepository
    {
        private readonly ITransactionDataSerializer _dataSerializer;
        private readonly ILogger<SqliteTransactionRepository> _logger;

        public SqliteTransactionRepository(string @namespace, string databaseName, ITransactionDataSerializer dataSerializer, ILogger<SqliteTransactionRepository> logger) : base(@namespace, databaseName, logger)
        {
            _dataSerializer = dataSerializer;
            _logger = logger;
        }

        public IEnumerable<Transaction> GetAll()
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                var transactions = db.Query<Transaction>("SELECT t.* FROM 'Transaction' t", buffered: false);

                foreach (var transaction in transactions)
                {
                    yield return TransformTransaction(transaction, db);
                }
            }
        }

        public Transaction GetById(string id)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                const string sql = "SELECT t.* FROM 'Transaction' t WHERE t.'Id' = @Id";

                var transaction = db.QuerySingleOrDefault<Transaction>(sql, new { Id = id });

                TransformTransaction(transaction, db);

                return transaction;
            }
        }

        private static Transaction TransformTransaction(Transaction transaction, IDbConnection db)
        {
            transaction.Data = new TransactionData
            {
                Inputs = db.Query<TransactionItem>("SELECT i.* " +
                                                    "FROM 'TransactionData' i " +
                                                    "WHERE i.Type = @Type AND i.TransactionId = @Id",
                    new { Type = TransactionDataType.Input, transaction.Id }).ToArray(),

                Outputs = db.Query<TransactionItem>("SELECT o.* " +
                                                    "FROM 'TransactionData' o " +
                                                    "WHERE o.Type = @Type AND o.TransactionId = @Id",
                    new { Type = TransactionDataType.Output, transaction.Id }).ToArray(),
            };

            return transaction;
        }

        public int Delete(IEnumerable<string> ids)
        {
            var deleted = 0;
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                var sets = ids.Split().ToList();

                foreach (var set in sets)
                {
                    db.Execute("DELETE FROM 'TransactionData' WHERE 'TransactionId' IN @Set", new { Set = set });

                    deleted += db.Execute("DELETE FROM 'Transaction' WHERE 'Id' IN @Set", new {Set = set});
                }
            }
            return deleted;
        }

        public bool Delete(string id)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                db.Execute("DELETE FROM 'TransactionData' WHERE 'TransactionId' = @Id", new { Id = id });

                var deleted = db.Execute("DELETE FROM 'Transaction' WHERE 'Id' = @Id", new {Id = id});

                return deleted == 1;
            }
        }

        public void Add(Transaction transaction)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                db.Open();

                using (var t = db.BeginTransaction())
                {
                    db.Execute("INSERT INTO 'Transaction' ('Id','Hash','Type','Data') VALUES (@Id,@Hash,@Type,@Data);", new
                    {
                        transaction.Id,
                        transaction.Hash,
                        transaction.Type,
                        Data = _dataSerializer.Serialize(transaction.Data)
                    }, t);

                    foreach (var input in transaction.Data?.Inputs ?? Enumerable.Empty<TransactionItem>())
                    {
                        db.Execute("INSERT INTO 'TransactionData' ('TransactionId','Type','Index','Address','Amount','Signature') VALUES (@TransactionId,@Type,@Index,@Address,@Amount,@Signature);",
                            new
                            {
                                input.TransactionId,
                                TransactionDataType.Input,
                                input.Index,
                                input.Address,
                                input.Amount,
                                input.Signature
                            }, t);
                    }

                    foreach (var output in transaction.Data?.Outputs ?? Enumerable.Empty<TransactionItem>())
                    {
                        db.Execute("INSERT INTO 'TransactionData' ('TransactionId','Type','Index','Address','Amount','Signature') VALUES (@TransactionId,@Type,@Index,@Address,@Amount,@Signature);",
                            new
                            {
                                output.TransactionId,
                                TransactionDataType.Output,
                                output.Index,
                                output.Address,
                                output.Amount,
                                output.Signature
                            }, t);
                    }

                    t.Commit();
                }
            }
        }

        protected override void MigrateToLatest()
        {
            try
            {
                using (var db = new SqliteConnection($"Data Source={DataFile}"))
                {
                    db.Execute(@"
CREATE TABLE IF NOT EXISTS 'Transaction'
(  
    'Id' VARCHAR(64) NOT NULL PRIMARY KEY, 
    'Hash' VARCHAR(64) NOT NULL,
    'Type' INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS 'TransactionData'
(  
    'TransactionId' VARCHAR(64) NOT NULL PRIMARY KEY, 
    'Type' INTEGER NOT NULL,
    'Index' INTEGER NOT NULL,
    'Amount' INTEGER NOT NULL,
    'Address' BLOB NOT NULL,
    'Signature' BLOB NOT NULL,

    FOREIGN KEY('TransactionId') REFERENCES 'Transaction'('Id')
);
");
                }
            }
            catch (SqliteException e)
            {
                _logger?.LogError(e, "Error migrating transactions table");
                throw;
            }
        }
    }
}