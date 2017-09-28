using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NaiveChain.Models;

namespace NaiveChain.DataAccess
{
    public class SqliteBlockRepository : SqliteRepository, IBlockRepository<Block>
    {
	    private readonly Block _genesisBlock;
	    private readonly IBlockObjectSerializer _serializer;
	    private readonly ILogger<SqliteBlockRepository> _logger;

        public SqliteBlockRepository(
			string @namespace, 
			string databaseName,
			Block genesisBlock,
			IBlockObjectSerializer serializer,
			ILogger<SqliteBlockRepository> logger) : base(@namespace, databaseName, logger)
        {
	        _genesisBlock = genesisBlock;
	        _serializer = serializer;
	        _logger = logger;
        }

	    public Task<Block> GetGenesisBlockAsync()
	    {
		    return Task.FromResult(_genesisBlock);
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

        public async Task AddAsync(Block block)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                await db.OpenAsync();

                using (var t = db.BeginTransaction())
                {
                    var index = await db.QuerySingleAsync<long>(
                        "INSERT INTO 'Block' ('PreviousHash','Timestamp','Nonce','Hash') VALUES (@PreviousHash,@Timestamp,@Nonce,@Hash); " +
                        "SELECT LAST_INSERT_ROWID();", block, t);

	                foreach (var @object in block.Objects)
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

                    t.Commit();

                    block.Index = index;
                }
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
