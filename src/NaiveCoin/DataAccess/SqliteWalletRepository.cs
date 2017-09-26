using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NaiveChain.DataAccess;
using NaiveCoin.Wallets;
using NaiveCoin.Core.Models;

namespace NaiveCoin.DataAccess
{
    public class SqliteWalletRepository : SqliteRepository, IWalletRepository
    {
	    private readonly IWalletAddressProvider _addressProvider;
        private readonly ILogger<SqliteWalletRepository> _logger;

        public SqliteWalletRepository(string @namespace, string databaseName, IWalletAddressProvider addressProvider, ILogger<SqliteWalletRepository> logger) : base(@namespace, databaseName, logger)
        {
	        _addressProvider = addressProvider;
            _logger = logger;
        }

        public async Task<IEnumerable<Wallet>> GetAllAsync()
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                const string sql = "SELECT w.*, a.* FROM 'Wallet' w " +
                                   "LEFT JOIN 'Address' a ON a.'WalletId' = w.'Id' " +
                                   "ORDER BY a.'Index' ASC";

                var wallets = new Dictionary<string, Wallet>();

                await db.QueryAsync<Wallet, KeyPair, Wallet>(sql, (parent, child) =>
                {
                    if (!wallets.ContainsKey(parent.Id))
                        wallets.Add(parent.Id, parent);

                    wallets[parent.Id].KeyPairs.Add(child);
                    return wallets[parent.Id];
                }, splitOn: "WalletId");

                return wallets.Values;
            }
        }

        public async Task<Wallet> GetByIdAsync(string id)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                const string sql = "SELECT w.*, a.* FROM 'Wallet' w " +
                                   "LEFT JOIN 'Address' a ON a.'WalletId' = w.'Id' " +
                                   "WHERE w.'Id' = @Id " +
                                   "ORDER BY a.'Index' ASC";

                Wallet wallet = null;

                await db.QueryAsync<Wallet, KeyPair, Wallet>(sql, (parent, child) =>
                {
                    if (wallet == null)
                        wallet = parent;
                    wallet.KeyPairs.Add(child);
                    return wallet;
                }, new { Id = id }, splitOn: "WalletId");

                return wallet;
            }
        }

        public async Task<Wallet> AddAsync(Wallet wallet)
        {
            if (wallet.KeyPairs.Count == 0)
            {
                _addressProvider.GenerateAddress(wallet);
            }

            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                await db.OpenAsync();

                using (var t = db.BeginTransaction())
                {
                    await db.ExecuteAsync("INSERT INTO Wallet (Id,PasswordHash,Secret) VALUES (@Id,@PasswordHash,@Secret)", wallet, t);

                    await SaveAddressesInTransactionAsync(wallet, db, t);

                    t.Commit();
                }
            }

            return wallet;
        }

        public async Task SaveAddressesAsync(Wallet wallet)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                await db.OpenAsync();

                using (var t = db.BeginTransaction())
                {
                    await SaveAddressesInTransactionAsync(wallet, db, t);
                }
            }
        }

        private static async Task SaveAddressesInTransactionAsync(Wallet wallet, IDbConnection db, IDbTransaction t)
        {
            await db.ExecuteAsync("DELETE FROM 'Address' WHERE 'WalletId' = @Id", wallet, t);

            foreach (var keyPair in wallet.KeyPairs)
            {
                await db.ExecuteAsync("INSERT INTO Address ('WalletId','Index','PrivateKey','PublicKey') VALUES (@WalletId,@Index,@PrivateKey,@PublicKey)",
                new
                {
                    WalletId = wallet.Id,
                    keyPair.Index,
                    keyPair.PrivateKey,
                    keyPair.PublicKey
                }, t);
            }
        }

        protected override void MigrateToLatest()
        {
            try
            {
                using (var db = new SqliteConnection($"Data Source={DataFile}"))
                {
                    db.Execute(@"
CREATE TABLE IF NOT EXISTS 'Wallet'
(  
    'Id' VARCHAR(64) NOT NULL PRIMARY KEY, 
    'PasswordHash' VARCHAR(64) NOT NULL,
    'Secret' VARCHAR(1024) NOT NULL
);

CREATE TABLE IF NOT EXISTS 'Address'
(  
    'WalletId' VARCHAR(64) NOT NULL,
    'Index' INTEGER NOT NULL, 
    'PrivateKey' VARCHAR(1024) NOT NULL,
    'PublicKey' VARCHAR(64) NOT NULL,

    FOREIGN KEY('WalletId') REFERENCES Wallet('Id')
);");
                }
            }
            catch (SqliteException e)
            {
                _logger?.LogError(e, "Error migrating wallets database");
                throw;
            }
        }
    }
}