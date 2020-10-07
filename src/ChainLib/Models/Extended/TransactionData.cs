using System.Collections.Generic;

namespace ChainLib.Models.Extended
{
	public class TransactionData
	{
		public IList<TransactionItem> Inputs { get; set; }
		public IList<TransactionItem> Outputs { get; set; }
	}
}