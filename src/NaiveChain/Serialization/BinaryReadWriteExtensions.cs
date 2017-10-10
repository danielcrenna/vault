using System.IO;

namespace NaiveChain.Serialization
{
	public static class BinaryReadWriteExtensions
	{
		public static bool WriteBoolean(this BinaryWriter bw, bool value)
		{
			bw.Write(value);
			return value; // Returning the written value allows for easy null checks
		}

		public static void WriteNullableString(this BinaryWriter bw, string value)
		{
			if (bw.WriteBoolean(value != null))
				bw.Write(value);
		}

		public static string ReadNullableString(this BinaryReader br)
		{
			if (br.ReadBoolean())
				return br.ReadString();
			else
				return null;
		}

		public static void WriteNullableSingle(this BinaryWriter bw, float? value)
		{
			if (bw.WriteBoolean(value.HasValue))
				bw.Write(value.Value);
		}

		public static float? ReadNullableSingle(this BinaryReader br)
		{
			if (br.ReadBoolean())
				return br.ReadSingle();
			else
				return null;
		}

		public static void WriteNullableLong(this BinaryWriter bw, long? value)
		{
			if (bw.WriteBoolean(value.HasValue))
				bw.Write(value.Value);
		}

		public static long? ReadNullableLong(this BinaryReader br)
		{
			if (br.ReadBoolean())
				return br.ReadInt64();
			else
				return null;
		}

		public static void WriteBuffer(this BinaryWriter bw, byte[] buffer)
		{
			bw.Write(buffer.Length);
			bw.Write(buffer);
		}

		public static byte[] ReadBuffer(this BinaryReader br)
		{
			var length = br.ReadInt32();
			var buffer = br.ReadBytes(length);
			return buffer;
		}
	}
}