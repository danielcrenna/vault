using System;
using System.Collections.Generic;

namespace NaiveCoin.Core.Models
{
    public class KeyPair : IEquatable<KeyPair>
	{
        public int Index { get; }
        public byte[] PublicKey { get; }
        public byte[] PrivateKey { get; }

        internal KeyPair() { /* required for serialization */ }

        public KeyPair(int index, byte[] publicKey, byte[] privateKey)
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
				   EqualityComparer<byte[]>.Default.Equals(PublicKey, other.PublicKey) &&
				   EqualityComparer<byte[]>.Default.Equals(PrivateKey, other.PrivateKey);
		}

		public override int GetHashCode()
		{
			var hashCode = 605523053;
			hashCode = hashCode * -1521134295 + Index.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(PublicKey);
			hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(PrivateKey);
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