using System;

namespace NaiveCoin.Models.Exceptions
{
    public class BlockAssertionException : Exception
    {
        public BlockAssertionException(string message) : base(message)
        {

        }
    }
}