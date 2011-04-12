using System;

namespace metrics.Core
{
    /// <summary>
    /// A hash key for storing metrics associated by the parent class and name pair
    /// </summary>
    public struct MetricName
    {
        public Type Class { get; private set; }
        public string Name { get; private set; }

        public MetricName(Type @class, string name) : this()
        {
            Class = @class;
            Name = name;
        }

        public bool Equals(MetricName other)
        {
            return Equals(other.Name, Name) && Equals(other.Class, Class);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == typeof (MetricName) && Equals((MetricName) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0)*397) ^ (Class != null ? Class.GetHashCode() : 0);
            }
        }

        public static bool operator ==(MetricName left, MetricName right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MetricName left, MetricName right)
        {
            return !left.Equals(right);
        }
    }
}



