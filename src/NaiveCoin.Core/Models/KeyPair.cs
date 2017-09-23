using System;
using System.Collections.Generic;

namespace NaiveCoin.Core.Models
{
    public class KeyPair : IEquatable<KeyPair>
    {
        public int Index { get; }
        public string PublicKey { get; }
        public string PrivateKey { get; }

        internal KeyPair() { /* required for serialization */ }

        public KeyPair(int index, string publicKey, string privateKey)
        {
            Index = index;
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as KeyPair);
        }

        public bool Equals(KeyPair other)
        {
            return other != null &&
                   Index == other.Index &&
                   PublicKey == other.PublicKey &&
                   PrivateKey == other.PrivateKey;
        }

        public override int GetHashCode()
        {
            var hashCode = -1787115702;
            hashCode = hashCode * -1521134295 + Index.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PublicKey);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PrivateKey);
            return hashCode;
        }

        public static bool operator ==(KeyPair pair1, KeyPair pair2)
        {
            return EqualityComparer<KeyPair>.Default.Equals(pair1, pair2);
        }

        public static bool operator !=(KeyPair pair1, KeyPair pair2)
        {
            return !(pair1 == pair2);
        }
    }
}