using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using NaiveCoin.Core.Helpers;

namespace NaiveChain.Models
{
    public class Block
    {
        public long? Index { get; set; }
        public string PreviousHash { get; set; }
        public long? Timestamp { get; set; }
        public long? Nonce { get; set; }
        public ICollection<BlockObject> Objects { get; set; }
        public string Hash { get; set; }
        
        public double GetDifficulty()
        {
	        byte[] array = Hash.FromHex();
			return BitConverter.ToDouble(array, 24); // take last 8 bytes, not 14 like the original
        }
    }
}
