using System;

namespace pratt
{
    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {

        }
    }
}
