using System;
using System.Diagnostics;
using System.IO;

namespace ChainLib.Serialization
{
	internal class MemoryCompareStream : Stream
	{
		public MemoryCompareStream(byte[] compareTo)
		{
			this.compareTo = compareTo;
			this.position = 0;
		}

		byte[] compareTo;
		long position;

		public override void Write(byte[] buffer, int offset, int count)
		{
			for (int i = 0; i < count; i++)
			{
				if (buffer[offset + i] != compareTo[position + i])
				{

					Debug.Assert(false);
					throw new Exception("Data mismatch");
				}
			}

			position += count;
		}

		public override void WriteByte(byte value)
		{
			if (compareTo[position] != value)
			{
				Debug.Assert(false);
				throw new Exception("Data mismatch");
			}

			position++;
		}


		#region Boring Stream Stuff

		public override bool CanRead { get { return false; } }
		public override bool CanSeek { get { return true; } }
		public override bool CanWrite { get { return true; } }
		public override void Flush() { }
		public override long Length { get { return compareTo.Length; } }
		public override long Position { get { return position; } set { position = value; } }

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new InvalidOperationException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					position = offset;
					break;
				case SeekOrigin.Current:
					position += offset;
					break;
				case SeekOrigin.End:
					position = compareTo.Length - offset;
					break;
			}
			return Position;
		}

		public override void SetLength(long value)
		{
			throw new InvalidOperationException();
		}

		#endregion

	}
}