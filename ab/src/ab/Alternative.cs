using System.Collections.Generic;

namespace ab
{
    public class Alternative
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public double Value { get; set; }
        public double ZScore { get; set; }
        public double Probability { get; set; }
        public double Difference { get; set; }

        protected bool Equals(Alternative other)
        {
            return Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Alternative) obj);
        }

        public override int GetHashCode()
        {
            return Index;
        }

        public static bool operator ==(Alternative left, Alternative right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Alternative left, Alternative right)
        {
            return !Equals(left, right);
        }

        private sealed class IndexEqualityComparer : IEqualityComparer<Alternative>
        {
            public bool Equals(Alternative x, Alternative y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Index == y.Index;
            }

            public int GetHashCode(Alternative obj)
            {
                return obj.Index;
            }
        }

        private static readonly IEqualityComparer<Alternative> IndexComparerInstance = new IndexEqualityComparer();

        public static IEqualityComparer<Alternative> IndexComparer
        {
            get { return IndexComparerInstance; }
        }
    }
}