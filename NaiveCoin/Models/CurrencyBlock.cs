using System.Collections.Generic;
using NaiveChain;
using NaiveChain.Models;

namespace NaiveCoin.Models
{
	public class CurrencyBlock : Block
	{
		public ICollection<Transaction> Transactions { get; set; }
	}
}