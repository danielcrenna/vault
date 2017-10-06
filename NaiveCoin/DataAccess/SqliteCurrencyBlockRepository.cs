using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NaiveChain.DataAccess;
using NaiveChain.Models;
using NaiveCoin.Core.Helpers;
using NaiveCoin.Core.Providers;
using NaiveCoin.Extensions;
using NaiveCoin.Models;

namespace NaiveCoin.DataAccess
{
    public class SqliteCurrencyBlockRepository : SqliteRepository, ICurrencyBlockRepository
    {
	    private readonly IOptions<CoinSettings> _coinSettings;
	    private readonly IHashProvider _hashProvider;
	    private readonly IBlockObjectSerializer _serializer;
	    private readonly ILogger<SqliteCurrencyBlockRepository> _logger;

        public SqliteCurrencyBlockRepository(
			string @namespace, 
			string databaseName,
			IOptions<CoinSettings> coinSettings,
			IHashProvider hashProvider,
			IBlockObjectSerializer serializer,
			ILogger<SqliteCurrencyBlockRepository> logger) : base(@namespace, databaseName, logger)
        {
	        _coinSettings = coinSettings;
	        _hashProvider = hashProvider;
	        _serializer = serializer;
	        _logger = logger;
        }

	    public Task<CurrencyBlock> GetGenesisBlockAsync()
	    {
			foreach (var transaction in _coinSettings.Value.GenesisBlock.Transactions ?? Enumerable.Empty<Transaction>())
			    transaction.Hash = transaction.ToHash(_hashProvider);
		    _coinSettings.Value.GenesisBlock.Hash = _coinSettings.Value.GenesisBlock.ToHash(_hashProvider);
		    return Task.FromResult(_coinSettings.Value.GenesisBlock);
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

        public async Task<CurrencyBlock> GetByTransactionIdAsync(string transactionId)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
	            const string sql = "SELECT b.* FROM 'Block' b " +
	                               "LEFT JOIN 'BlockTransaction' bt ON bt.'BlockIndex' = b.'Index' " +
	                               "WHERE bt.'Id' = @Id";

	            var block = await db.QuerySingleOrDefaultAsync<CurrencyBlock>(sql, new { Id = transactionId });

                await TransformBlockAsync(block, db);

                return block;
            }
        }

        public async Task AddAsync(CurrencyBlock block)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                await db.OpenAsync();

                using (var t = db.BeginTransaction())
                {
                    var index = await db.QuerySingleAsync<long>(
                        "INSERT INTO 'Block' ('PreviousHash','Timestamp','Nonce','Hash') VALUES (@PreviousHash,@Timestamp,@Nonce,@Hash); " +
                        "SELECT LAST_INSERT_ROWID();", block, t);

	                foreach (var @object in block.Objects ?? Enumerable.Empty<BlockObject>())
	                {
						await db.ExecuteAsync("INSERT INTO 'BlockObject' ('BlockIndex','Id','Hash','Type') VALUES (@BlockIndex,@Id,@Hash,@Type);",
							new
							{
								BlockIndex = index,
								@object.Index,
								@object.Timestamp,
								Data = _serializer.Serialize(@object),
								@object.Hash
							}, t);
					}

                    foreach (var transaction in block.Transactions ?? Enumerable.Empty<Transaction>())
                    {
                        await db.ExecuteAsync("INSERT INTO 'BlockTransaction' ('BlockIndex','Id','Hash','Type') VALUES (@BlockIndex,@Id,@Hash,@Type);",
                        new
                        {
                            BlockIndex = index,
                            transaction.Id,
                            transaction.Hash,
                            transaction.Type
                        }, t);

                        foreach (var input in transaction.Data?.Inputs ?? Enumerable.Empty<TransactionItem>())
                        {
                            await db.ExecuteAsync("INSERT INTO 'BlockTransactionItem' ('TransactionId','Type','Index','Address','Amount','Signature') VALUES (@TransactionId,@Type,@Index,@Address,@Amount,@Signature);",
                                new
                                {
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
							await db.ExecuteAsync("INSERT INTO 'BlockTransactionItem' ('TransactionId','Type','Index','Address','Amount','Signature') VALUES (@TransactionId,@Type,@Index,@Address,@Amount,@Signature);",
								new
								{
									output.TransactionId,
									Type = TransactionDataType.Output,
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
				                   "ORDER BY b.'Index', bo.'Index', bo.'SourceId', bo.'Version'";

				var objects = db.Query<BlockObject>(sql, buffered: false);

				return objects;
			}
		}

	    public IEnumerable<CurrencyBlock> StreamAllBlocks()
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                const string sql = "SELECT b.* " +
                                   "FROM 'Block' b " +
                                   "ORDER BY b.'Index' ASC";

                foreach (var block in db.Query<CurrencyBlock>(sql, buffered: false))
                {
                    TransformBlockAsync(block, db).ConfigureAwait(false).GetAwaiter().GetResult();

                    yield return block;
                }
            }
        }

        public async Task<CurrencyBlock> GetByIndexAsync(long index)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
	            const string sql = "SELECT b.* FROM 'Block' b WHERE b.'Index' = @Index";

	            var block = await db.QuerySingleOrDefaultAsync<CurrencyBlock>(sql, new {Index = index});

                await TransformBlockAsync(block, db);

                return block;
            }
        }

        public async Task<CurrencyBlock> GetByHashAsync(string hash)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
	            const string sql = "SELECT b.* FROM 'Block' b WHERE b.'Hash' = @Hash";

	            var block = await db.QuerySingleOrDefaultAsync<CurrencyBlock>(sql, new { Hash = hash });

                await TransformBlockAsync(block, db);

                return block;
            }
        }

        public async Task<CurrencyBlock> GetLastBlockAsync()
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
	            const string sql = "SELECT COUNT(1), b.* " +
	                               "FROM 'Block' b " +
	                               "GROUP BY b.'Index' " +
	                               "ORDER BY b.'Index' DESC LIMIT 1";

	            var block = await db.QuerySingleOrDefaultAsync<CurrencyBlock>(sql);

                await TransformBlockAsync(block, db);
                
                return block;
            }
        }

        private static async Task TransformBlockAsync(CurrencyBlock block, IDbConnection db)
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
			
	        block.Transactions = (await db.QueryAsync<Transaction>(transactionsSql, new { block.Index })).AsList();
			
			foreach (var transaction in block.Transactions)
            {
	            const string transactionItemSql = "SELECT i.* " +
	                                              "FROM 'BlockTransactionItem' i " +
	                                              "WHERE i.Type = @Type AND i.TransactionId = @Id";

				transaction.Data = new TransactionData
                {
                    Inputs = (await db.QueryAsync<TransactionItem>(transactionItemSql,
                        new { Type = TransactionDataType.Input, transaction.Id })).AsList(),

                    Outputs = (await db.QueryAsync<TransactionItem>(transactionItemSql,
                        new { Type = TransactionDataType.Output, transaction.Id })).AsList()
                };
            }
        }

	    public override void MigrateToLatest()
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
	'SourceId' VARCHAR(64) NOT NULL,
	'Version' INTEGER NOT NULL,
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
    'TransactionId' VARCHAR(64), 
    'Type' INTEGER NOT NULL,
    'Index' INTEGER NOT NULL,
    'Amount' INTEGER NOT NULL,
    'Address' BLOB NOT NULL,
    'Signature' BLOB NULL
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
