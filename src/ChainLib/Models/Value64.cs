using System.Runtime.InteropServices;

namespace ChainLib.Models
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Value64
	{
		public uint v1, v2;

		public static bool operator == (Value64 a, Value64 b)
		{
			return a.v1 == b.v1 && a.v2 == b.v2;
		}

		public static bool operator != (Value64 a, Value64 b)
		{
			return !(a == b);
		}

		public byte[] ToBytes()
		{
			byte[] result = new byte[4 * 4];

			result[0] = (byte)(v1 >> (0 * 8));
			result[1] = (byte)(v1 >> (1 * 8));
			result[2] = (byte)(v1 >> (2 * 8));
			result[3] = (byte)(v1 >> (3 * 8));

			result[4] = (byte)(v2 >> (0 * 8));
			result[5] = (byte)(v2 >> (1 * 8));
			result[6] = (byte)(v2 >> (2 * 8));
			result[7] = (byte)(v2 >> (3 * 8));

			return result;
		}
		
		public override bool Equals(object obj)
		{
			if (obj is Value64 value64)
				return value64 == this;
			return false;
		}

		public override int GetHashCode()
		{
			return (int)(v1 ^ v2);
		}

		public override string ToString()
		{
			return string.Format("{0:X8}{1:X8}", v1, v2);
		}
	}
}