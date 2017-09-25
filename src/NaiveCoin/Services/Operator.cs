using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NaiveCoin.Models;
using NaiveCoin.Wallets;
using NaiveCoin.Core.Helpers;
using NaiveCoin.Core.Providers;

namespace NaiveCoin.Services
{
    public class Operator
    {
        private readonly Blockchain _blockchain;
        private readonly IHashProvider _hashProvider;
        private readonly IWalletProvider _provider;
        private readonly IWalletRepository _walletRepository;
        private readonly CoinSettings _coinSettings;
        private readonly ILogger<Operator> _logger;

        public Operator(Blockchain blockchain, 
            IHashProvider hashProvider,
            IWalletProvider walletProvider, 
            IWalletRepository walletRepository, 
            IOptions<CoinSettings> coinSettings, 
            ILogger<Operator> logger)
        {
            _blockchain = blockchain;
            _hashProvider = hashProvider;
            _provider = walletProvider;
            _walletRepository = walletRepository;
            _coinSettings = coinSettings.Value;
            _logger = logger;
        }

        public Wallet AddWallet(Wallet wallet)
        {
            return _walletRepository.Add(wallet);
        }
        
        public Wallet CreateWalletFromPassword(string password)
        {
            return _provider.Create(password);
        }
        
        public Wallet CheckWalletPassword(string id, string password)
        {
            var wallet = GetWalletById(id);
            if (wallet != null && PasswordUtil.Verify(password, wallet.PasswordHash))
                return wallet;
            return null;
        }

        public IEnumerable<Wallet> GetWallets()
        {
            return _walletRepository.GetAll();
        }

        public Wallet GetWalletById(string id)
        {
            return _walletRepository.GetById(id);
        }

        public string GenerateAddressForWallet(string id)
        {
            var wallet = GetWalletById(id);
            if (wallet == null)
                throw new ArgumentException($"Wallet not found with id '{id}'");

            var address = _provider.GenerateAddress(wallet);

            _walletRepository.SaveAddresses(wallet);

            return address;
        }

        public IEnumerable<string> GetAddressesForWallet(string id)
        {
            var wallet = GetWalletById(id);
            if (wallet == null)
                throw new ArgumentException($"Wallet not found with id '{id}'");

            var addresses = wallet.GetAddresses();

	        return addresses.Select(x => x.ToHex());
        }

        public string GetAddressForWallet(string walletId, string addressId)
        {
            var wallet = GetWalletById(walletId);
            if (wallet == null)
                throw new ArgumentException($"Wallet not found with id '{walletId}'");

            var addressFound = wallet.GetAddressByPublicKey(addressId.FromHex());
            if (addressFound == null)
                throw new ArgumentException($"Address not found with id '{addressId}' for wallet { walletId}");

            return addressFound.ToHex();
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

            var secretKey = wallet.GetPrivateKeyByAddress(fromAddress);
            if (secretKey == null)
                throw new ArgumentException($"Secret key not found with Wallet id '${walletId}' and address '${fromAddress}'");

            var tx = new TransactionBuilder(_hashProvider);
            tx.From(utxo)
                .To(toAddress, amount)
                .Change(changeAddress ?? fromAddress)
                .Fee(_coinSettings.FeePerTransaction)
                .Sign(secretKey.ToHex());

            return tx.Build();
        }
    }
}