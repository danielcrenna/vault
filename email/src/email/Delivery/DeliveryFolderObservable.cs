using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

namespace email.Delivery
{
    /// <summary>
    /// A micro-wrapper around the <see cref="FileSystemWatcher"/> to make it behave with observables
    /// </summary>
    public class DeliveryFolderObservable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly IObservable<EventPattern<FileSystemEventArgs>> _observable;

        public DeliveryFolderObservable(string directory, string filter, bool includeSubdirectories)
        {
            _watcher = new FileSystemWatcher(directory, filter)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = includeSubdirectories,
                InternalBufferSize = 50000
            };

            _observable = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => _watcher.Created += h, h => _watcher.Created -= h);
        }

        public void Subscribe(Action<EventPattern<FileSystemEventArgs>> onNext)
        {
            _observable.Subscribe(onNext);
        }
    }
}