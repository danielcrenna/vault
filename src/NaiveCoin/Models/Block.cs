using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace NaiveCoin.Models
{
    public class Block
    {
        public long? Index { get; set; }
        public string PreviousHash { get; set; }
        public long? Timestamp { get; set; }
        public long? Nonce { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
		public ICollection<BlockObject> Objects { get; set; }
        public string Hash { get; set; }
        
        public double GetDifficulty()
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(Hash));

            // 14 is the maximum precision length supported by javascript
            return Convert.ToByte(Hash.Substring(0, 14), 16);
        }
    }
}
