using System;

namespace copper
{
    /// <summary>
    /// An observer that handles the next item in a sequence using a delegate
    /// <see href="http://blogs.msdn.com/b/pfxteam/archive/2010/04/06/9990420.aspx"/>
    /// <seealso href="http://code.msdn.microsoft.com/ParExtSamples"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DelegatingObserver<T> : IObserver<T>
    {
        private readonly Action<T> _onNext;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;

        public DelegatingObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            if (onNext == null) throw new ArgumentNullException("onNext");
            if (onError == null) throw new ArgumentNullException("onError");
            if (onCompleted == null) throw new ArgumentNullException("onCompleted");
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        public void OnCompleted() { _onCompleted(); }
        public void OnError(Exception error) { _onError(error); }
        public void OnNext(T value) { _onNext(value); }
    }
}