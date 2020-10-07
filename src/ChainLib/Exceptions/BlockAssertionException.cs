using System;

namespace ChainLib.Exceptions
{
    public class BlockAssertionException : Exception
    {
		public BlockAssertionException(string message) : base(message) { }

	    public BlockAssertionException(string message, Exception inner) : base(message, inner) { }
	}
}