using System;

namespace copper
{
    /// <summary>
    /// A base implementation for a producer that uses the default background producer as its worker thread.
    /// <remarks>
    /// See implementers for reference implementation; basically you subscribe a production to the Background directly in the constructor
    /// </remarks>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UsesBackgroundProducer<T> : Produces<T>, IDisposable
    {
        protected BackgroundProducer<T> Background { get; private set; }

        protected UsesBackgroundProducer()
        {
            Background = new BackgroundProducer<T>();
        }

        public virtual void Attach(Consumes<T> consumer)
        {
            Background.Attach(consumer);
        }

        public virtual void Start()
        {
            Background.Start();
        }

        public virtual void Stop()
        {
            Background.Stop();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (Background == null) return;
            Background.Dispose();
            Background = null;
        }
    }
}