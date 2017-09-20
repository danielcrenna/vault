using NaiveCoin.Core.Helpers;
using NaiveCoin.Core.Providers;
using System;
using System.Collections.Generic;

namespace NaiveCoin.Models
{
    public class Block
    {
        public long? Index { get; set; }
        public string PreviousHash { get; set; }
        public long? Timestamp { get; set; }
        public long? Nonce { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
        public string Hash { get; set; }

        public string ToHash(IObjectHashProvider provider)
        {
            return CryptoUtil.Hash($"{Index}{PreviousHash}{Timestamp}{provider.ComputeHash(Transactions)}{Nonce}");
        }
        
        public double GetDifficulty()
        {
            // 14 is the maximum precision length supported by javascript
            return Convert.ToByte(Hash.Substring(0, 14), 16);
        }
    }
}
