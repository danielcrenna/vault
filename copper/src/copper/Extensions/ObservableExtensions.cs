using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace copper.Extensions
{
    internal static class ObservableExtensions
    {
        /// <summary>
        /// Executes the delegate continuously until cancelled by the subscriber
        /// <remarks>
        /// It's important to add an additional buffer or window to this to avoid busy waiting
        /// </remarks>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="delegate"></param>
        /// <returns></returns>
        public static IObservable<T> AsContinuousObservable<T>(this Func<T> @delegate)
        {
            return new Func<CancellationToken, T>(token => @delegate()).AsContinuousObservable();
        }

        /// <summary>
        /// Executes the delegate continuously until cancelled by the subscriber
        /// <remarks>
        /// It's important to add an additional buffer or window to this to avoid busy waiting
        /// </remarks>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="delegate"></param>
        /// <returns></returns>
        public static IObservable<T> AsContinuousObservable<T>(this Func<IEnumerable<T>> @delegate)
        {
            return Observable.Create<T>((observer, cancelToken) => Task.Factory.StartNew(() =>
            {
                if(!cancelToken.IsCancellationRequested)
                {
                    var items = @delegate();
                    foreach (var item in items)
                    {
                        observer.OnNext(item);
                    }
                }
                cancelToken.ThrowIfCancellationRequested();
            })).Repeat();
        }

        /// <summary>
        /// Executes the delegate continuously until cancelled by the subscriber
        /// <remarks>
        /// It's important to add an additional buffer or window to this to avoid busy waiting
        /// </remarks>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="delegate"></param>
        /// <returns></returns>
        public static IObservable<T> AsContinuousObservable<T>(this Func<CancellationToken, T> @delegate)
        {
            return Observable.Create<T>((observer, cancelToken) => Task.Factory.StartNew(() =>
            {
                if(!cancelToken.IsCancellationRequested)
                {
                    var items = @delegate(cancelToken);
                    observer.OnNext(items);
                }
                cancelToken.ThrowIfCancellationRequested();
            })).Repeat();
        }
    }
}