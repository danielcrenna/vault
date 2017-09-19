using System.Collections.Generic;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NaiveCoin.Models;

namespace NaiveCoin.DataAccess
{
    public class SqliteWalletRepository : SqliteRepository, IWalletRepository
    {
        private readonly IWalletProvider _provider;
        private readonly ILogger<SqliteWalletRepository> _logger;

        public SqliteWalletRepository(string @namespace, IWalletProvider provider, ILogger<SqliteWalletRepository> logger) : base(@namespace, "wallets", logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public IEnumerable<Wallet> GetAll()
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                const string sql = "SELECT w.*, a.* FROM 'Wallet' w " +
                                   "LEFT JOIN 'Address' a ON a.'WalletId' = w.'Id' " +
                                   "ORDER BY a.'Index' ASC";

                var wallets = new Dictionary<string, Wallet>();

                db.Query<Wallet, KeyPair, Wallet>(sql, (parent, child) =>
                {
                    if (!wallets.ContainsKey(parent.Id))
                        wallets.Add(parent.Id, parent);

                    wallets[parent.Id].KeyPairs.Add(child);
                    return wallets[parent.Id];
                }, splitOn: "WalletId");

                return wallets.Values;
            }
        }

        public Wallet GetById(string id)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                const string sql = "SELECT w.*, a.* FROM 'Wallet' w " +
                                   "LEFT JOIN 'Address' a ON a.'WalletId' = w.'Id' " +
                                   "WHERE w.'Id' = @Id " +
                                   "ORDER BY a.'Index' ASC";

                Wallet wallet = null;

                db.Query<Wallet, KeyPair, Wallet>(sql, (parent, child) =>
                {
                    if (wallet == null)
                        wallet = parent;
                    wallet.KeyPairs.Add(child);
                    return wallet;
                }, new { Id = id }, splitOn: "WalletId");

                return wallet;
            }
        }

        public Wallet CreateFromPassword(string password)
        {
            var wallet = _provider.CreateFromPassword(password);

            return Add(wallet);
        }

        public Wallet CreateFromPasswordHash(string passwordHash)
        {
            var wallet = _provider.CreateFromPasswordHash(passwordHash);

            return Add(wallet);
        }

        public Wallet Add(Wallet wallet)
        {
            if (wallet.KeyPairs.Count == 0)
            {
                _provider.GenerateAddress(wallet);
            }

            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                db.Open();

                using (var t = db.BeginTransaction())
                {
                    db.Execute("INSERT INTO Wallet (Id,PasswordHash,Secret) VALUES (@Id,@PasswordHash,@Secret)", wallet, t);

                    SaveAddressesInTransaction(wallet, db, t);

                    t.Commit();
                }
            }

            return wallet;
        }

        public void SaveAddresses(Wallet wallet)
        {
            using (var db = new SqliteConnection($"Data Source={DataFile}"))
            {
                db.Open();

                using (var t = db.BeginTransaction())
                {
                    SaveAddressesInTransaction(wallet, db, t);
                }
            }
        }

        private static void SaveAddressesInTransaction(Wallet wallet, SqliteConnection db, SqliteTransaction t)
        {
            db.Execute("DELETE FROM 'Address' WHERE 'WalletId' = @Id", wallet, t);

            foreach (var keyPair in wallet.KeyPairs)
            {
                db.Execute("INSERT INTO Address ('WalletId','Index','SecretKey','PublicKey') VALUES (@WalletId,@Index,@SecretKey,@PublicKey)",
                new
                {
                    WalletId = wallet.Id,
                    keyPair.Index,
                    keyPair.SecretKey,
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
    'SecretKey' VARCHAR(1024) NOT NULL,
    'PublicKey' VARCHAR(64) NOT NULL,

    FOREIGN KEY('WalletId') REFERENCES Wallet('Id')
);");
                }
            }
            catch (SqliteException e)
            {
                _logger?.LogError(e, "Error migrating wallets table");
                throw;
            }
        }
    }
}