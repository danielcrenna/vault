using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;

#if Smartphone
using TweetSharp.Core.Attributes;
#endif

#if SILVERLIGHT
using System.Reflection;
#endif

namespace TweetSharp
{
#if !SILVERLIGHT
    /// <summary>
    /// Represents a normalized date from the Twitter API. 
    /// </summary>
    [Serializable]
#endif
#if !Smartphone && !NET20
    [DataContract]
#endif
    public class TwitterDateTime : ITwitterModel
    {
        private static readonly IDictionary<string, string> _map =
            new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the Twitter-based date format.
        /// </summary>
        /// <value>The format.</value>
        public virtual TwitterDateFormat Format { get; private set; }

        /// <summary>
        /// Gets or sets the actual date time.
        /// </summary>
        /// <value>The date time.</value>
        public virtual DateTime DateTime { get; private set; }

#if !SILVERLIGHT && !Smartphone && !NET20
        static readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();
#else
        private static readonly object _lock = new object();
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterDateTime"/> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="format">The format.</param>
        public TwitterDateTime(DateTime dateTime, TwitterDateFormat format)
        {
            Format = format;
            DateTime = dateTime;
        }

#if Smartphone || SILVERLIGHT
        private static readonly IList<string> _names = new List<string>();
#endif

        /// <summary>
        /// Converts from date time.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static string ConvertFromDateTime(DateTime input, TwitterDateFormat format)
        {
            EnsureDateFormatsAreMapped();

#if !SILVERLIGHT && !Smartphone
            var name = Enum.GetName(typeof(TwitterDateFormat), format);
#else
            EnsureEnumNamesAreMapped(typeof (TwitterDateFormat));
            var name = _names[_names.IndexOf(format.ToString())];
#endif
            GetReadLockOnMap();
            var value = _map[name];
            ReleaseReadLockOnMap();

            value = value.Replace(" zzzzz", " +0000");

            var converted = input.ToString(value, CultureInfo.InvariantCulture);
            return converted;
        }

        /// <summary>
        /// Converts to date time.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(string input)
        {
            EnsureDateFormatsAreMapped();
            GetReadLockOnMap();
            var formats = _map.Values;
            ReleaseReadLockOnMap();
            foreach (var format in formats)
            {
                DateTime date;
#if !Smartphone
                if (DateTime.TryParseExact(input, format,
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.AdjustToUniversal, out date))
#else
                if (TryParseDateTime(input, format,
                                     CultureInfo.InvariantCulture,
                                     DateTimeStyles.AdjustToUniversal, out date))
#endif
                {
                    return date;
                }
            }

            return default(DateTime);
        }

        /// <summary>
        /// Converts to twitter date time.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static TwitterDateTime ConvertToTwitterDateTime(string input)
        {
            EnsureDateFormatsAreMapped();
            GetReadLockOnMap();
            try
            {
                foreach (var format in _map)
                {
                    DateTime date;
#if !Smartphone
                    if (DateTime.TryParseExact(input, format.Value,
                                               CultureInfo.InvariantCulture,
                                               DateTimeStyles.AdjustToUniversal, out date))
#else
                if (TryParseDateTime(input, format.Value,
                                     CultureInfo.InvariantCulture,
                                     DateTimeStyles.AdjustToUniversal, out date))
#endif
                    {
                        var kind = Enum.Parse(typeof (TwitterDateFormat), format.Key, true);
                        return new TwitterDateTime(date, (TwitterDateFormat) kind);
                    }
                }

                return default(TwitterDateTime);
            }
            finally
            {
                ReleaseReadLockOnMap();
            }
        }

        private static void EnsureDateFormatsAreMapped()
        {
            var type = typeof (TwitterDateFormat);
#if !SILVERLIGHT && !Smartphone
            var names = Enum.GetNames(type);
#else
            EnsureEnumNamesAreMapped(type);
            var names = _names;
#endif
            GetReadLockOnMap();
            try
            {
                foreach (var name in names)
                {
                    if (_map.ContainsKey(name))
                    {
                        continue;
                    }
                    GetWriteLockOnMap();
                    try
                    {
                        var fi = typeof (TwitterDateFormat).GetField(name);
                        var attributes = fi.GetCustomAttributes(typeof (DescriptionAttribute), false);
                        var format = (DescriptionAttribute) attributes[0];

                        _map.Add(name, format.Description);
                    }
                    finally
                    {
                        ReleaseWriteLockOnMap();
                    }
                }
            }
            finally
            {
                ReleaseReadLockOnMap();
            }
        }


        private static void GetReadLockOnMap()
        {
#if !SILVERLIGHT && !Smartphone && !NET20
            _readerWriterLock.EnterUpgradeableReadLock();
#else
            Monitor.Enter(_lock);
#endif
        }

        private static void ReleaseReadLockOnMap()
        {
#if !SILVERLIGHT && !Smartphone && !NET20
            _readerWriterLock.ExitUpgradeableReadLock();
#else
            Monitor.Exit(_lock);
#endif
        }

        private static void GetWriteLockOnMap()
        {
#if !SILVERLIGHT && !Smartphone && !NET20
            _readerWriterLock.EnterWriteLock();
#else
            //already have exclusive access
#endif
        }

        private static void ReleaseWriteLockOnMap()
        {
#if !SILVERLIGHT && !Smartphone && !NET20
            _readerWriterLock.ExitWriteLock();
#else
            //will exit when we give up read lock
#endif
        }

#if Smartphone || SILVERLIGHT
        private static bool TryParseDateTime(string input, string format, IFormatProvider provider,
                                             DateTimeStyles styles, out DateTime result)
        {
            try
            {
                result = DateTime.ParseExact(input, format, provider, styles);
                return true;
            }
            catch (Exception)
            {
                result = default(DateTime);
                return false;
            }
        }

        private static void EnsureEnumNamesAreMapped(Type type)
        {
            GetReadLockOnMap();
            GetWriteLockOnMap();
            try
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var field in fields)
                {
                    if (_names.Contains(field.Name))
                    {
                        continue;
                    }
                    _names.Add(field.Name);
                }
            }
            finally
            {
                ReleaseWriteLockOnMap();
                ReleaseReadLockOnMap();
            }
        }
#endif

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ConvertFromDateTime(DateTime, Format);
        }

#if !Smartphone && !NET20
        [DataMember]
#endif
        public virtual string RawSource { get; set; }
    }
}