using System.Collections.Generic;
using ChainLib;
using ChainLib.Models;

namespace CoinLib.Models
{
	public class CurrencyBlock : Block
	{
		public ICollection<Transaction> Transactions { get; set; }
	}
}