using System;

namespace Hammock.Validation
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class ValidationException : Exception
    {
        public ValidationException()
        {

        }

        public ValidationException(string message) : base(message)
        {

        }
    }
}