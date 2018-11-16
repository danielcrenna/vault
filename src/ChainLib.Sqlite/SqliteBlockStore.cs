using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ChainLib.Models;
using ChainLib.Serialization;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Sodium;

namespace ChainLib.Sqlite
{
    public class SqliteBlockStore : SqliteRepository, IBlockStore
    {
	    private readonly Block _genesisBlock;
	    private readonly IBlockObjectTypeProvider _typeProvider;
	    private readonly ILogger<SqliteBlockStore> _logger;

        public SqliteBlockStore(
			string baseDirectory,
			string subDirectory, 
			string databaseName,
			Block genesisBlock,
			IBlockObjectTypeProvider typeProvider,
			ILogger<SqliteBlockStore> logger) : base(baseDirectory, subDirectory, databaseName, logger)
        {
	        _genesisBlock = genesisBlock;
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

        public async Task AddAsync(Block block)
        {
	        byte[] data = SerializeObjects(block);
			
			using (var db = new SqliteConnection($"Data Source={DataFile}"))
			{
				await db.OpenAsync();

				using (var t = db.BeginTransaction())
				{
					var index = await db.QuerySingleAsync<long>(
						"INSERT INTO 'Block' ('Version','PreviousHash','MerkleRootHash','Timestamp','Difficulty','Nonce','Hash','Data') VALUES (@Version,@PreviousHash,@MerkleRootHash,@Timestamp,@Difficulty,@Nonce,@Hash,@Data); " +
						"SELECT LAST_INSERT_ROWID();", new
						{
							block.Version,
							block.PreviousHash,
							block.MerkleRootHash,
							block.Timestamp,
							block.Difficulty,
							block.Nonce,
							block.Hash,
							Data = data
						}, t);

					t.Commit();

					block.Index = index;
				}
			}
        }

	    public IEnumerable<BlockObject> StreamAllBlockObjects(bool forwards, long startingFrom = 0)
	    {
		    using (var db = new SqliteConnection($"Data Source={DataFile}"))
		    {
			    const string sql = "SELECT b.* " +
			                       "FROM 'Block' b " +
			                       "WHERE b.'Index' >= @startingFrom " +
								   "ORDER BY b.'Index' ASC";

			    foreach (var block in db.Query<BlockResult>(sql, new { startingFrom }, buffered: false))
			    {
				    DeserializeObjects(block, block.Data);

				    foreach (var @object in block.Objects)
				    {
					    yield return @object;
				    }
			    }
		    }
		}

	    public IEnumerable<BlockHeader> StreamAllBlockHeaders(bool forwards, long startingFrom = 0)
	    {
			using (var db = new SqliteConnection($"Data Source={DataFile}"))
			{
				const string sql = "SELECT b.'Version', b.'PreviousHash', b.'MerkleRootHash', b.'Timestamp', b.'Difficulty', b.'Nonce'" +
				                   "FROM 'Block' b " +
				                   "WHERE b.'Index' >= @startingFrom " +
				                   "ORDER BY b.'Index' ASC";

				foreach (var header in db.Query<BlockHeader>(sql, new { startingFrom }, buffered: false))
				{
					yield return header;
				}
			}
		}

	    public IEnumerable<Block> StreamAllBlocks(bool forwards, long startingFrom = 0)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                const string sql = "SELECT b.* " +
                                   "FROM 'Block' b " +
                                   "WHERE b.'Index' >= @startingFrom " +
								   "ORDER BY b.'Index' ASC";

                foreach (var block in db.Query<BlockResult>(sql, new { startingFrom }, buffered: false))
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
	'Version' INTEGER NOT NULL,
    'PreviousHash' VARCHAR(64) NOT NULL, 
	'MerkleRootHash' VARCHAR(64) NOT NULL,
    'Timestamp' INTEGER NOT NULL,
	'Difficulty' INTEGER NOT NULL,
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
					// Version:
				    var context = new BlockSerializeContext(bw, _typeProvider);

				    if (context.typeProvider.SecretKey != null)
				    {
						// Nonce:
					    var nonce = StreamEncryption.GenerateNonceChaCha20();
						context.bw.WriteBuffer(nonce);

					    // Data:
						using (var ems = new MemoryStream())
					    {
						    using (var ebw = new BinaryWriter(ems, Encoding.UTF8))
						    {
							    var ec = new BlockSerializeContext(ebw, _typeProvider, context.Version);
							    block.SerializeObjects(ec);
							    context.bw.WriteBuffer(StreamEncryption.EncryptChaCha20(ems.ToArray(), nonce, ec.typeProvider.SecretKey));
						    }
					    }
				    }
				    else
				    {
						// Data:
					    context.bw.Write(false);
					    block.SerializeObjects(context);
					}

				    data = ms.ToArray();
				}
		    }

		    return data;
	    }

	    private void DeserializeObjects(Block block, byte[] data)
	    {
		    using (var ms = new MemoryStream(data))
		    {
			    using (var br = new BinaryReader(ms))
			    {
				    // Version:
					var context = new BlockDeserializeContext(br, _typeProvider);

					// Nonce:
				    var nonce = context.br.ReadBuffer();
					if(nonce != null)
					{
						// Data:
						using (var dms = new MemoryStream(StreamEncryption.EncryptChaCha20(context.br.ReadBuffer(), nonce, _typeProvider.SecretKey)))
						{
							using (var dbr = new BinaryReader(dms))
							{
								var dc = new BlockDeserializeContext(dbr, _typeProvider);
								block.DeserializeObjects(dc);
							}
						}
					}
					else
					{
						// Data:
						block.DeserializeObjects(context);
					}
				}
		    }
		}
	}
}
