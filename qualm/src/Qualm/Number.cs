using System;
using System.Diagnostics;

namespace Qualm
{
    /// <summary>
    /// A floating-point number.
    /// </summary>
    [DebuggerDisplay("{_key}")]
    public struct Number : IComparable
    {
        private readonly ValueType _key;

        private Number(ValueType key) : this()
        {
            _key = key;
        }

#if single_precision
        public static implicit operator Number(float value)
        {
            return new Number(value);
        }
        
        public static implicit operator Number(double value)
        {
            return new Number((float)value);
        }

        public static implicit operator float(Number value)
        {
            if (value._key == null)
            {
                return default(float);
            }
            return (float)value._key;
        }

         public int CompareTo(object obj)
        {
            if(obj is Number)
            {
                var key = (float)((Number) obj)._key;
                return key.CompareTo((float)_key);
            }
            return -1;
        }
#else
        public static implicit operator Number(double value)
        {
            return new Number(value);
        }

        public static implicit operator Number(float value)
        {
            return new Number((double)value);
        }

        public static implicit operator double(Number value)
        {
            if (value._key == null)
            {
                return default(double);
            }
            return (double)value._key;
        }

        public int CompareTo(object obj)
        {
            if(obj is Number)
            {
                var key = (double)((Number) obj)._key;
                return key.CompareTo((double)_key);
            }
            return -1;
        }
#endif

        public bool Equals(Number other)
        {
            return other._key.Equals(_key);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Number && Equals((Number) obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
        
        public static bool operator ==(Number left, Number right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Number left, Number right)
        {
            return !left.Equals(right);
        }
    }
}