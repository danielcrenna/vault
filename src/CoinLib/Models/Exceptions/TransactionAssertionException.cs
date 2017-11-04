using System;

namespace CoinLib.Models.Exceptions
{
    public class TransactionAssertionException : Exception
    {
		public TransactionAssertionException(string message) : base(message) { }

	    public TransactionAssertionException(string message, Exception inner) : base(message, inner) { }
	}
}