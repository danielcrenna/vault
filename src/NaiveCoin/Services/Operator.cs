using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NaiveCoin.Models;
using NaiveCoin.Wallets;
using NaiveCoin.Core.Helpers;

namespace NaiveCoin.Services
{
    public class Operator
    {
        private readonly Blockchain _blockchain;
        private readonly IWalletProvider _provider;
        private readonly IWalletRepository _wallets;
        private readonly CoinSettings _coinSettings;
        private readonly ILogger<Operator> _logger;

        public Operator(Blockchain blockchain, IWalletProvider provider, IWalletRepository wallets, IOptions<CoinSettings> coinSettings, ILogger<Operator> logger)
        {
            _blockchain = blockchain;
            _provider = provider;
            _wallets = wallets;
            _coinSettings = coinSettings.Value;
            _logger = logger;
        }

        public Wallet AddWallet(Wallet wallet)
        {
            return _wallets.Add(wallet);
        }
        
        public Wallet CreateWalletFromPassword(string password)
        {
            return _wallets.CreateFromPassword(password);
        }

        public Wallet CreateWalletFromHash(string passwordHash)
        {
            return _wallets.CreateFromPasswordHash(passwordHash);
        }

        public bool CheckWalletPassword(string id, string passwordHash)
        {
            var wallet = GetWalletById(id);
            return wallet != null && wallet.PasswordHash == passwordHash;
        }

        public IEnumerable<Wallet> GetWallets()
        {
            return _wallets.GetAll();
        }

        public Wallet GetWalletById(string id)
        {
            return _wallets.GetById(id);
        }

        public string GenerateAddressForWallet(string id)
        {
            var wallet = GetWalletById(id);
            if (wallet == null)
                throw new ArgumentException($"Wallet not found with id '{id}'");

            var address = _provider.GenerateAddress(wallet);

            _wallets.SaveAddresses(wallet);

            return address;
        }

        public IEnumerable<string> GetAddressesForWallet(string id)
        {
            var wallet = GetWalletById(id);
            if (wallet == null)
                throw new ArgumentException($"Wallet not found with id '{id}'");

            var addresses = wallet.GetAddresses();
            return addresses;
        }

        public string GetAddressForWallet(string walletId, string addressId)
        {
            var wallet = GetWalletById(walletId);
            if (wallet == null)
                throw new ArgumentException($"Wallet not found with id '{walletId}'");

            var addressFound = wallet.GetAddressByPublicKey(addressId);
            if (addressFound == null)
                throw new ArgumentException($"Address not found with id '{addressId}' for wallet { walletId}");

            return addressFound;
        }

        public long GetBalanceForWalletAddress(string id, string addressId)
        {
            var address = GetAddressForWallet(id, addressId);
            var utxo = _blockchain.GetUnspentTransactionsForAddress(address);
            return utxo.Sum(x => x.Amount);
        }

        public Transaction CreateTransaction(string walletId, byte[] fromAddress, byte[] toAddress, long amount, byte[] changeAddress)
        {
            var utxo = _blockchain.GetUnspentTransactionsForAddress(fromAddress.ToHex());
            var wallet = GetWalletById(walletId);

            if (wallet == null)
                throw new ArgumentException($"Wallet not found with id '{walletId}'");

            var secretKey = wallet.GetSecretKeyByAddress(fromAddress.ToHex());
            if (secretKey == null)
                throw new ArgumentException($"Secret key not found with Wallet id '${walletId}' and address '${fromAddress}'");

            var tx = new TransactionBuilder();
            tx.From(utxo)
                .To(toAddress, amount)
                .Change(changeAddress ?? fromAddress)
                .Fee(_coinSettings.FeePerTransaction)
                .Sign(secretKey);

            return tx.Build();
        }
    }
}