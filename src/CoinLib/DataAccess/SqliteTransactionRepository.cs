using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using ChainLib.DataAccess;
using CoinLib.Extensions;
using CoinLib.Models;

namespace CoinLib.DataAccess
{
    public class SqliteTransactionRepository : SqliteRepository, ITransactionRepository
    {
	    private readonly ILogger<SqliteTransactionRepository> _logger;

        public SqliteTransactionRepository(string @namespace, string databaseName, ILogger<SqliteTransactionRepository> logger) : base(@namespace, databaseName, logger)
        {
	        _logger = logger;
        }

        public IEnumerable<Transaction> StreamAllTransactions()
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                var transactions = db.Query<Transaction>("SELECT t.* FROM 'Transaction' t", buffered: false);

                foreach (var transaction in transactions)
                {
	                TransformTransactionAsync(transaction, db).ConfigureAwait(false).GetAwaiter().GetResult();

					yield return transaction;
                }
            }
        }

        public async Task<Transaction> GetByIdAsync(string id)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                const string sql = "SELECT t.* FROM 'Transaction' t WHERE t.'Id' = @Id";

                var transaction = await db.QuerySingleOrDefaultAsync<Transaction>(sql, new { Id = id });

                await TransformTransactionAsync(transaction, db);

                return transaction;
            }
        }

        private static async Task TransformTransactionAsync(Transaction transaction, IDbConnection db)
        {
	        const string sql = "SELECT i.* " +
							   "FROM 'TransactionItem' i " +
	                           "WHERE i.Type = @Type AND i.TransactionParent = @Id";

	        transaction.Data = new TransactionData
            {
                Inputs = (await db.QueryAsync<TransactionItem>(sql,
                    new { Type = TransactionDataType.Input, transaction.Id })).AsList(),

                Outputs = (await db.QueryAsync<TransactionItem>(sql,
                    new { Type = TransactionDataType.Output, transaction.Id })).AsList()
            };
        }

        public async Task<int> DeleteAsync(IEnumerable<string> ids)
        {
            var deleted = 0;

            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
	            await db.OpenAsync();

	            using (var t = db.BeginTransaction())
	            {
		            var sets = ids.Batch(100);

		            foreach (var set in sets)
		            {
			            var list = set.AsList();

			            await db.ExecuteAsync("DELETE FROM 'TransactionItem' WHERE 'TransactionId' IN @Set", new { Set = list }, t);

			            deleted += await db.ExecuteAsync("DELETE FROM 'Transaction' WHERE 'Id' IN @Set", new { Set = list }, t);
		            }
	            }
            }
            return deleted;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
	            await db.OpenAsync();

	            using (var t = db.BeginTransaction())
	            {
		            await db.ExecuteAsync("DELETE FROM 'TransactionItem' WHERE 'TransactionId' = @Id", new {Id = id}, t);

		            var deleted = await db.ExecuteAsync("DELETE FROM 'Transaction' WHERE 'Id' = @Id", new {Id = id}, t);

		            return deleted == 1;
	            }
            }
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                await db.OpenAsync();

                using (var t = db.BeginTransaction())
                {
                    await db.ExecuteAsync("INSERT INTO 'Transaction' ('Id','Hash','Type') VALUES (@Id,@Hash,@Type);", new
                    {
                        transaction.Id,
                        transaction.Hash,
                        transaction.Type
                    }, t);

                    foreach (var input in transaction.Data?.Inputs ?? Enumerable.Empty<TransactionItem>())
                    {
                        await db.ExecuteAsync("INSERT INTO 'TransactionItem' ('TransactionParent','TransactionId','Type','Index','Address','Amount','Signature') VALUES (@TransactionParent,@TransactionId,@Type,@Index,@Address,@Amount,@Signature);",
                            new
                            {
	                            TransactionParent = transaction.Id,
								input.TransactionId,
                                Type = TransactionDataType.Input,
                                input.Index,
                                input.Address,
                                input.Amount,
                                input.Signature
                            }, t);
                    }

                    foreach (var output in transaction.Data?.Outputs ?? Enumerable.Empty<TransactionItem>())
                    {
                        await db.ExecuteAsync("INSERT INTO 'TransactionItem' ('TransactionParent','TransactionId','Type','Index','Address','Amount','Signature') VALUES (@TransactionParent,@TransactionId,@Type,@Index,@Address,@Amount,@Signature);",
                            new
                            {
								TransactionParent = transaction.Id,
                                output.TransactionId,
                                Type = TransactionDataType.Output,
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

	    public override void MigrateToLatest()
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

CREATE TABLE IF NOT EXISTS 'TransactionItem'
(  
	'TransactionParent' VARCHAR(64) NOT NULL, 
    'TransactionId' VARCHAR(64) NOT NULL, 
    'Type' INTEGER NOT NULL,
    'Index' INTEGER NOT NULL,
    'Amount' INTEGER NOT NULL,
    'Address' BLOB NOT NULL,
    'Signature' BLOB NULL,

    FOREIGN KEY('TransactionParent') REFERENCES 'Transaction'('Id')
);
");
                }
            }
            catch (SqliteException e)
            {
                _logger?.LogError(e, "Error migrating transactions database");
                throw;
            }
        }
    }
}