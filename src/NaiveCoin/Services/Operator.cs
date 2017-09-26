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

        public Wallet CreateWalletFromPassword(string password)
        {
            return _provider.Create(password);
        }

		public async Task<Wallet> AddWalletAsync(Wallet wallet)
		{
			return await _walletRepository.AddAsync(wallet);
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
            return await _walletRepository.GetAllAsync();
        }

        public async Task<Wallet> GetWalletByIdAsync(string id)
        {
            return await _walletRepository.GetByIdAsync(id);
        }

        public async Task<string> GenerateAddressForWalletAsync(string id)
        {
            var wallet = await GetWalletByIdAsync(id);
            if (wallet == null)
                throw new ArgumentException($"Wallet not found with id '{id}'");

            var address = _provider.GenerateAddress(wallet);

            await _walletRepository.SaveAddressesAsync(wallet);

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

            var addressFound = wallet.GetAddressByPublicKey(addressId.FromHex());
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

        public async Task<Transaction> CreateTransactionAsync(string walletId, byte[] fromAddress, byte[] toAddress, long amount, byte[] changeAddress)
        {
            var utxo = await _blockchain.GetUnspentTransactionsForAddressAsync(fromAddress.ToHex());
            var wallet = await GetWalletByIdAsync(walletId);

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