using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NaiveChain.Models;
using NaiveCoin.Core;

namespace NaiveCoin.Models
{
    public class TransactionBuilder
    {
        private readonly IHashProvider _hashProvider;
        private byte[] _secretKey;
        private TransactionType _type;
        private long _feeAmount;
        private byte[] _changeAddress;
        private byte[] _outputAddress;
        private long? _totalAmount;
        private IEnumerable<TransactionItem> _utxo;

        public TransactionBuilder(IHashProvider hashProvider)
        {
            _hashProvider = hashProvider;
            _type = TransactionType.Regular;
        }

        public TransactionBuilder From(IEnumerable<TransactionItem> utxo)
        {
            _utxo = utxo;
            return this;
        }

        public TransactionBuilder To(byte[] address, long amount)
        {
            _outputAddress = address;
            _totalAmount = amount;
            return this;
        }

        public TransactionBuilder Change(byte[] changeAddress)
        {
            _changeAddress = changeAddress;
            return this;
        }

        public TransactionBuilder Fee(long amount)
        {
            _feeAmount = amount;
            return this;
        }

        public TransactionBuilder Sign(string secretKey)
        {
            _secretKey = secretKey.FromHex();
            return this;
        }

	    public TransactionBuilder Sign(byte[] secretKey)
	    {
		    _secretKey = secretKey;
		    return this;
	    }


		public TransactionBuilder Type(TransactionType type)
        {
            _type = type;
            return this;
        }

        public Transaction Build()
        {
            // Check required information
            if (_utxo == null)
                throw new ArgumentException($"It's necessary to provide a list of unspent output transactions.");
            if (_outputAddress == null)
                throw new ArgumentException($"It's necessary to provide the destination address.");
            if (_totalAmount == null)
                throw new ArgumentException($"It's necessary to provide the transaction value.");

			// Calculates the change amount
			var changeAmount = _utxo.Sum(x => x.Amount) - _totalAmount - _feeAmount;

			var transactionId = CryptoUtil.RandomString();

			// For each transaction input, calculates the hash of the input and signs the data
	        var inputIndex = 1;
			var inputs = _utxo.Select(utxo =>
            {
                var keyPair = Ed25519.GenerateKeyPairFromSecret(_secretKey);
	            var hash = _hashProvider.ComputeHash(new
	            {
		            Transaction = utxo.TransactionId,
		            utxo.Index,
		            utxo.Address
	            });
	            utxo.Signature = Ed25519.Sign(keyPair, hash);
                return utxo;
            }).Select(x => new TransactionItem
			{
				TransactionId = x.TransactionId,
				Index = x.Index,
				Address = x.Address,
				Amount = x.Amount,
				Signature = x.Signature,
				Type = TransactionDataType.Input
			}).AsList();

            // Add target receiver
	        var outputIndex = 1;
            var outputs = new List<TransactionItem>
            {
                new TransactionItem
                {
					Index = outputIndex++,
					TransactionId = transactionId,
                    Amount = _totalAmount.Value,
                    Address = _outputAddress,
					Type = TransactionDataType.Output
                }
            };

            // Add change amount
            if (changeAmount > 0)
            {
                outputs.Add(new TransactionItem
                {
					Index = outputIndex,
					TransactionId = transactionId,
                    Amount = changeAmount.GetValueOrDefault(),
                    Address = _changeAddress,
					Type = TransactionDataType.Output
                });
            }

            // The remaining value is the fee to be collected by the block's creator
	        var transaction = new Transaction
	        {
		        Id = transactionId,
		        Type = _type,
		        Data = new TransactionData
		        {
			        Inputs = inputs,
			        Outputs = outputs.AsList()
		        }
	        };

	        transaction.Hash = transaction.ToHash(_hashProvider);

	        return transaction;
        }
    }
}