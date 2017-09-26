using System.Collections.Generic;
using NaiveChain;

namespace NaiveCoin.Models
{
	public class CurrencyBlock : Block
	{
		public ICollection<Transaction> Transactions { get; set; }
	}
}