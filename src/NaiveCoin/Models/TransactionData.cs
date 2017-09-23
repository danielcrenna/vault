using System.Linq;
using NaiveCoin.Models.Exceptions;
using NaiveCoin.Core.Helpers;

namespace NaiveCoin.Models
{
    public class TransactionData
    {
        public TransactionInput[] Inputs { get; set; }
        public TransactionOutput[] Outputs { get; set; }
        
        public void Check(CoinSettings coinSettings)
        {
            // Check if the signature of all input transactions are correct (transaction data is signed by the public key of the address)
            var checks = Inputs.Select(input =>
            {
                var hash = CryptoUtil.ObjectHashBytes(new
                {
                    Transaction = input.TransactionId,
                    input.Index,
                    input.Address
                });

                return new
                {
                    valid = CryptoEdDsaUtil.VerifySignature(input.Address, input.Signature, hash),
                    input
                };
            });

            foreach (var check in checks)
            {
                if (!check.valid)
                {
                    throw new TransactionAssertionException($"Invalid transaction input signature at index '{check.input.Index}'");
                }
            }

            // Check if the sum of input transactions are greater than output transactions, it needs to leave some room for the transaction fee
            var sumOfInputsAmount = Inputs.Sum(x => x.Amount);
            var sumOfOutputsAmount = Outputs.Sum(x => x.Amount);
            if (sumOfInputsAmount >= sumOfOutputsAmount)
            {
                throw new TransactionAssertionException($"Invalid transaction balance: inputs sum '{sumOfInputsAmount}', outputs sum '{sumOfOutputsAmount}'");
            }

            // Check if there is enough to cover the transaction fee
            var isEnoughFee = sumOfInputsAmount - sumOfOutputsAmount >= coinSettings.FeePerTransaction; 
            if (!isEnoughFee)
            {
                throw new TransactionAssertionException($"Not enough fee: expected '{coinSettings.FeePerTransaction}' got '{sumOfInputsAmount - sumOfOutputsAmount}'");
            }
        }
    }
}