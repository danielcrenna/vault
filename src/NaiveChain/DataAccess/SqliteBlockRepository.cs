using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NaiveChain.Extensions;
using NaiveChain.Models;
using NaiveChain.Serialization;

namespace NaiveChain.DataAccess
{
    public class SqliteBlockRepository : SqliteRepository, IBlockRepository<Block>
    {
	    private readonly Block _genesisBlock;
	    private readonly IHashProvider _hashProvider;
	    private readonly IBlockObjectTypeProvider _typeProvider;
	    private readonly ILogger<SqliteBlockRepository> _logger;

        public SqliteBlockRepository(
			string @namespace, 
			string databaseName,
			Block genesisBlock,
			IHashProvider hashProvider,
			IBlockObjectTypeProvider typeProvider,
			ILogger<SqliteBlockRepository> logger) : base(@namespace, databaseName, logger)
        {
	        _genesisBlock = genesisBlock;
	        _hashProvider = hashProvider;
	        _typeProvider = typeProvider;
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

	            var block = await db.QuerySingleOrDefaultAsync<BlockResult>(sql, new { Id = transactionId });

				DeserializeObjects(block, block.Data);

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
						"INSERT INTO 'Block' ('PreviousHash','Timestamp','Nonce','Hash','Data') VALUES (@PreviousHash,@Timestamp,@Nonce,@Hash,@Data); " +
						"SELECT LAST_INSERT_ROWID();", new
						{
							block.PreviousHash,
							block.Timestamp,
							block.Nonce,
							block.Hash,
							Data = SerializeObjects(block)
						}, t);

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

                foreach (var block in db.Query<BlockResult>(sql, buffered: false))
                {
	                DeserializeObjects(block, block.Data);

                    yield return block;
                }
            }
        }

        public async Task<Block> GetByIndexAsync(long index)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
	            const string sql = "SELECT b.* FROM 'Block' b WHERE b.'Index' = @Index";

	            var block = await db.QuerySingleOrDefaultAsync<BlockResult>(sql, new {Index = index});

				DeserializeObjects(block, block.Data);

				return block;
            }
        }
		
	    public async Task<Block> GetByHashAsync(byte[] hash)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
	            const string sql = "SELECT b.* FROM 'Block' b WHERE b.'Hash' = @Hash";

	            var block = await db.QuerySingleOrDefaultAsync<BlockResult>(sql, new { Hash = hash });

                DeserializeObjects(block, block.Data);

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

	            var block = await db.QuerySingleOrDefaultAsync<BlockResult>(sql);

                DeserializeObjects(block, block.Data);
                
                return block;
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
    'Hash' VARCHAR(64) UNIQUE NOT NULL,
	'Data' BLOB NOT NULL
);");
                }
            }
            catch (SqliteException e)
            {
                _logger?.LogError(e, "Error migrating blocks database");
                throw;
            }
        }

	    public class BlockResult : Block
	    {
		    public byte[] Data { get; set; }
	    }

	    private byte[] SerializeObjects(Block block)
	    {
		    byte[] data;
		    using (var ms = new MemoryStream())
		    {
			    using (var bw = new BinaryWriter(ms, Encoding.UTF8))
			    {
				    var context = new BlockSerializeContext(bw, _typeProvider);
				    block.SerializeObjects(context);
			    }
			    data = ms.ToArray();
		    }
		    return data;
	    }

	    private void DeserializeObjects(Block block, byte[] data)
	    {
		    using (var ms = new MemoryStream(data))
		    {
			    using (var br = new BinaryReader(ms))
			    {
				    var context = new BlockDeserializeContext(br, _typeProvider);
				    block.DeserializeObjects(context);
				    block.Hash = block.ToHashBytes(_hashProvider);
			    }
		    }
	    }
	}
}
