using System;

namespace linger
{
    [Serializable]
    public class DelegateJob
    {
        private readonly Func<bool> _delegate;

        public DelegateJob(Func<bool> @delegate)
        {
            _delegate = @delegate;
        }

        public bool Perform()
        {
            return _delegate();
        }
    }
}