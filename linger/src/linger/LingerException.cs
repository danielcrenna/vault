using System;

namespace linger
{
    [Serializable]
    public class LingerException : Exception
    {
        public LingerException(string message) : base(message)
        {
            
        }
    }
}