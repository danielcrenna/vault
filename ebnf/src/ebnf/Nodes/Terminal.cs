using System;
using System.Diagnostics;

namespace ebnf.Nodes
{
    [DebuggerDisplay("Terminal: {Value}")]
    public class Terminal : Factor, IEquatable<Terminal>
    {
        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }

        public bool Equals(Terminal other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Value, Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Terminal)) return false;
            return Equals((Terminal) obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }

        public static bool operator ==(Terminal left, Terminal right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Terminal left, Terminal right)
        {
            return !Equals(left, right);
        }
    }
}