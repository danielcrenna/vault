using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NaiveCoin.Core.Helpers;
using NaiveCoin.Core.Providers;
using NaiveCoin.Models;
using NaiveCoin.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaiveCoin.Services
{
	public class Operator
    {
        private readonly Blockchain _blockchain;
        private readonly IHashProvider _hashProvider;
        private readonly IWalletProvider _walletProvider;
        private readonly CoinSettings _coinSettings;
        private readonly ILogger<Operator> _logger;

        public Operator(Blockchain blockchain, 
            IHashProvider hashProvider,
            IWalletProvider walletProvider, 
            IOptions<CoinSettings> coinSettings, 
            ILogger<Operator> logger)
        {
            _blockchain = blockchain;
            _hashProvider = hashProvider;
            _walletProvider = walletProvider;
            _coinSettings = coinSettings.Value;
            _logger = logger;
        }

        public Wallet CreateWalletFromPassword(string password)
        {
            return _walletProvider.Create(password);
        }

		public async Task<Wallet> AddWalletAsync(Wallet wallet)
		{
			return await _walletProvider.AddAsync(wallet);
		}

		public async Task<Wallet> CheckWalletPasswordAsync(string id, string password)
        {
            var wallet = await GetWalletByIdAsync(id);
            if (wallet != null && PasswordUtil.Verify(password, wallet.PasswordHash))
                return wallet;
            return null;
        }

        public async Task<IEnumerable<Wallet>> GetWalletsAsync()
        {
            return await _walletProvider.GetAllAsync();
        }

        public async Task<Wallet> GetWalletByIdAsync(string id)
        {
            return await _walletProvider.GetByIdAsync(id);
        }

        public async Task<string> GenerateAddressForWalletAsync(string id)
        {
            var wallet = await GetWalletByIdAsync(id);
            if (wallet == null)
                throw new ArgumentException($"Wallet not found with id '{id}'");

            var address = _walletProvider.GenerateAddress(wallet);

            await _walletProvider.SaveAddressesAsync(wallet);

            return address;
        }

        public async Task<IEnumerable<string>> GetAddressesForWalletAsync(string id)
        {
            var wallet = await GetWalletByIdAsync(id);
            if (wallet == null)
                throw new ArgumentException($"Wallet not found with id '{id}'");

            var addresses = wallet.GetAddresses();

	        return addresses.Select(x => x.ToHex());
        }

        public async Task<string> GetAddressForWalletAsync(string walletId, string addressId)
        {
            var wallet = await GetWalletByIdAsync(walletId);
            if (wallet == null)
                throw new ArgumentException($"Wallet not found with id '{walletId}'");

	        var publicKey = addressId.FromHex();
	        var addressFound = wallet.GetAddressByPublicKey(publicKey);
            if (addressFound == null)
                throw new ArgumentException($"Address not found with id '{addressId}' for wallet { walletId}");

            return addressFound.ToHex();
        }

        public async Task<long> GetBalanceForWalletAddressAsync(string id, string addressId)
        {
            var address = await GetAddressForWalletAsync(id, addressId);
            var utxo = await _blockchain.GetUnspentTransactionsForAddressAsync(address);
            return utxo.Sum(x => x.Amount);
        }

        public async Task<Transaction> CreateTransactionAsync(string walletId, byte[] fromAddress, byte[] toAddress, long amount, byte[] changeAddress = null)
        {
            var utxo = await _blockchain.GetUnspentTransactionsForAddressAsync(fromAddress.ToHex());
            var wallet = await GetWalletByIdAsync(walletId);

            if (wallet == null)
                throw new ArgumentException($"Wallet not found with id '{walletId}'");

            var secretKey = wallet.GetPrivateKeyByAddress(fromAddress);
            if (secretKey == null)
                throw new ArgumentException($"Secret key not found with Wallet id '{walletId}' and address '{fromAddress}'");

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