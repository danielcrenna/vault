using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NaiveCoin.Core.Helpers;
using NaiveCoin.Models;

namespace NaiveCoin.DataAccess
{
    public class SqliteBlockRepository : SqliteRepository, IBlockRepository
    {
        private readonly ILogger<SqliteBlockRepository> _logger;

        public SqliteBlockRepository(string @namespace, string databaseName, ILogger<SqliteBlockRepository> logger) : base(@namespace, databaseName, logger)
        {
            _logger = logger;
        }

        public async Task<long> GetLengthAsync()
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
	            const string sql = "SELECT MAX(b.'Index') FROM 'Block' b";

	            return (await db.QuerySingleOrDefaultAsync<long?>(sql))
					.GetValueOrDefault(0L);
            }
        }

        public async Task<Block> GetByTransactionIdAsync(string transactionId)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
	            const string sql = "SELECT b.* FROM 'Block' b " +
	                               "LEFT JOIN 'BlockTransaction' bt ON bt.'BlockIndex' = b.'Index' " +
	                               "WHERE bt.'Id' = @Id";

	            var block = await db.QuerySingleOrDefaultAsync<Block>(sql, new { Id = transactionId });

                await TransformBlockAsync(block, db);

                return block;
            }
        }

        public void Add(Block block)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                db.Open();

                using (var t = db.BeginTransaction())
                {
                    var index = db.QuerySingle<long>(
                        "INSERT INTO 'Block' ('PreviousHash','Timestamp','Nonce','Hash') VALUES (@PreviousHash,@Timestamp,@Nonce,@Hash); " +
                        "SELECT LAST_INSERT_ROWID();", block, t);

                    foreach (var transaction in block.Transactions)
                    {
                        db.Execute("INSERT INTO 'BlockTransaction' ('BlockIndex','Id','Hash','Type') VALUES (@BlockIndex,@Id,@Hash,@Type);",
                        new
                        {
                            BlockIndex = index,
                            transaction.Id,
                            transaction.Hash,
                            transaction.Type
                        }, t);

                        foreach (var input in transaction.Data?.Inputs ?? Enumerable.Empty<TransactionItem>())
                        {
                            db.Execute("INSERT INTO 'BlockTransactionData' ('TransactionId','Type','Index','Address','Amount','Signature') VALUES (@TransactionId,@Type,@Index,@Address,@Amount,@Signature);",
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
                            db.Execute("INSERT INTO 'BlockTransactionData' ('TransactionId','Type','Index','Address','Amount','Signature') VALUES (@TransactionId,@Type,@Index,@Address,@Amount,@Signature);",
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
                    }

                    t.Commit();

                    block.Index = index;
                }
            }
        }
        
        public async Task<IEnumerable<TransactionItem>> GetTransactionItemsForAddressAsync(TransactionDataType type, string address)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                const string sql = "SELECT i.* " +
                                   "FROM 'BlockTransactionItem' i " +
                                   "WHERE i.'Address' = @Address " +
                                   "AND i.'Type' = @Type";

                var items = await db.QueryAsync<TransactionItem>(sql, new
                {
                    Address = address.FromHex(),
                    Type = type
                });

                return items;
            }
        }


	    public IEnumerable<string> StreamAllTransactionIds()
	    {
		    using (var db = new SqliteConnection($"Data Source={DataFile}"))
		    {
			    const string sql = "SELECT bt.'Id' " +
			                       "FROM 'Block' b " +
			                       "INNER JOIN 'BlockTransaction' bt ON bt.'BlockIndex' = b.'Index' " +
			                       "ORDER BY b.'Index'";

			    var ids = db.Query<string>(sql, buffered: false);

			    return ids;
		    }
	    }

		public IEnumerable<BlockObject> StreamAllBlockObjects()
	    {
			using (var db = new SqliteConnection($"Data Source={DataFile}"))
			{
				const string sql = "SELECT bo.* " +
				                   "FROM 'Block' b " +
				                   "INNER JOIN 'BlockObject' bo ON bo.'BlockIndex' = b.'Index' " +
				                   "ORDER BY b.'Index', bo.'Index'";

				var objects = db.Query<BlockObject>(sql, buffered: false);

				return objects;
			}
		}

	    public IEnumerable<Block> StreamAllBlocks()
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                const string sql = "SELECT b.* " +
                                   "FROM 'Block' b " +
                                   "ORDER BY b.'Index' ASC";

                foreach (var block in db.Query<Block>(sql, buffered: false))
                {
                    TransformBlockAsync(block, db).ConfigureAwait(false).GetAwaiter().GetResult();

                    yield return block;
                }
            }
        }

        public async Task<Block> GetByIndexAsync(long index)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
	            const string sql = "SELECT b.* FROM 'Block' b WHERE b.'Index' = @Index";

	            var block = await db.QuerySingleOrDefaultAsync<Block>(sql, new {Index = index});

                await TransformBlockAsync(block, db);

                return block;
            }
        }

        public async Task<Block> GetByHashAsync(string hash)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
	            const string sql = "SELECT b.* FROM 'Block' b WHERE b.'Hash' = @Hash";

	            var block = await db.QuerySingleOrDefaultAsync<Block>(sql, new { Hash = hash });

                await TransformBlockAsync(block, db);

                return block;
            }
        }

        public async Task<Block> GetLastBlockAsync()
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
	            const string sql = "SELECT COUNT(1), b.* " +
	                               "FROM 'Block' b " +
	                               "GROUP BY b.'Index' " +
	                               "ORDER BY b.'Index' DESC LIMIT 1";

	            var block = await db.QuerySingleOrDefaultAsync<Block>(sql);

                await TransformBlockAsync(block, db);
                
                return block;
            }
        }

        private static async Task TransformBlockAsync(Block block, IDbConnection db)
        {
            if (block == null)
                return;

	        const string objectsSql = "SELECT bo.* " +
	                                  "FROM 'Block' b " +
	                                  "INNER JOIN 'BlockObject' bo ON bo.'BlockIndex' = b.'Index' " +
	                                  "ORDER BY b.'Index', bo.'Index'";

	        block.Objects = (await db.QueryAsync<BlockObject>(objectsSql, new {block.Index})).AsList();

            const string transactionsSql = "SELECT t.* " +
	                                       "FROM 'BlockTransaction' t " +
	                                       "WHERE t.'BlockIndex' = @Index";
			
	        block.Transactions = new List<Transaction>();

			var transactions = await db.QueryAsync<Transaction>(transactionsSql, new { block.Index });
			
			foreach (var transaction in transactions)
            {
	            const string transactionItemSql = "SELECT i.* " +
	                                              "FROM 'BlockTransactionItem' i " +
	                                              "WHERE i.Type = @Type AND i.TransactionId = @Id";

				transaction.Data = new TransactionData
                {
                    Inputs = (await db.QueryAsync<TransactionItem>(transactionItemSql,
                        new { Type = TransactionDataType.Input, transaction.Id })).ToArray(),

                    Outputs = (await db.QueryAsync<TransactionItem>(transactionItemSql,
                        new { Type = TransactionDataType.Output, transaction.Id })).ToArray()
                };

                block.Transactions.Add(transaction);
            }
        }

        protected override void MigrateToLatest()
        {
            try
            {
                using (var db = new SqliteConnection($"Data Source={DataFile}"))
                {
                    db.Execute(@"
CREATE TABLE IF NOT EXISTS 'Block'
(  
    'Index' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
    'PreviousHash' VARCHAR(64) NOT NULL, 
    'Timestamp' INTEGER NOT NULL,
    'Nonce' INTEGER NOT NULL,
    'Hash' VARCHAR(64) NOT NULL
);

CREATE TABLE IF NOT EXISTS 'BlockObject'
(
	'BlockIndex' INTEGER NOT NULL,
	'Index' INTEGER NOT NULL,
	'Timestamp' INTEGER NOT NULL,
	'Data' BLOB NOT NULL,
	'Hash' VARCHAR(64) NOT NULL
);

CREATE TABLE IF NOT EXISTS 'BlockTransaction'
(  
    'BlockIndex' INTEGER NOT NULL,
    'Id' VARCHAR(64) NOT NULL PRIMARY KEY,
    'Type' INTEGER NOT NULL,
	'Hash' VARCHAR(64) NOT NULL,

    FOREIGN KEY('BlockIndex') REFERENCES Block('Index')
);

CREATE TABLE IF NOT EXISTS 'BlockTransactionItem'
(  
    'TransactionId' VARCHAR(64) NOT NULL PRIMARY KEY, 
    'Type' INTEGER NOT NULL,
    'Index' INTEGER NOT NULL,
    'Amount' INTEGER NOT NULL,
    'Address' BLOB NOT NULL,
    'Signature' BLOB NOT NULL,

    FOREIGN KEY('TransactionId') REFERENCES BlockTransaction('Id')
);
");
                }
            }
            catch (SqliteException e)
            {
                _logger?.LogError(e, "Error migrating blocks database");
                throw;
            }
        }
    }
}
