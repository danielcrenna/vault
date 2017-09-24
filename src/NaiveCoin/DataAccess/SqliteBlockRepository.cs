using System.Collections.Generic;
using System.Data;
using System.Linq;
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

        public SqliteBlockRepository(string @namespace, ILogger<SqliteBlockRepository> logger) : base(@namespace, "blocks", logger)
        {
            _logger = logger;
        }

        public long GetLength()
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                return db.QuerySingleOrDefault<long?>("SELECT MAX(b.'Index') FROM 'Block' b").GetValueOrDefault(0L);
            }
        }

        public Block GetByTransactionId(string transactionId)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                var block = db.QuerySingleOrDefault<Block>("SELECT b.* FROM 'Block' b " +
                                                           "LEFT JOIN 'BlockTransaction' bt ON bt.'BlockIndex' = b.'Index' " +
                                                           "WHERE bt.'Id' = @Id", new { Id = transactionId });

                TransformBlock(block, db);

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
        
        public IEnumerable<TransactionItem> GetTransactionItemsForAddress(TransactionDataType type, string address)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                const string sql = "SELECT d.* " +
                                   "FROM 'BlockTransactionData' d " +
                                   "WHERE d.'Address' = @Address " +
                                   "AND d.'Type' = @Type";

                var outputs = db.Query<TransactionItem>(sql, new
                {
                    Address = address.FromHex(),
                    Type = type
                });

                return outputs;
            }
        }

        public IEnumerable<string> GetAllTransactionIds()
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                const string sql = "SELECT bt.'Id' " +
                                   "FROM 'Block' b " +
                                   "INNER JOIN 'BlockTransaction' bt ON bt.'BlockIndex' = b.'Index'";

                var ids = db.Query<string>(sql, buffered: false);

                return ids;
            }
        }

        public IEnumerable<Block> GetAll()
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                const string sql = "SELECT b.* " +
                                   "FROM 'Block' b " +
                                   "ORDER BY b.'Index' ASC";

                foreach (var block in db.Query<Block>(sql, buffered: false))
                {
                    TransformBlock(block, db);

                    yield return block;
                }
            }
        }

        public Block GetByIndex(long index)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                var block = db.QuerySingleOrDefault<Block>("SELECT b.* FROM 'Block' b WHERE b.'Index' = @Index", new {Index = index});

                TransformBlock(block, db);

                return block;
            }
        }

        public Block GetByHash(string hash)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                var block = db.QuerySingleOrDefault<Block>("SELECT b.* FROM 'Block' b WHERE b.'Hash' = @Hash", new { Hash = hash });

                TransformBlock(block, db);

                return block;
            }
        }

        public Block GetLastBlock()
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                var block = db.QuerySingleOrDefault<Block>("SELECT COUNT(1), b.* " +
                                                           "FROM 'Block' b " +
                                                           "GROUP BY b.'Index' " +
                                                           "ORDER BY b.'Index' DESC LIMIT 1");

                TransformBlock(block, db);
                
                return block;
            }
        }

        private static void TransformBlock(Block block, IDbConnection db)
        {
            if (block == null)
                return;

            block.Transactions = new List<Transaction>();

            var transactions = db.Query<Transaction>("SELECT t.* " +
                                                     "FROM 'BlockTransaction' t " +
                                                     "WHERE t.'BlockIndex' = @Index", new { block.Index }, buffered: false);

            foreach (var transaction in transactions)
            {
                transaction.Data = new TransactionData
                {
                    Inputs = db.Query<TransactionItem>("SELECT i.* " +
                                                       "FROM 'BlockTransactionData' i " +
                                                       "WHERE i.Type = @Type AND i.TransactionId = @Id",
                        new { Type = TransactionDataType.Input, transaction.Id }).ToArray(),

                    Outputs = db.Query<TransactionItem>("SELECT o.* " +
                                                        "FROM 'BlockTransactionData' o " +
                                                        "WHERE o.Type = @Type AND o.TransactionId = @Id",
                        new { Type = TransactionDataType.Output, transaction.Id }).ToArray(),
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

CREATE TABLE IF NOT EXISTS 'BlockTransaction'
(  
    'BlockIndex' INTEGER NOT NULL,

    'Id' VARCHAR(64) NOT NULL PRIMARY KEY, 
    'Hash' VARCHAR(64) NOT NULL,
    'Type' INTEGER NOT NULL,

    FOREIGN KEY('BlockIndex') REFERENCES Block('Index')
);

CREATE TABLE IF NOT EXISTS 'BlockTransactionData'
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
                _logger?.LogError(e, "Error migrating blocks table");
                throw;
            }
        }
    }
}
